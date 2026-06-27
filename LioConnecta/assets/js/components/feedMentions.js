import { escapeHtml } from "./html.js";
import { suggestFeedMentions } from "../services/feedService.js?v=0.21.4";
import { getPortalAuthHeaders } from "../services/portalAuthService.js?v=0.17.0";

const mentionState = new WeakMap();
const suggestTimers = new WeakMap();
const suggestControllers = new WeakMap();

const DROPDOWN_SELECTOR = ".feed-mention-dropdown";
const CHIP_SELECTOR = ".feed-mention-chip";
const DROPDOWN_WIDTH = 280;
const DROPDOWN_GAP = 6;

export function renderMentionBody(content = {}) {
  const text = String(content.text ?? content ?? "");
  const mentions = Array.isArray(content.mentions) ? content.mentions : [];

  if (!mentions.length) {
    return escapeHtml(text);
  }

  const tokens = mentions
    .map((mention) => ({
      token: `@${String(mention.displayName || "")}`,
      displayName: String(mention.displayName || "")
    }))
    .filter((item) => item.displayName)
    .sort((left, right) => right.token.length - left.token.length);

  let parts = [{ type: "text", value: text }];

  for (const { token, displayName } of tokens) {
    const next = [];
    for (const part of parts) {
      if (part.type !== "text") {
        next.push(part);
        continue;
      }

      const segments = part.value.split(token);
      segments.forEach((segment, index) => {
        if (segment) {
          next.push({ type: "text", value: segment });
        }
        if (index < segments.length - 1) {
          next.push({ type: "mention", displayName });
        }
      });
    }
    parts = next;
  }

  return parts.map((part) => (
    part.type === "mention"
      ? `<span class="post-comment-mention">@${escapeHtml(part.displayName)}</span>`
      : escapeHtml(part.value)
  )).join("");
}

function parseMentionSuggestions(payload = {}) {
  const items = Array.isArray(payload?.items)
    ? payload.items
    : Array.isArray(payload?.Items)
      ? payload.Items
      : [];

  return items.map((item) => ({
    userId: String(item.userId || item.UserId || item.user_id || ""),
    displayName: String(item.displayName || item.DisplayName || item.display_name || ""),
    department: String(item.department || item.Department || "Companhia")
  })).filter((item) => item.userId && item.displayName);
}

function getState(fieldRoot) {
  if (!mentionState.has(fieldRoot)) {
    mentionState.set(fieldRoot, {
      activeIndex: -1,
      suggestions: []
    });
  }
  return mentionState.get(fieldRoot);
}

function getDropdown(fieldRoot) {
  return fieldRoot.querySelector(DROPDOWN_SELECTOR);
}

function getCaretClientRect(editor) {
  const selection = window.getSelection();
  if (!selection || selection.rangeCount === 0) {
    return null;
  }

  const range = selection.getRangeAt(0).cloneRange();
  if (!editor.contains(range.startContainer)) {
    return null;
  }

  range.collapse(true);

  const rects = range.getClientRects();
  if (rects.length > 0) {
    return rects[rects.length - 1];
  }

  const marker = document.createElement("span");
  marker.textContent = "\u200b";
  range.insertNode(marker);
  const rect = marker.getBoundingClientRect();
  marker.remove();

  selection.removeAllRanges();
  selection.addRange(range);

  if (rect.width === 0 && rect.height === 0) {
    return null;
  }

  return rect;
}

function resetMentionDropdownPosition(dropdown) {
  if (!dropdown) {
    return;
  }

  dropdown.style.top = "";
  dropdown.style.left = "";
  dropdown.style.width = "";
  dropdown.style.right = "";
}

function positionMentionDropdown(fieldRoot, editor) {
  const dropdown = getDropdown(fieldRoot);
  if (!dropdown || dropdown.hidden) {
    return;
  }

  const caretRect = getCaretClientRect(editor);
  if (!caretRect) {
    return;
  }

  const fieldRect = fieldRoot.getBoundingClientRect();
  const dropdownWidth = Math.min(DROPDOWN_WIDTH, Math.max(0, fieldRect.width - 8));
  const top = caretRect.bottom - fieldRect.top + DROPDOWN_GAP;
  const left = Math.max(0, Math.min(
    caretRect.left - fieldRect.left,
    fieldRect.width - dropdownWidth
  ));

  dropdown.style.top = `${top}px`;
  dropdown.style.left = `${left}px`;
  dropdown.style.width = `${dropdownWidth}px`;
  dropdown.style.right = "auto";
}

