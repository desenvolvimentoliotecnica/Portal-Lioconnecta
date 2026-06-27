import { DEFAULT_FEED_TITLE, DEFAULT_POSTS } from "../view-models/defaults.js";
import { asArray, asNumber, asString } from "./shared.js";

function mapMentions(mentions = []) {
  return asArray(mentions)
    .map((mention) => ({
      userId: asString(mention?.userId ?? mention?.UserId ?? mention?.user_id, ""),
      displayName: asString(mention?.displayName ?? mention?.DisplayName ?? mention?.display_name, "")
    }))
    .filter((mention) => mention.userId && mention.displayName);
}

function mapComment(comment) {
  return {
    id: asString(comment?.id, ""),
    author: asString(comment?.author, "Colaborador"),
    text: asString(comment?.text, ""),
    createdAtUtc: comment?.createdAtUtc || comment?.created_at_utc || null,
    mentions: mapMentions(comment?.mentions)
  };
}

function mapPost(post) {
  const images = asArray(post?.images)
    .map((item) => ({
      id: asString(item?.id, ""),
      url: asString(item?.url, ""),
      description: asString(item?.description, ""),
      aspectRatio: asString(item?.aspectRatio, "free"),
      commentCount: asNumber(item?.commentCount, 0)
    }))
    .filter((item) => item.url);

  const image = asString(post?.image, images[0]?.url || "");

  return {
    postId: asString(post?.postId, ""),
    source: asString(post?.source, ""),
    communicationId: asString(post?.communicationId, ""),
    slug: asString(post?.slug, ""),
    author: asString(post?.author, "Autor não informado"),
    authorUserId: asString(post?.authorUserId ?? post?.AuthorUserId ?? post?.author_user_id, ""),
    area: asString(post?.area, "Área não informada"),
    timeAgo: asString(post?.timeAgo, "agora"),
    text: asString(post?.text, ""),
    mentions: mapMentions(post?.mentions),
    highlightTitle: asString(post?.highlightTitle, ""),
    highlightText: asString(post?.highlightText, ""),
    image,
    imageAlt: asString(post?.imageAlt, asString(post?.author, "Imagem do post")),
    images,
    reactions: asNumber(post?.reactions, 0),
    hasLiked: Boolean(post?.hasLiked),
    commentsCount: asNumber(post?.commentsCount, 0),
    sharesCount: asNumber(post?.sharesCount, 0),
    hasShared: Boolean(post?.hasShared),
    hasSaved: Boolean(post?.hasSaved),
    comments: asArray(post?.comments).map(mapComment).filter((comment) => comment.text)
  };
}

export function mapFeedViewModel(raw = {}, { allowDefaults = true } = {}) {
  const posts = asArray(raw.posts)
    .map(mapPost)
    .filter((post) => post.text || post.image || post.images.length);

  return {
    title: asString(raw.title, DEFAULT_FEED_TITLE),
    posts: posts.length > 0
      ? posts
      : allowDefaults
        ? [...DEFAULT_POSTS]
        : []
  };
}
