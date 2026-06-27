import test from "node:test";
import assert from "node:assert/strict";

import { mapFeedViewModel } from "../../../assets/js/mappers/feedMapper.js";

test("feedMapper usa default de título e filtra posts sem conteúdo", () => {
  const result = mapFeedViewModel({
    posts: [
      { text: "", image: "" },
      { author: "Carlos", text: "Olá mundo" }
    ]
  });

  assert.equal(result.title, "FEED LIOCONNECTA");
  assert.equal(result.posts.length, 1);
  assert.equal(result.posts[0].author, "Carlos");
});

test("feedMapper normaliza métricas e comentários", () => {
  const result = mapFeedViewModel({
    title: "Feed RH",
    posts: [
      {
        author: "Marina",
        area: "RH",
        timeAgo: "agora",
        text: "Teste",
        reactions: "12",
        commentsCount: "7",
        sharesCount: "2",
        comments: [{ author: "Paulo", text: "Ótimo" }, { author: "", text: "" }]
      }
    ]
  });

  assert.equal(result.title, "Feed RH");
  assert.equal(result.posts[0].reactions, 12);
  assert.equal(result.posts[0].commentsCount, 7);
  assert.equal(result.posts[0].sharesCount, 2);
  assert.equal(result.posts[0].comments.length, 1);
  assert.equal(result.posts[0].comments[0].author, "Paulo");
});

test("feedMapper preserva galeria de imagens do post", () => {
  const result = mapFeedViewModel({
    posts: [
      {
        author: "Leonardo",
        text: "",
        image: "https://example.com/foto.jpg",
        images: [
          {
            url: "https://example.com/foto.jpg",
            description: "Cardapio do dia",
            aspectRatio: "9:16"
          }
        ]
      }
    ]
  });

  assert.equal(result.posts.length, 1);
  assert.equal(result.posts[0].images.length, 1);
  assert.equal(result.posts[0].images[0].description, "Cardapio do dia");
  assert.equal(result.posts[0].images[0].aspectRatio, "9:16");
});
