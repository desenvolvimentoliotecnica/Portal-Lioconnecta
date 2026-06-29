// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", () => {
  const toggle = document.getElementById("toggle-test-grid-dashed");
  const shell = document.querySelector(".showcase-shell");

  if (!toggle || !shell) {
    return;
  }

  const syncDashedState = () => {
    shell.classList.toggle("showcase-shell--test-grid-dashed", toggle.checked);
  };

  toggle.addEventListener("change", syncDashedState);
  syncDashedState();
});