function scheduleMentionDropdownPosition(fieldRoot, editor) {
  requestAnimationFrame(() => {
    positionMentionDropdown(fieldRoot, editor);
  });
}

function nodeToPlainText(node) {
  if (!node) {
    return "";
  }

  if (node.nodeType === Node.TEXT_NODE) {
    return node.textContent || "";
  }

  if (node.nodeType === Node.ELEMENT_NODE) {
    if (node.matches?.(CHIP_SELECTOR)) {
      return `@${node.getAttribute("data-display-name") || ""}`;
    }

    return Array.from(node.childNodes).map(nodeToPlainText).join("");
  }

  if (node instanceof DocumentFragment) {
    return Array.from(node.childNodes).map(nodeToPlainText).join("");
  }

  return "";
}

export function serializeEditorContent(editor) {
  const text = nodeToPlainText(editor).replace(/\u00a0/g, " ");
  const mentionedUserIds = Array.from(editor.querySelectorAll(CHIP_SELECTOR))
    .map((chip) => chip.getAttribute("data-user-id") || "")
    .filter(Boolean);

  return { text, mentionedUserIds };
}

function getTextBeforeCursor(editor) {
  const selection = window.getSelection();
  if (!selection || selection.rangeCount === 0 || !editor.contains(selection.anchorNode)) {
    return nodeToPlainText(editor);
  }

  const range = selection.getRangeAt(0);
  const preRange = range.cloneRange();
  preRange.selectNodeContents(editor);
  preRange.setEnd(range.endContainer, range.endOffset);
  return nodeToPlainText(preRange.cloneContents()).replace(/\u00a0/g, " ");
}

function getActiveMentionQuery(editor) {
  const before = getTextBeforeCursor(editor);
  const atIndex = before.lastIndexOf("@");

  if (atIndex === -1) {
    return null;
  }

  if (atIndex > 0) {
    const charBefore = before[atIndex - 1];
    if (charBefore !== " " && charBefore !== "\n" && charBefore !== "\t") {
      return null;
    }
  }

  const fragment = before.slice(atIndex + 1);
  if (fragment.includes("\n") || fragment.length > 80) {
    return null;
  }

  if (fragment.endsWith(" ") && fragment.trim().includes(" ")) {
    return null;
  }

  return { query: fragment, start: atIndex, end: before.length };
}

function visitPlainTextOffsets(editor, callback) {
  let offset = 0;

  const visit = (node) => {
    if (node.nodeType === Node.TEXT_NODE) {
      const length = (node.textContent || "").length;
      callback(node, offset, offset + length, "text");
      offset += length;
      return;
    }

    if (node.nodeType === Node.ELEMENT_NODE && node.matches(CHIP_SELECTOR)) {
      const token = `@${node.getAttribute("data-display-name") || ""}`;
      callback(node, offset, offset + token.length, "chip");
      offset += token.length;
      return;
    }

    Array.from(node.childNodes).forEach(visit);
  };

  visit(editor);
  return offset;
}

function plainTextOffsetsToRange(editor, start, end) {
  let startNode = null;
  let startOffset = 0;
  let endNode = null;
  let endOffset = 0;

  visitPlainTextOffsets(editor, (node, nodeStart, nodeEnd, type) => {
    if (startNode === null && start >= nodeStart && start <= nodeEnd) {
      if (type === "text") {
        startNode = node;
        startOffset = start - nodeStart;
      } else {
        startNode = node.parentNode;
        startOffset = Array.from(node.parentNode.childNodes).indexOf(node);
      }
    }

    if (endNode === null && end >= nodeStart && end <= nodeEnd) {
      if (type === "text") {
        endNode = node;
        endOffset = end - nodeStart;
      } else {
        endNode = node.parentNode;
        endOffset = Array.from(node.parentNode.childNodes).indexOf(node) + 1;
      }
    }
  });

  if (!startNode || !endNode) {
    return null;
  }

  const range = document.createRange();
  range.setStart(startNode, startOffset);
  range.setEnd(endNode, endOffset);
  return range;
}

