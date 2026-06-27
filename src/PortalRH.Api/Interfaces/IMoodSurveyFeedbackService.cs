using PortalRH.Api.Contracts.MoodSurvey;

namespace PortalRH.Api.Interfaces;

public interface IMoodSurveyFeedbackService
{
    Task EnsureSeedAsync(CancellationToken cancellationToken);

    Task<MoodSurveyFeedbackMessageListResponse> GetAllAsync(string? optionKey, CancellationToken cancellationToken);

    Task<MoodSurveyFeedbackMessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MoodSurveyFeedbackMessageDto> CreateAsync(
        UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken);

    Task<MoodSurveyFeedbackMessageDto?> UpdateAsync(
        Guid id,
        UpsertMoodSurveyFeedbackMessageRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<MoodSurveyFeedbackPickResult?> PickRandomAsync(string optionKey, CancellationToken cancellationToken);
}

public sealed record MoodSurveyFeedbackPickResult(Guid Id, string Message);
