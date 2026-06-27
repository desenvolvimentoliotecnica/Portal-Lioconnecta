const CACHE_NAME = "lioconnecta-static-v44";
const ASSETS = [
  "./",
  "./index.html",
  "./admin/",
  "./admin/index.html",
  "./manifest.webmanifest",
  "./assets/css/tokens.css",
  "./assets/css/base.css",
  "./assets/css/components.css",
  "./assets/js/analytics.js",
  "./assets/js/admin/app.js",
  "./assets/js/app.js",
  "./assets/js/communications/index.js",
  "./assets/js/communications/mapper.js",
  "./assets/js/communications/renderer.js",
  "./assets/js/communications/service.js",
  "./assets/js/communications/validator.js",
  "./assets/js/components/cards.js",
  "./assets/js/components/carousel.js",
  "./assets/js/components/feed.js",
  "./assets/js/components/feedPhotoModal.js",
    "./assets/js/components/feedPhotoViewerModal.js",
    "./assets/js/components/feedMentions.js",
    "./assets/js/components/feedPostCommentComposer.js",
  "./assets/js/components/header.js",
  "./assets/js/components/html.js",
  "./assets/js/components/sidebar.js",
  "./assets/js/core/feedback.js",
  "./assets/js/core/runtimeConfig.js",
  "./assets/js/feed/index.js",
  "./assets/js/feed/mapper.js",
  "./assets/js/feed/renderer.js",
  "./assets/js/feed/service.js",
  "./assets/js/feed/validator.js",
  "./assets/js/home/index.js",
  "./assets/js/home/renderer.js",
  "./assets/js/home/service.js",
  "./assets/js/hrProfile/index.js",
  "./assets/js/hrProfile/moduleCatalog.js",
  "./assets/js/hrProfile/renderer.js",
  "./assets/js/hrProfile/service.js",
  "./assets/js/journey/index.js",
  "./assets/js/journey/moduleCatalog.js",
  "./assets/js/journey/renderer.js",
  "./assets/js/journey/service.js",
  "./assets/js/layout/header.js",
  "./assets/js/layout/index.js",
  "./assets/js/layout/sidebar.js",
  "./assets/js/mappers/carouselMapper.js",
  "./assets/js/mappers/communicationMapper.js",
  "./assets/js/mappers/feedMapper.js",
  "./assets/js/mappers/panelMapper.js",
  "./assets/js/mappers/shared.js",
  "./assets/js/mappers/userMapper.js",
  "./assets/js/profile/index.js",
  "./assets/js/profile/mapper.js",
  "./assets/js/profile/service.js",
  "./assets/js/profile/validator.js",
  "./assets/js/services/apiClient.js",
  "./assets/js/services/adminAuthService.js",
  "./assets/js/services/carouselService.js",
  "./assets/js/services/communicationService.js",
  "./assets/js/services/feedService.js",
  "./assets/js/services/feedMedia.js",
  "./assets/js/services/homeService.js",
  "./assets/js/services/index.js",
  "./assets/js/services/mockPublicationStore.js",
  "./assets/js/services/panelService.js",
  "./assets/js/services/userService.js",
  "./assets/js/validators/carouselValidator.js",
  "./assets/js/validators/communicationValidator.js",
  "./assets/js/validators/feedValidator.js",
  "./assets/js/validators/panelValidator.js",
  "./assets/js/validators/shared.js",
  "./assets/js/validators/userValidator.js",
  "./assets/js/validators/validationError.js",
  "./assets/js/view-models/defaults.js",
  "./assets/data/user.json",
  "./assets/data/carousel.json",
  "./assets/data/communications.json",
  "./assets/data/feed.json",
  "./assets/data/panels.json",
  "./local-api/user.json",
  "./local-api/feed.json",
  "./local-api/panels.json",
  "./local-api/carousel.json",
  "./local-api/communications.json"
];

self.addEventListener("install", (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(ASSETS))
  );
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(
        keys
          .filter((key) => key !== CACHE_NAME)
          .map((key) => caches.delete(key))
      )
    )
  );
  self.clients.claim();
});

self.addEventListener("fetch", (event) => {
  event.respondWith(
    caches.match(event.request).then((cached) => cached || fetch(event.request))
  );
});