function createMentionChip(userId, displayName) {
  const chip = document.createElement("span");
  chip.className = "feed-mention-chip";
  chip.contentEditable = "false";
  chip.setAttribute("data-user-id", userId);
  chip.setAttribute("data-display-name", displayName);
  chip.innerHTML = `
    <span class="feed-mention-chip__label">@${escapeHtml(displayName)}</span>
    <button
      type="button"
      class="feed-mention-chip__remove"
      data-action="remove-feed-mention"
      aria-label="Remover mencao de ${escapeHtml(displayName)}"
    >&times;</button>
  `;
  return chip;
}

function placeCaretAfter(node) {
  const selection = window.getSelection();
  if (!selection) {
    return;
  }

  const range = document.createRange();
  range.setStartAfter(node);
  range.collapse(true);
  selection.removeAllRanges();
  selection.addRange(range);
}

function removeMentionChip(chip) {
  if (!chip) {
    return;
  }

  const parent = chip.parentNode;
  chip.remove();

  if (parent && parent.childNodes.length === 0) {
    parent.appendChild(document.createTextNode(""));
  }
}

function getPreviousSignificantNode(node, offset) {
  if (node.nodeType === Node.TEXT_NODE) {
    if (offset > 0) {
      return null;
    }
    return node.previousSibling;
  }

  if (node.nodeType === Node.ELEMENT_NODE && offset > 0) {
    return node.childNodes[offset - 1] || null;
  }

  return null;
}

function getNextSignificantNode(node, offset) {
  if (node.nodeType === Node.TEXT_NODE) {
    if (offset < (node.textContent || "").length) {
      return null;
    }
    return node.nextSibling;
  }

  if (node.nodeType === Node.ELEMENT_NODE) {
    return node.childNodes[offset] || null;
  }

  return null;
}

function removeChipAdjacentToCaret(editor, direction) {
  const selection = window.getSelection();
  if (!selection || selection.rangeCount === 0 || !selection.isCollapsed) {
    return false;
  }

  const range = selection.getRangeAt(0);
  if (!editor.contains(range.startContainer)) {
    return false;
  }

  const target = direction === "backspace"
    ? getPreviousSignificantNode(range.startContainer, range.startOffset)
    : getNextSignificantNode(range.startContainer, range.startOffset);

  if (!target?.matches?.(CHIP_SELECTOR)) {
    return false;
  }

  removeMentionChip(target);
  return true;
}

function showMentionHint(fieldRoot, editor) {
  const dropdown = getDropdown(fieldRoot);
  if (!dropdown) {
    return;
  }

  dropdown.hidden = false;
  dropdown.innerHTML = `<p class="post-comment-mention-hint">Digite o nome do colaborador para buscar</p>`;
  scheduleMentionDropdownPosition(fieldRoot, editor);
}

function hideMentionDropdown(fieldRoot) {
  const dropdown = getDropdown(fieldRoot);
  if (dropdown) {
    dropdown.hidden = true;
    dropdown.innerHTML = "";
    resetMentionDropdownPosition(dropdown);
  }

  const state = getState(fieldRoot);
  state.activeIndex = -1;
  state.suggestions = [];
}

function renderMentionDropdown(fieldRoot, editor, suggestions, activeIndex, { message = "" } = {}) {
  const dropdown = getDropdown(fieldRoot);
  if (!dropdown) {
    return;
  }

  if (message) {
    dropdown.hidden = false;
    dropdown.innerHTML = `<p class="post-comment-mention-hint">${escapeHtml(message)}</p>`;
    scheduleMentionDropdownPosition(fieldRoot, editor);
    return;
  }

  if (!suggestions.length) {
    dropdown.hidden = true;
    dropdown.innerHTML = "";
    resetMentionDropdownPosition(dropdown);
    return;
  }

  dropdown.hidden = false;
  dropdown.innerHTML = suggestions.map((item, index) => `
    <button
      type="button"
      class="post-comment-mention-option ${index === activeIndex ? "is-active" : ""}"
      data-action="pick-feed-mention"
      data-user-id="${escapeHtml(item.userId)}"
      data-display-name="${escapeHtml(item.displayName)}"
      role="option"
      aria-selected="${index === activeIndex ? "true" : "false"}"
    >
      <span class="post-comment-mention-option__name">${escapeHtml(item.displayName)}</span>
      <span class="post-comment-mention-option__meta">${escapeHtml(item.department || "Companhia")}</span>
    </button>
  `).join("");
  scheduleMentionDropdownPosition(fieldRoot, editor);
}

