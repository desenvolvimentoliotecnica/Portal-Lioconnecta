using PortalRH.Api.Domain;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data;

public static class MoodSurveyFeedbackSeedData
{
  private static readonly DateTime SeedTimestamp = new(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

  public static IReadOnlyList<MoodSurveyFeedbackMessage> BuildMessages()
  {
    var messages = new List<MoodSurveyFeedbackMessage>();
    var order = 0;

    foreach (var message in MotivatedMessages)
    {
      messages.Add(CreateMessage(MoodSurveyOptionCatalog.Motivated, message, ++order));
    }

    order = 0;
    foreach (var message in GoodMessages)
    {
      messages.Add(CreateMessage(MoodSurveyOptionCatalog.Good, message, ++order));
    }

    order = 0;
    foreach (var message in TiredMessages)
    {
      messages.Add(CreateMessage(MoodSurveyOptionCatalog.Tired, message, ++order));
    }

    return messages;
  }

  private static MoodSurveyFeedbackMessage CreateMessage(string optionKey, string message, int sortOrder)
  {
    return new MoodSurveyFeedbackMessage
    {
      Id = Guid.NewGuid(),
      OptionKey = optionKey,
      Message = message,
      SortOrder = sortOrder,
      IsActive = true,
      CreatedAtUtc = SeedTimestamp,
      UpdatedAtUtc = SeedTimestamp
    };
  }

  private static readonly string[] MotivatedMessages =
  [
    "Sua energia positiva faz a diferença todos os dias.",
    "Continue assim, seu entusiasmo inspira quem está ao seu redor.",
    "Pessoas motivadas transformam desafios em oportunidades.",
    "Seu comprometimento contribui para grandes resultados.",
    "Que essa disposição continue impulsionando suas conquistas.",
    "Seu engajamento é um dos motores do sucesso da equipe.",
    "Aproveite esse momento para alcançar novos objetivos.",
    "Sua dedicação é percebida e valorizada.",
    "A motivação é contagiante. Obrigado por compartilhá-la.",
    "Continue acreditando no seu potencial.",
    "Seu esforço diário gera impacto positivo.",
    "Grandes resultados começam com atitudes positivas como a sua.",
    "Sua energia ajuda a construir um ambiente melhor para todos.",
    "Continue evoluindo e inspirando quem trabalha ao seu lado.",
    "O seu entusiasmo fortalece toda a equipe.",
    "Cada conquista começa com a vontade de fazer acontecer.",
    "Sua atitude demonstra compromisso e profissionalismo.",
    "Pessoas motivadas ajudam a construir empresas extraordinárias.",
    "Que sua determinação continue abrindo novos caminhos.",
    "Sua motivação é uma força valiosa para todos nós."
  ];

  private static readonly string[] GoodMessages =
  [
    "Ficamos felizes em saber que você está bem.",
    "Seu bem-estar é importante para nós.",
    "Que seu dia continue produtivo e equilibrado.",
    "Obrigado por compartilhar como você está se sentindo.",
    "Manter o equilíbrio é essencial para uma boa jornada.",
    "Continue cuidando de você e do seu desenvolvimento.",
    "Seu conforto e satisfação fazem a diferença.",
    "Pequenos avanços diários geram grandes resultados.",
    "Que você continue encontrando motivos para seguir em frente.",
    "Um ambiente saudável é construído por pessoas como você.",
    "Sua participação ajuda a tornar nosso ambiente melhor.",
    "Continue cultivando hábitos que promovam seu bem-estar.",
    "Estamos felizes por saber que as coisas estão caminhando bem.",
    "Cada dia é uma nova oportunidade para crescer.",
    "Que seu equilíbrio se transforme em novas conquistas.",
    "Obrigado por contribuir com sua experiência e dedicação.",
    "Sua percepção é muito importante para nós.",
    "Continue seguindo no seu ritmo e valorizando suas conquistas.",
    "Estar bem é um passo importante para alcançar grandes objetivos.",
    "Desejamos que seu dia continue positivo e produtivo."
  ];

  private static readonly string[] TiredMessages =
  [
    "Todo esforço merece reconhecimento e momentos de descanso.",
    "Lembre-se de cuidar da sua energia e do seu bem-estar.",
    "Dias mais difíceis também fazem parte da jornada.",
    "Seu empenho é valorizado, mesmo nos momentos de cansaço.",
    "Permita-se desacelerar quando necessário.",
    "Cuidar de si mesmo também é uma forma de produtividade.",
    "Você não precisa carregar tudo sozinho.",
    "Sua saúde e seu equilíbrio são prioridades.",
    "Pequenas pausas podem fazer uma grande diferença.",
    "Respire fundo, um passo de cada vez.",
    "Seu esforço é percebido e reconhecido.",
    "Momentos de descanso ajudam a renovar a motivação.",
    "Nem todos os dias precisam ser extraordinários.",
    "Respeitar seus limites é sinal de inteligência e maturidade.",
    "O cansaço passa, mas seu valor permanece.",
    "Cuide de você com a mesma dedicação que dedica ao trabalho.",
    "Estamos ao seu lado na construção de um ambiente melhor.",
    "Seu bem-estar é tão importante quanto seus resultados.",
    "A recuperação faz parte de qualquer trajetória de sucesso.",
    "Obrigado por continuar contribuindo, mesmo nos dias mais desafiadores."
  ];
}
