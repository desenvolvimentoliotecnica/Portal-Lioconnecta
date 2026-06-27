import { listCommunications } from "./communicationService.js";
import { mapCarouselViewModel } from "../mappers/carouselMapper.js";

function mapCommunicationSlides(items = []) {
  return items
    .filter((item) => item?.imageUrl && item?.slug)
    .sort((left, right) => {
      const leftTime = new Date(left?.publishedAt || 0).getTime();
      const rightTime = new Date(right?.publishedAt || 0).getTime();
      return rightTime - leftTime;
    })
    .slice(0, 5)
    .map((item) => ({
      src: String(item.imageUrl),
      alt: String(item.title || "Comunicado oficial"),
      href: `#comunicacao/leitura/${String(item.slug)}`
    }));
}

export async function getCarouselData() {
  try {
    const apiItems = await listCommunications();

    return mapCarouselViewModel({
      title: "COMUNICACAO CENTRALIZADA",
      slides: mapCommunicationSlides(apiItems)
    }, { allowDefaults: false });
  } catch (error) {
    console.error("Falha ao carregar carrossel de comunicados.", error);
    return mapCarouselViewModel({
      title: "COMUNICACAO CENTRALIZADA",
      slides: [],
      errorMessage: "Não foi possível carregar os comunicados publicados com imagem."
    }, { allowDefaults: false });
  }
}