async function loadMentionSuggestions(fieldRoot, editor, query) {
  const normalized = String(query || "").trim();
  if (!normalized) {
    showMentionHint(fieldRoot, editor);
    return;
  }

  const existingController = suggestControllers.get(fieldRoot);
  if (existingController) {
    existingController.abort();
  }

  const controller = new AbortController();
  suggestControllers.set(fieldRoot, controller);
  renderMentionDropdown(fieldRoot, editor, [], -1, { message: "Buscando colaboradores..." });

  try {
    const payload = await suggestFeedMentions(normalized, {
      headers: getPortalAuthHeaders(),
      signal: controller.signal
    });

    if (controller.signal.aborted) {
      return;
    }

    const active = getActiveMentionQuery(editor);
    if (!active || active.query.trim().toLowerCase() !== normalized.toLowerCase()) {
      return;
    }

    const state = getState(fieldRoot);
    state.suggestions = parseMentionSuggestions(payload);
    state.activeIndex = state.suggestions.length ? 0 : -1;

    if (!state.suggestions.length) {
      renderMentionDropdown(fieldRoot, editor, [], -1, { message: "Nenhum colaborador encontrado." });
      return;
    }

    renderMentionDropdown(fieldRoot, editor, state.suggestions, state.activeIndex);
  } catch (error) {
    if (controller.signal.aborted || error?.name === "AbortError") {
      return;
    }

    console.error("Falha ao sugerir mencoes.", error);
    renderMentionDropdown(fieldRoot, editor, [], -1, { message: "Nao foi possivel buscar colaboradores agora." });
  } finally {
    if (suggestControllers.get(fieldRoot) === controller) {
      suggestControllers.delete(fieldRoot);
    }
  }
}

function scheduleMentionSuggestions(fieldRoot, editor, query) {
  const existing = suggestTimers.get(fieldRoot);
  if (existing) {
    clearTimeout(existing);
  }

  suggestTimers.set(fieldRoot, setTimeout(() => {
    loadMentionSuggestions(fieldRoot, editor, query);
  }, 120));
}

function syncMentionState(fieldRoot, editor) {
  const active = getActiveMentionQuery(editor);
  if (!active) {
    hideMentionDropdown(fieldRoot);
    return;
  }

  if (!active.query) {
    showMentionHint(fieldRoot, editor);
    return;
  }

  scheduleMentionSuggestions(fieldRoot, editor, active.query);
}

function applyMentionSelection(fieldRoot, editor, suggestion) {
  const active = getActiveMentionQuery(editor);
  if (!active || !suggestion?.displayName || !suggestion?.userId) {
    return;
  }

  const range = plainTextOffsetsToRange(editor, active.start, active.end);
  if (!range) {
    return;
  }

  range.deleteContents();

  const chip = createMentionChip(suggestion.userId, suggestion.displayName);
  const trailingSpace = document.createTextNode(" ");
  const fragment = document.createDocumentFragment();
  fragment.append(chip, trailingSpace);
  range.insertNode(fragment);

  placeCaretAfter(trailingSpace);
  editor.dispatchEvent(new Event("input", { bubbles: true }));
  hideMentionDropdown(fieldRoot);
  editor.focus();
}

function pickActiveSuggestion(fieldRoot, editor) {
  const state = getState(fieldRoot);
  if (!state.suggestions.length || state.activeIndex < 0) {
    return false;
  }

  applyMentionSelection(fieldRoot, editor, state.suggestions[state.activeIndex]);
  return true;
}

function moveMentionSelection(fieldRoot, editor, delta) {
  const state = getState(fieldRoot);
  if (!state.suggestions.length) {
    return;
  }

  const total = state.suggestions.length;
  state.activeIndex = (state.activeIndex + delta + total) % total;
  renderMentionDropdown(fieldRoot, editor, state.suggestions, state.activeIndex);
}

export function bindMentionField({ fieldRoot, editor, onSync, maxLength = 2000 }) {
  if (!fieldRoot || !editor || fieldRoot.dataset.mentionBound === "true") {
    return {
      getText: () => "",
      getMentionedUserIds: () => [],
      resetMentions: () => {}
    };
  }

  fieldRoot.dataset.mentionBound = "true";

  const repositionDropdown = () => {
    const dropdown = getDropdown(fieldRoot);
    if (dropdown && !dropdown.hidden) {
      scheduleMentionDropdownPosition(fieldRoot, editor);
    }
  };

  const runSync = () => {
    requestAnimationFrame(() => {
      onSync?.();
      syncMentionState(fieldRoot, editor);
    });
  };

  editor.addEventListener("beforeinput", (event) => {
    if (!event.inputType?.startsWith("insert")) {
      return;
    }

    const { text } = serializeEditorContent(editor);
    const insertion = String(event.data || "");
    if (text.length + insertion.length > maxLength) {
      event.preventDefault();
    }
  });

  editor.addEventListener("input", runSync);

  editor.addEventListener("keyup", runSync);
  editor.addEventListener("click", runSync);
  editor.addEventListener("scroll", repositionDropdown, { passive: true });
  window.addEventListener("resize", repositionDropdown, { passive: true });

  editor.addEventListener("paste", (event) => {
    event.preventDefault();
    const pasted = event.clipboardData?.getData("text/plain") || "";
    if (!pasted) {
      return;
    }

    const { text } = serializeEditorContent(editor);
    const allowed = Math.max(0, maxLength - text.length);
    if (!allowed) {
      return;
    }

    document.execCommand("insertText", false, pasted.slice(0, allowed));
    runSync();
  });

  editor.addEventListener("keydown", (event) => {
    const state = getState(fieldRoot);
    const dropdownOpen = Boolean(getDropdown(fieldRoot) && !getDropdown(fieldRoot).hidden);

    if (dropdownOpen && event.key === "ArrowDown") {
      event.preventDefault();
      moveMentionSelection(fieldRoot, editor, 1);
      return;
    }

    if (dropdownOpen && event.key === "ArrowUp") {
      event.preventDefault();
      moveMentionSelection(fieldRoot, editor, -1);
      return;
    }

    if (dropdownOpen && (event.key === "Enter" || event.key === "Tab") && state.suggestions.length) {
      event.preventDefault();
      pickActiveSuggestion(fieldRoot, editor);
      return;
    }

    if (event.key === "Escape") {
      hideMentionDropdown(fieldRoot);
      return;
    }

    if (event.key === "Backspace") {
      if (removeChipAdjacentToCaret(editor, "backspace")) {
        event.preventDefault();
        runSync();
      }
      return;
    }

    if (event.key === "Delete") {
      if (removeChipAdjacentToCaret(editor, "delete")) {
        event.preventDefault();
        runSync();
      }
      return;
    }

    if (event.key === "@" || event.key.length === 1) {
      queueMicrotask(() => syncMentionState(fieldRoot, editor));
    }
  });

  fieldRoot.addEventListener("click", (event) => {
    const removeButton = event.target.closest("[data-action='remove-feed-mention']");
    if (removeButton) {
      event.preventDefault();
      removeMentionChip(removeButton.closest(CHIP_SELECTOR));
      editor.focus();
      runSync();
      return;
    }

    const target = event.target.closest("[data-action='pick-feed-mention']");
    if (!target) {
      return;
    }

    event.preventDefault();
    applyMentionSelection(fieldRoot, editor, {
      userId: target.getAttribute("data-user-id") || "",
      displayName: target.getAttribute("data-display-name") || ""
    });
  });

  return {
    getText: () => serializeEditorContent(editor).text,
    getMentionedUserIds: () => serializeEditorContent(editor).mentionedUserIds,
    resetMentions: () => {
      editor.innerHTML = "";
      hideMentionDropdown(fieldRoot);
    }
  };
}

export function renderMentionDropdownMarkup() {
  return `<div class="feed-mention-dropdown post-comment-mention-dropdown" hidden role="listbox" aria-label="Sugestoes de mencao"></div>`;
}
