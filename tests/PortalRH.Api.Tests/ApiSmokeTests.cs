using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortalRH.Api.Contracts.Admin.Auth;
using PortalRH.Api.Contracts.Admin.Ldap;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Contracts.Admin.Polls;
using PortalRH.Api.Contracts.Admin.PortalUsers;
using PortalRH.Api.Contracts.Agenda;
using PortalRH.Api.Contracts.Auth;
using PortalRH.Api.Contracts.Communications;
using PortalRH.Api.Contracts.Feed;
using PortalRH.Api.Contracts.MoodSurvey;
using PortalRH.Api.Contracts.Notifications;
using PortalRH.Api.Contracts.Polls;
using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Contracts.Journey;
using PortalRH.Api.Contracts.Kpis;
using PortalRH.Api.Contracts.QuickLinks;
using PortalRH.Api.Contracts.Shell;
using PortalRH.Api.Data;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Tests;

public class ApiSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiSmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        ResetDatabaseAsync(factory.Services).GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CorsPolicy_AllowsFrontendOriginsOnPorts3020And4173()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/admin/auth/login");
        request.Headers.Add("Origin", "http://10.0.0.79:3020");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowOrigins));
        Assert.Contains("http://10.0.0.79:3020", allowOrigins);
    }

    [Fact]
    public async Task CommunicationsEndpoint_CreatesAndReadsCommunication()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        Assert.False(string.IsNullOrWhiteSpace(adminSession.Token));

        var request = new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Alta prioridade",
            Title = "Comunicado inicial de RH",
            Summary = "Resumo do comunicado inicial.",
            Body = "Corpo do comunicado inicial.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            ImageUrl = "https://example.com/comunicado.png",
            IsFeatured = true,
            PublishedAt = new DateTime(2026, 06, 19, 0, 0, 0, DateTimeKind.Utc)
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/communications", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CommunicationDto>();
        Assert.NotNull(created);
        Assert.Equal("Comunicado inicial de RH", created.Title);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.False(string.IsNullOrWhiteSpace(created.Slug));

        var listResponse = await _client.GetFromJsonAsync<List<CommunicationDto>>("/api/communications");
        Assert.NotNull(listResponse);
        Assert.Single(listResponse);
        Assert.Equal(0, listResponse[0].LikeCount);
        Assert.False(listResponse[0].HasLiked);
    }

    [Fact]
    public async Task CommunicationsEndpoint_TogglesLikeWithAuditTrail()
    {
        await EnsureLdapEnabledAsync();

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Comunicado",
            Title = "Comunicado para curtidas",
            Summary = "Resumo do comunicado para curtidas.",
            Body = "Corpo do comunicado para curtidas.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            PublishedAt = new DateTime(2026, 06, 24, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CommunicationDto>();
        Assert.NotNull(created);

        var portalSession = await LoginPortalUserAsync();
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
        _client.DefaultRequestHeaders.Remove("Origin");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "10.30.40.51");
        _client.DefaultRequestHeaders.Add("Origin", "http://127.0.0.1:3020");

        var likeResponse = await _client.PostAsync($"/api/communications/{created.Id}/like", content: null);
        Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

        var liked = await likeResponse.Content.ReadFromJsonAsync<CommunicationLikeResponse>();
        Assert.NotNull(liked);
        Assert.Equal(created.Id, liked.CommunicationId);
        Assert.Equal(1, liked.LikeCount);
        Assert.True(liked.HasLiked);

        var listResponse = await _client.GetFromJsonAsync<List<CommunicationDto>>("/api/communications");
        Assert.NotNull(listResponse);
        Assert.Contains(listResponse, item => item.Id == created.Id && item.LikeCount == 1 && item.HasLiked);

        var unlikeResponse = await _client.PostAsync($"/api/communications/{created.Id}/like", content: null);
        Assert.Equal(HttpStatusCode.OK, unlikeResponse.StatusCode);

        var unliked = await unlikeResponse.Content.ReadFromJsonAsync<CommunicationLikeResponse>();
        Assert.NotNull(unliked);
        Assert.Equal(0, unliked.LikeCount);
        Assert.False(unliked.HasLiked);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.CommunicationInteractionAuditLogs
            .Where(item => item.CommunicationId == created.Id && item.PortalUserId == portalSession.User.Id)
            .OrderBy(item => item.CreatedAtUtc)
            .ToListAsync();

        Assert.Equal(2, auditEntries.Count);
        Assert.Equal("CurtidaRegistrada", auditEntries[0].ActionType);
        Assert.Equal("CurtidaRemovida", auditEntries[1].ActionType);
        Assert.Equal("10.30.40.51", auditEntries[0].IpAddress);
        Assert.Equal("http://127.0.0.1:3020", auditEntries[0].Origin);
    }

    [Fact]
    public async Task FeedEndpoint_CreatesTextPostWithAuditTrailForAnyPortalUser()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
        _client.DefaultRequestHeaders.Remove("Origin");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "10.30.40.52");
        _client.DefaultRequestHeaders.Add("Origin", "http://127.0.0.1:3020");

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Primeira publicacao de texto no feed interno."
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);
        Assert.Equal("UserPost", created.Item.Source);
        Assert.Equal("Primeira publicacao de texto no feed interno.", created.Item.Text);
        Assert.False(string.IsNullOrWhiteSpace(created.Item.Author));

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.Text.Contains("Primeira publicacao"));

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.FeedPostAuditLogs
            .Where(item => item.FeedPostId == created.Item.Id && item.PortalUserId == portalSession.User.Id)
            .ToListAsync();

        Assert.Single(auditEntries);
        Assert.Equal("PublicacaoRegistrada", auditEntries[0].ActionType);
        Assert.Equal("10.30.40.52", auditEntries[0].IpAddress);
    }

    [Fact]
    public async Task FeedEndpoint_CreatesPhotoPostWithMediaMetadata()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var imageUploadUrl = await UploadFeedAssetAsync("feed-foto.png", "image/png", [137, 80, 78, 71, 13, 10, 26, 10]);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Publicacao com foto no feed interno.",
            Media =
            [
                new CreateFeedPostMediaItem
                {
                    Url = imageUploadUrl,
                    Description = "Foto do time",
                    AspectRatio = "16:9"
                }
            ]
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);
        Assert.Single(created.Item.Media);
        Assert.Equal("Foto do time", created.Item.Media[0].Description);
        Assert.Equal("16:9", created.Item.Media[0].AspectRatio);
        Assert.Equal(imageUploadUrl, created.Item.Media[0].Url);
        Assert.Equal(imageUploadUrl, created.Item.ImageUrl);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.Media.Count == 1);
    }

    [Fact]
    public async Task FeedEndpoint_CreatesAndListsMediaCommentsSeparatelyFromPost()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var imageUploadUrl = await UploadFeedAssetAsync("feed-comentario.png", "image/png", [137, 80, 78, 71, 13, 10, 26, 10]);
        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post com comentario na foto.",
            Media =
            [
                new CreateFeedPostMediaItem
                {
                    Url = imageUploadUrl,
                    Description = "Foto comentada",
                    AspectRatio = "1:1"
                }
            ]
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);
        var mediaId = created.Item.Media[0].Id;

        var commentResponse = await _client.PostAsJsonAsync($"/api/feed/media/{mediaId}/comments", new CreateFeedMediaCommentRequest
        {
            Text = "Comentario exclusivo da foto."
        });
        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

        var commentPayload = await commentResponse.Content.ReadFromJsonAsync<CreateFeedMediaCommentResponse>();
        Assert.NotNull(commentPayload);
        Assert.Equal("Comentario exclusivo da foto.", commentPayload.Item.Text);

        var listResponse = await _client.GetAsync($"/api/feed/media/{mediaId}/comments");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var comments = await listResponse.Content.ReadFromJsonAsync<FeedMediaCommentsResponse>();
        Assert.NotNull(comments);
        Assert.Single(comments.Items);
        Assert.Equal("Comentario exclusivo da foto.", comments.Items[0].Text);
    }

    [Fact]
    public async Task FeedEndpoint_CreatesAndListsPostCommentsWithMentions()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post com comentario e mencao."
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);

        var suggestionsResponse = await _client.GetAsync("/api/feed/mentions/suggest?q=por");
        Assert.Equal(HttpStatusCode.OK, suggestionsResponse.StatusCode);
        var suggestions = await suggestionsResponse.Content.ReadFromJsonAsync<FeedMentionSuggestionsResponse>();
        Assert.NotNull(suggestions);

        var commentResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/comments", new CreateFeedPostCommentRequest
        {
            Text = "Comentario no post com @Portal User",
            MentionedUserIds = [portalSession.User.Id]
        });
        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

        var commentPayload = await commentResponse.Content.ReadFromJsonAsync<CreateFeedPostCommentResponse>();
        Assert.NotNull(commentPayload);
        Assert.Equal("Comentario no post com @Portal User", commentPayload.Item.Text);
        Assert.Single(commentPayload.Item.Mentions);

        var listResponse = await _client.GetAsync($"/api/feed/{created.Item.Id}/comments");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var comments = await listResponse.Content.ReadFromJsonAsync<FeedPostCommentsResponse>();
        Assert.NotNull(comments);
        Assert.Single(comments.Items);
        Assert.Equal("Comentario no post com @Portal User", comments.Items[0].Text);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.CommentCount == 1 && item.Comments.Count == 1);
    }

    [Fact]
    public async Task FeedEndpoint_CreatesPostWithMentionsInBody()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post com mencao @Portal User no texto.",
            MentionedUserIds = [portalSession.User.Id]
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);
        Assert.Single(created.Item.Mentions);
        Assert.Equal("Post com mencao @Portal User no texto.", created.Item.Text);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.Mentions.Count == 1);
    }

    [Fact]
    public async Task FeedEndpoint_TogglesLikeOnUserPostWithAuditTrail()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post para validar curtidas reais."
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);

        var likeResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/like", new ToggleFeedLikeRequest
        {
            Source = "UserPost"
        });
        Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

        var liked = await likeResponse.Content.ReadFromJsonAsync<FeedLikeResponse>();
        Assert.NotNull(liked);
        Assert.Equal(1, liked.LikeCount);
        Assert.True(liked.HasLiked);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.LikeCount == 1 && item.HasLiked);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.FeedPostAuditLogs
            .Where(item => item.FeedPostId == created.Item.Id && item.ActionType == "CurtidaRegistrada")
            .ToListAsync();

        Assert.Single(auditEntries);
    }

    [Fact]
    public async Task FeedEndpoint_TogglesSaveOnUserPostAndCommunication_AndListsSavedFeed()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post proprio para validar salvamento."
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);

        var saveResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/save", new ToggleFeedSaveRequest
        {
            Source = "UserPost"
        });
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var saved = await saveResponse.Content.ReadFromJsonAsync<FeedSaveResponse>();
        Assert.NotNull(saved);
        Assert.True(saved.HasSaved);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == created.Item.Id && item.HasSaved);

        var savedFeed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed/saved");
        Assert.NotNull(savedFeed);
        Assert.Contains(savedFeed.Items, item => item.Id == created.Item.Id && item.HasSaved);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var communicationResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Comunicado",
            Title = "Comunicado para salvar",
            Summary = "Resumo do comunicado para salvar.",
            Body = "Corpo do comunicado para salvar.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            PublishedAt = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.Created, communicationResponse.StatusCode);

        var communication = await communicationResponse.Content.ReadFromJsonAsync<CommunicationDto>();
        Assert.NotNull(communication);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var communicationSaveResponse = await _client.PostAsJsonAsync($"/api/feed/{communication.Id}/save", new ToggleFeedSaveRequest
        {
            Source = "Communication"
        });
        Assert.Equal(HttpStatusCode.OK, communicationSaveResponse.StatusCode);

        var savedCommunication = await communicationSaveResponse.Content.ReadFromJsonAsync<FeedSaveResponse>();
        Assert.NotNull(savedCommunication);
        Assert.True(savedCommunication.HasSaved);

        savedFeed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed/saved");
        Assert.NotNull(savedFeed);
        Assert.Contains(savedFeed.Items, item => item.Id == communication.Id && item.Source == "Communication" && item.HasSaved);

        var unsaveResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/save", new ToggleFeedSaveRequest
        {
            Source = "UserPost"
        });
        Assert.Equal(HttpStatusCode.OK, unsaveResponse.StatusCode);

        var unsaved = await unsaveResponse.Content.ReadFromJsonAsync<FeedSaveResponse>();
        Assert.NotNull(unsaved);
        Assert.False(unsaved.HasSaved);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.FeedPostAuditLogs
            .Where(item => item.FeedPostId == created.Item.Id && item.ActionType == "SalvamentoRegistrado")
            .ToListAsync();

        Assert.Single(auditEntries);
    }

    [Fact]
    public async Task PortalLogin_AllowsSeededCollaboratorWithoutLdap()
    {
        await EnsureLdapEnabledAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var portalUserSeedService = scope.ServiceProvider.GetRequiredService<IPortalUserSeedService>();
            await portalUserSeedService.EnsureSeedAsync(CancellationToken.None);
        }

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/ldap/login", new LdapLoginRequest
        {
            Login = "colaborador1@liotecnica.com.br",
            Password = PortalCollaboratorSeedData.DefaultPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var payload = await loginResponse.Content.ReadFromJsonAsync<PortalLoginResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Colaborador Um", payload.User.DisplayName);
    }

    [Fact]
    public async Task FeedEndpoint_TogglesShareOnOtherUserPost_AndBlocksOwnPostAndCommunication()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post proprio para validar bloqueio de compartilhamento."
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);

        var ownShareResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/share", new ToggleFeedShareRequest
        {
            Source = "UserPost"
        });
        Assert.Equal(HttpStatusCode.Forbidden, ownShareResponse.StatusCode);

        var otherUserPostId = await SeedFeedPostForAnotherPortalUserAsync("Post de outro colaborador para compartilhar.");

        var shareResponse = await _client.PostAsJsonAsync($"/api/feed/{otherUserPostId}/share", new ToggleFeedShareRequest
        {
            Source = "UserPost"
        });
        Assert.Equal(HttpStatusCode.OK, shareResponse.StatusCode);

        var shared = await shareResponse.Content.ReadFromJsonAsync<FeedShareResponse>();
        Assert.NotNull(shared);
        Assert.Equal(1, shared.ShareCount);
        Assert.True(shared.HasShared);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.Contains(feed.Items, item => item.Id == otherUserPostId && item.ShareCount == 1 && item.HasShared);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var communicationResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Comunicado",
            Title = "Comunicado sem compartilhamento",
            Summary = "Resumo do comunicado sem compartilhamento.",
            Body = "Corpo do comunicado sem compartilhamento.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            PublishedAt = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.Created, communicationResponse.StatusCode);

        var communication = await communicationResponse.Content.ReadFromJsonAsync<CommunicationDto>();
        Assert.NotNull(communication);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var communicationShareResponse = await _client.PostAsJsonAsync($"/api/feed/{communication.Id}/share", new ToggleFeedShareRequest
        {
            Source = "Communication"
        });
        Assert.Equal(HttpStatusCode.Forbidden, communicationShareResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.FeedPostAuditLogs
            .Where(item => item.FeedPostId == otherUserPostId && item.ActionType == "CompartilhamentoRegistrado")
            .ToListAsync();

        Assert.Single(auditEntries);
    }

    [Fact]
    public async Task FeedEndpoint_SoftDeletesOwnPostAndHidesItFromFeed()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var createResponse = await _client.PostAsJsonAsync("/api/feed", new CreateFeedPostRequest
        {
            Text = "Post para validar exclusao logica."
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreateFeedPostResponse>();
        Assert.NotNull(created);
        Assert.Equal(portalSession.User.Id, created.Item.AuthorUserId);

        var commentResponse = await _client.PostAsJsonAsync($"/api/feed/{created.Item.Id}/comments", new CreateFeedPostCommentRequest
        {
            Text = "Comentario que deve sumir junto com o post."
        });
        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/feed/{created.Item.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deleted = await deleteResponse.Content.ReadFromJsonAsync<DeleteFeedPostResponse>();
        Assert.NotNull(deleted);
        Assert.True(deleted.Deleted);

        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/feed");
        Assert.NotNull(feed);
        Assert.DoesNotContain(feed.Items, item => item.Id == created.Item.Id);

        var commentsResponse = await _client.GetAsync($"/api/feed/{created.Item.Id}/comments");
        Assert.Equal(HttpStatusCode.NotFound, commentsResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var persistedPost = await dbContext.FeedPosts.FirstAsync(item => item.Id == created.Item.Id);
        Assert.NotNull(persistedPost.DeletedAtUtc);

        var auditEntries = await dbContext.FeedPostAuditLogs
            .Where(item => item.FeedPostId == created.Item.Id && item.ActionType == "PublicacaoRemovida")
            .ToListAsync();
        Assert.Single(auditEntries);
    }

    [Fact]
    public async Task MeUiEndpoint_ReturnsPersonalizedShellForPortalSession()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var meUi = await _client.GetFromJsonAsync<MeUiResponse>("/api/me-ui");

        Assert.NotNull(meUi);
        Assert.Equal("LIOCONNECTA", meUi.Brand.Name);
        Assert.Equal(portalSession.User.DisplayName, meUi.User.Name);
        Assert.Contains(meUi.NavItems, item => item.Route == "inicio");
        Assert.Contains(meUi.NavItems, item => item.Route == "comunicacao");
        Assert.DoesNotContain(meUi.NavItems, item => item.Route == "configuracoes");
        Assert.True(meUi.Composer.Enabled);
        Assert.NotEmpty(meUi.Composer.Actions);
    }

    [Fact]
    public async Task MeUiEndpoint_IncludesSettingsNavigationForPortalAdmin()
    {
        await EnsureLdapEnabledAsync();
        await LoginPortalUserAsync();

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var usersPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&page=1&pageSize=5");
        Assert.NotNull(usersPayload);

        var user = Assert.Single(usersPayload.Items.Where(item => item.Login == "roberto.almeida@liotecnica.com.br"));
        var roleResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/role", new UpdatePortalUserRoleRequest
        {
            Role = "PortalAdmin"
        });
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var meUi = await _client.GetFromJsonAsync<MeUiResponse>("/api/me-ui");

        Assert.NotNull(meUi);
        Assert.Contains(meUi.NavItems, item => item.Route == "configuracoes" && item.ModuleKey == "settings");
    }

    [Fact]
    public async Task PanelsEndpoint_ReturnsPersonalizedProfilePanelForPortalSession()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var panels = await _client.GetFromJsonAsync<PanelsResponse>("/api/panels");

        Assert.NotNull(panels);
        Assert.NotEmpty(panels.LeftPanels);
        Assert.NotEmpty(panels.RightPanels);

        var profilePanel = panels.RightPanels.FirstOrDefault(item => string.Equals(item.Type, "profile", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profilePanel);
        Assert.Equal(portalSession.User.DisplayName, profilePanel.Name);
        Assert.Equal(portalSession.User.Department ?? string.Empty, profilePanel.Subtitle);
        Assert.NotNull(profilePanel.Items);
        Assert.NotEmpty(profilePanel.Items);

        var quickLinksPanel = panels.RightPanels.FirstOrDefault(item => string.Equals(item.Type, "quick-links", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(quickLinksPanel);
        Assert.NotEmpty(quickLinksPanel.Items!);

        var journeyPanel = panels.LeftPanels.FirstOrDefault(item => item.Title == "MINHA JORNADA");
        Assert.NotNull(journeyPanel);
        Assert.NotEmpty(journeyPanel.Items!);
    }

    [Fact]
    public async Task OperationalEndpoints_ReturnSimulatedProvidersForPortalSession()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var quickLinks = await _client.GetFromJsonAsync<QuickLinkListResponse>("/api/quick-links");
        var journey = await _client.GetFromJsonAsync<JourneySummaryResponse>("/api/journey/summary");
        var kpis = await _client.GetFromJsonAsync<KpiSummaryResponse>("/api/kpis/summary");
        var hrProfile = await _client.GetFromJsonAsync<HrProfileResponse>("/api/hr/profile");

        Assert.NotNull(quickLinks);
        Assert.NotEmpty(quickLinks.Items);
        Assert.NotNull(journey);
        Assert.True(journey.IsSimulated);
        Assert.NotEmpty(journey.Items);
        Assert.Contains(journey.Items, item => item.Url.Contains("minha-jornada/tarefas", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(kpis);
        Assert.True(kpis.IsSimulated);
        Assert.NotNull(hrProfile);
        Assert.True(hrProfile.IsSimulated);
        Assert.Equal(portalSession.User.DisplayName, hrProfile.Name);
        Assert.Contains(hrProfile.Items, item => item.Url.Contains("perfil-rh/holerite", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task HrWorkspaceEndpoints_ReturnSimulatedDataForPortalSession()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var ferias = await _client.GetFromJsonAsync<HrVacationResponse>("/api/hr/ferias");
        var holerite = await _client.GetFromJsonAsync<HrPayslipResponse>("/api/hr/holerite");
        var beneficios = await _client.GetFromJsonAsync<HrBenefitsResponse>("/api/hr/beneficios");
        var avaliacao = await _client.GetFromJsonAsync<HrEvaluationResponse>("/api/hr/avaliacao");
        var cadastro = await _client.GetFromJsonAsync<HrPersonalDataResponse>("/api/hr/cadastro");
        var ponto = await _client.GetFromJsonAsync<HrTimesheetResponse>("/api/hr/ponto");

        Assert.NotNull(ferias);
        Assert.True(ferias.IsSimulated);
        Assert.NotEmpty(ferias.Requests);
        Assert.NotNull(holerite);
        Assert.True(holerite.IsSimulated);
        Assert.NotEmpty(holerite.Items);
        Assert.NotNull(beneficios);
        Assert.True(beneficios.IsSimulated);
        Assert.NotEmpty(beneficios.Items);
        Assert.NotNull(avaliacao);
        Assert.True(avaliacao.IsSimulated);
        Assert.NotEmpty(avaliacao.Competencies);
        Assert.NotNull(cadastro);
        Assert.True(cadastro.IsSimulated);
        Assert.NotEmpty(cadastro.Sections);
        Assert.NotNull(ponto);
        Assert.True(ponto.IsSimulated);
        Assert.NotEmpty(ponto.Entries);
    }

    [Fact]
    public async Task JourneyWorkspaceEndpoints_ReturnSimulatedDataForPortalSession()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var tarefas = await _client.GetFromJsonAsync<JourneyTasksResponse>("/api/journey/tarefas");
        var solicitacoes = await _client.GetFromJsonAsync<JourneyRequestsResponse>("/api/journey/solicitacoes");
        var trilhas = await _client.GetFromJsonAsync<JourneyLearningPathsResponse>("/api/journey/trilhas");
        var documentos = await _client.GetFromJsonAsync<JourneyDocumentsResponse>("/api/journey/documentos");

        Assert.NotNull(tarefas);
        Assert.True(tarefas.IsSimulated);
        Assert.NotEmpty(tarefas.Items);
        Assert.NotNull(solicitacoes);
        Assert.True(solicitacoes.IsSimulated);
        Assert.NotEmpty(solicitacoes.Items);
        Assert.NotNull(trilhas);
        Assert.True(trilhas.IsSimulated);
        Assert.NotEmpty(trilhas.Items);
        Assert.NotNull(documentos);
        Assert.True(documentos.IsSimulated);
        Assert.NotEmpty(documentos.Items);
    }

    [Fact]
    public async Task MeUiEndpoint_RequiresPortalSession()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var response = await _client.GetAsync("/api/me-ui");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PanelsEndpoint_RequiresPortalSession()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var response = await _client.GetAsync("/api/panels");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CommunicationsEndpoint_RejectsCreateWithoutAdminSession()
    {
        var request = new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Alta prioridade",
            Title = "Comunicado sem sessao admin",
            Summary = "Resumo",
            Body = "Corpo",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            PublishedAt = new DateTime(2026, 06, 19, 0, 0, 0, DateTimeKind.Utc)
        };

        var client = _client;
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PostAsJsonAsync("/api/communications", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CommunicationsEndpoint_AllowsHrManagerPortalSessionToCreateCommunication()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var forbiddenResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Alta prioridade",
            Title = "Comunicado bloqueado para colaborador",
            Summary = "Resumo",
            Body = "Corpo",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir anexo",
            Owner = "Recursos Humanos",
            PublishedAt = new DateTime(2026, 06, 24, 10, 0, 0, DateTimeKind.Utc)
        });
        Assert.Equal(HttpStatusCode.Unauthorized, forbiddenResponse.StatusCode);

        await PromotePortalUserToHrManagerAsync();

        var allowedResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Alta prioridade",
            Title = "Comunicado RH via portal",
            Summary = "Resumo persistido pelo gestor de RH.",
            Body = "Corpo persistido pelo gestor de RH.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir comunicado",
            Owner = "Recursos Humanos",
            PublishedAt = new DateTime(2026, 06, 24, 10, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(HttpStatusCode.Created, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task AdminLdapEndpoint_ReturnsSeededDefaultConfiguration()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var configuration = await _client.GetFromJsonAsync<LdapConfigurationDto>("/api/admin/ldap");

        Assert.NotNull(configuration);
        Assert.True(configuration.IsEnabled);
        Assert.Equal("dc-virtual-02.liotecnica.com.br", configuration.Server);
        Assert.Equal(389, configuration.Port);
        Assert.Equal("DC=liotecnica,DC=com,DC=br", configuration.BaseDn);
        Assert.Equal("OU=Departamentos,DC=liotecnica,DC=com,DC=br", configuration.UserSearchBase);
        Assert.Equal("LIOTECNICA", configuration.NetbiosDomain);
        Assert.Equal("domain-backslash-samaccountname", configuration.LoginFormat);
        Assert.Equal("(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))", configuration.SearchFilter);
        Assert.Equal("displayName", configuration.DisplayNameAttribute);
        Assert.False(configuration.UseLdaps);
        Assert.False(configuration.UseStartTls);
        Assert.False(configuration.IgnoreCertificateValidation);
        Assert.False(configuration.HasServiceAccountPassword);
    }

    [Fact]
    public async Task AdminLdapEndpoint_SavesAndReadsConfiguration()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var request = new UpsertLdapConfigurationRequest
        {
            IsEnabled = true,
            Server = "dc-virtual-02.liotecnica.com.br",
            Port = 389,
            UseLdaps = false,
            UseStartTls = true,
            IgnoreCertificateValidation = true,
            BaseDn = "DC=liotecnica,DC=com,DC=br",
            UserSearchBase = "OU=Usuarios,DC=liotecnica,DC=com,DC=br",
            NetbiosDomain = "LIOTECNICA",
            LoginFormat = "email-or-upn-or-samaccountname",
            BindDn = "CN=servico-hub,OU=ServiceAccounts,DC=liotecnica,DC=com,DC=br",
            ServiceAccountPassword = "Senha@123",
            SearchFilter = "(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))",
            DisplayNameAttribute = "displayName"
        };

        var saveResponse = await _client.PutAsJsonAsync("/api/admin/ldap", request);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var saved = await saveResponse.Content.ReadFromJsonAsync<LdapConfigurationDto>();
        Assert.NotNull(saved);
        Assert.True(saved.IsEnabled);
        Assert.Equal("dc-virtual-02.liotecnica.com.br", saved.Server);
        Assert.True(saved.HasServiceAccountPassword);

        var getResponse = await _client.GetFromJsonAsync<LdapConfigurationDto>("/api/admin/ldap");
        Assert.NotNull(getResponse);
        Assert.Equal("LIOTECNICA", getResponse.NetbiosDomain);
        Assert.Equal("displayName", getResponse.DisplayNameAttribute);
        Assert.True(getResponse.HasServiceAccountPassword);
    }

    [Fact]
    public async Task AdminMicrosoftGraphEndpoint_ReturnsSeededDefaultConfiguration()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var configuration = await _client.GetFromJsonAsync<MicrosoftGraphConfigurationDto>("/api/admin/microsoft-graph");

        Assert.NotNull(configuration);
        Assert.False(configuration.IsEnabled);
        Assert.Equal(string.Empty, configuration.TenantId);
        Assert.Equal(string.Empty, configuration.ClientId);
        Assert.Equal("userPrincipalName", configuration.UserIdentifier);
        Assert.False(configuration.HasClientSecret);
    }

    [Fact]
    public async Task AdminMicrosoftGraphEndpoint_SavesAndReadsConfiguration()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var request = new UpsertMicrosoftGraphConfigurationRequest
        {
            IsEnabled = true,
            TenantId = "b95b38fc-0302-4cf4-8c95-d45754f48411",
            ClientId = "3e73d586-06c1-455c-86a7-07c2a89c383d",
            ClientSecret = "HzW8Q~sample-secret-value",
            UserIdentifier = "mail"
        };

        var saveResponse = await _client.PutAsJsonAsync("/api/admin/microsoft-graph", request);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var saved = await saveResponse.Content.ReadFromJsonAsync<MicrosoftGraphConfigurationDto>();
        Assert.NotNull(saved);
        Assert.True(saved.IsEnabled);
        Assert.Equal("b95b38fc-0302-4cf4-8c95-d45754f48411", saved.TenantId);
        Assert.Equal("3e73d586-06c1-455c-86a7-07c2a89c383d", saved.ClientId);
        Assert.Equal("mail", saved.UserIdentifier);
        Assert.True(saved.HasClientSecret);

        var getResponse = await _client.GetFromJsonAsync<MicrosoftGraphConfigurationDto>("/api/admin/microsoft-graph");
        Assert.NotNull(getResponse);
        Assert.True(getResponse.HasClientSecret);
        Assert.Equal("mail", getResponse.UserIdentifier);
    }

    [Fact]
    public async Task AdminMicrosoftGraphTest_ReturnsValidationErrorWhenTenantMissing()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var response = await _client.PostAsJsonAsync("/api/admin/microsoft-graph/test", new UpsertMicrosoftGraphConfigurationRequest
        {
            TenantId = string.Empty,
            ClientId = "3e73d586-06c1-455c-86a7-07c2a89c383d",
            ClientSecret = "secret",
            UserIdentifier = "userPrincipalName"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuthEndpoint_AuthenticatesViaLdap_WhenConfigurationIsEnabled()
    {
        var adminLoginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        var adminSession = await adminLoginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var ldapConfig = new UpsertLdapConfigurationRequest
        {
            IsEnabled = true,
            Server = "dc-virtual-02.liotecnica.com.br",
            Port = 389,
            UseLdaps = false,
            UseStartTls = true,
            IgnoreCertificateValidation = true,
            BaseDn = "DC=liotecnica,DC=com,DC=br",
            UserSearchBase = "OU=Usuarios,DC=liotecnica,DC=com,DC=br",
            NetbiosDomain = "LIOTECNICA",
            LoginFormat = "email-or-upn-or-samaccountname",
            BindDn = "CN=servico-hub,OU=ServiceAccounts,DC=liotecnica,DC=com,DC=br",
            ServiceAccountPassword = "Senha@123",
            SearchFilter = "(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))",
            DisplayNameAttribute = "displayName"
        };

        var saveResponse = await _client.PutAsJsonAsync("/api/admin/ldap", ldapConfig);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/ldap/login", new LdapLoginRequest
        {
            Login = "roberto.almeida@liotecnica.com.br",
            Password = "Liotec@2026"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var portalLogin = await loginResponse.Content.ReadFromJsonAsync<PortalLoginResponse>();
        Assert.NotNull(portalLogin);
        Assert.False(string.IsNullOrWhiteSpace(portalLogin.Token));
        Assert.Equal("Roberto Almeida", portalLogin.User.DisplayName);
        Assert.Equal("Recursos Humanos", portalLogin.User.Department);
        Assert.Equal("Analista de RH", portalLogin.User.Title);
        Assert.Equal("Elizabete Rodrigues da Silva", portalLogin.User.ManagerDisplayName);
        Assert.NotEmpty(portalLogin.User.ModulePermissions);
    }

    [Fact]
    public async Task AdminPortalUsersEndpoint_ReturnsPagedUsersAndRecentLoginData()
    {
        await EnsureLdapEnabledAsync();
        await LoginPortalUserAsync();

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var payload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&status=active&department=Recursos%20Humanos&sortBy=displayName&sortDirection=asc&page=1&pageSize=5");

        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Items);
        Assert.Contains(payload.Items, item => item.Login == "roberto.almeida@liotecnica.com.br");
        Assert.True(payload.TotalItems >= 1);
        Assert.True(payload.Summary.LoginEvents >= 1);
        Assert.NotEmpty(payload.RoleOptions);
        Assert.NotEmpty(payload.DepartmentOptions);
        Assert.NotEmpty(payload.RecentLogins);
        Assert.Equal("Recursos Humanos", payload.Department);
        Assert.Equal("displayName", payload.SortBy);
        Assert.Equal("asc", payload.SortDirection);
        Assert.Contains(payload.Items, item => item.ManagerDisplayName == "Elizabete Rodrigues da Silva");
    }

    [Fact]
    public async Task AdminPortalUsersEndpoint_UpdatesRoleAndStatus_AndBlocksInactiveLogin()
    {
        await EnsureLdapEnabledAsync();
        await LoginPortalUserAsync();

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var usersPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&page=1&pageSize=5");
        Assert.NotNull(usersPayload);

        var user = Assert.Single(usersPayload.Items.Where(item => item.Login == "roberto.almeida@liotecnica.com.br"));

        var roleResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/role", new UpdatePortalUserRoleRequest
        {
            Role = "PortalAdmin"
        });
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);

        var rolePayload = await roleResponse.Content.ReadFromJsonAsync<PortalUserAdminDto>();
        Assert.NotNull(rolePayload);
        Assert.Equal("PortalAdmin", rolePayload.Role);
        Assert.Contains("Acessar configuracoes administrativas", rolePayload.Permissions);

        var permissionResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/permissions", new UpdatePortalUserModulePermissionRequest
        {
            ModuleKey = "feed",
            AccessLevel = "Manage"
        });
        Assert.Equal(HttpStatusCode.OK, permissionResponse.StatusCode);

        var permissionPayload = await permissionResponse.Content.ReadFromJsonAsync<PortalUserAdminDto>();
        Assert.NotNull(permissionPayload);
        Assert.Contains(permissionPayload.ModulePermissions, item => item.ModuleKey == "feed" && item.AccessLevel == "Manage");

        var statusResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/status", new UpdatePortalUserStatusRequest
        {
            IsActive = false
        });
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var statusPayload = await statusResponse.Content.ReadFromJsonAsync<PortalUserAdminDto>();
        Assert.NotNull(statusPayload);
        Assert.False(statusPayload.IsActive);

        var refreshedPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&page=1&pageSize=5");
        Assert.NotNull(refreshedPayload);
        Assert.NotEmpty(refreshedPayload.RecentAuditEntries);
        Assert.Contains(refreshedPayload.RecentAuditEntries, item => item.ActionType == "PerfilAlterado");
        Assert.Contains(refreshedPayload.RecentAuditEntries, item => item.ActionType == "PermissaoModuloAlterada");
        Assert.Contains(refreshedPayload.RecentAuditEntries, item => item.ActionType == "StatusAlterado");

        _client.DefaultRequestHeaders.Authorization = null;

        var blockedLoginResponse = await _client.PostAsJsonAsync("/api/auth/ldap/login", new LdapLoginRequest
        {
            Login = "roberto.almeida@liotecnica.com.br",
            Password = "Liotec@2026"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, blockedLoginResponse.StatusCode);
    }

    [Fact]
    public async Task AuthEndpoint_RegistersFailedLoginAndLogoutAuditTrail()
    {
        await EnsureLdapEnabledAsync();

        _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
        _client.DefaultRequestHeaders.Remove("Origin");
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "10.20.30.40");
        _client.DefaultRequestHeaders.Add("Origin", "http://127.0.0.1:3020");

        var failedLoginResponse = await _client.PostAsJsonAsync("/api/auth/ldap/login", new LdapLoginRequest
        {
            Login = "roberto.almeida@liotecnica.com.br",
            Password = "senha-invalida"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, failedLoginResponse.StatusCode);

        var portalLogin = await LoginPortalUserAsync();
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalLogin.Token);

        var logoutResponse = await _client.PostAsync("/api/auth/logout", content: null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var payload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&page=1&pageSize=5");
        Assert.NotNull(payload);
        Assert.Equal(1, payload.Summary.LoginEvents);
        Assert.Equal(1, payload.Summary.FailedLoginEvents);
        Assert.Equal(1, payload.Summary.LogoutEvents);
        Assert.Contains(payload.RecentLogins, item => item.EventType == "LoginFailure" && item.FailureReason == "Credenciais invalidas.");
        Assert.Contains(payload.RecentLogins, item => item.EventType == "Logout");

        var user = Assert.Single(payload.Items.Where(item => item.Login == "roberto.almeida@liotecnica.com.br"));
        Assert.Equal(0, user.FailedLoginCount);
        Assert.Equal("10.20.30.40", user.LastKnownIpAddress);
        Assert.Equal("http://127.0.0.1:3020", user.LastOrigin);
    }

    [Fact]
    public async Task PollsEndpoint_CreatesListsAndReturnsPublishedPoll()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        await PromotePortalUserToHrManagerAsync();
        UsePortalAuth(portalSession);

        var imageUploadUrl = await UploadPollAssetAsync("image", "enquete-home.png", "image/png", [137, 80, 78, 71, 13, 10, 26, 10]);
        var attachmentUploadUrl = await UploadPollAssetAsync("attachment", "diretrizes.pdf", "application/pdf", [37, 80, 68, 70, 45, 49, 46, 55]);

        var createResponse = await _client.PostAsJsonAsync("/api/admin/polls", new UpsertPollRequest
        {
            Title = "Qual iniciativa deve abrir o proximo trimestre?",
            Summary = "Escolha a frente mais prioritaria para os proximos 90 dias.",
            Body = "A enquete apoia o planejamento das frentes internas da LIOCONNECTA.",
            ImageUrl = imageUploadUrl,
            AttachmentLabel = "Baixar diretrizes",
            AttachmentUrl = attachmentUploadUrl,
            Audience = "Toda a companhia",
            Status = "Published",
            AllowMultipleChoices = false,
            ResultsVisibility = "AfterVote",
            IsFeatured = true,
            PublishedAtUtc = new DateTime(2026, 06, 21, 12, 0, 0, DateTimeKind.Utc),
            ClosesAtUtc = new DateTime(2026, 06, 30, 18, 0, 0, DateTimeKind.Utc),
            Options =
            [
                new UpsertPollOptionRequest { Label = "Nova trilha de onboarding" },
                new UpsertPollOptionRequest { Label = "Programa de lideranca" },
                new UpsertPollOptionRequest { Label = "Revamp dos acessos rapidos" }
            ]
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<PollAdminDto>();
        Assert.NotNull(created);
        Assert.Equal("Published", created.Status);
        Assert.Equal(3, created.Options.Count);
        Assert.Equal(imageUploadUrl, created.ImageUrl);
        Assert.Equal("Baixar diretrizes", created.AttachmentLabel);
        Assert.Equal(attachmentUploadUrl, created.AttachmentUrl);

        ClearPortalAuth();

        var publicItems = await _client.GetFromJsonAsync<List<PollDto>>("/api/polls");
        Assert.NotNull(publicItems);
        Assert.Single(publicItems);
        Assert.Equal("Qual iniciativa deve abrir o proximo trimestre?", publicItems[0].Title);
        Assert.Equal(imageUploadUrl, publicItems[0].ImageUrl);
        Assert.Equal("Baixar diretrizes", publicItems[0].AttachmentLabel);

        var uploadedImageResponse = await _client.GetAsync(new Uri(imageUploadUrl).PathAndQuery);
        Assert.Equal(HttpStatusCode.OK, uploadedImageResponse.StatusCode);

        var detailResponse = await _client.GetAsync($"/api/polls/slug/{created.Slug}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
    }

    [Fact]
    public async Task PollsEndpoint_RegistersSingleVoteAndBlocksSecondAttempt()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        await PromotePortalUserToHrManagerAsync();
        UsePortalAuth(portalSession);

        var createResponse = await _client.PostAsJsonAsync("/api/admin/polls", new UpsertPollRequest
        {
            Title = "Qual canal gera mais engajamento interno?",
            Summary = "Mapeamento rapido para priorizar a proxima campanha.",
            Body = "Vote no canal que mais impulsiona a comunicacao interna hoje.",
            Audience = "Toda a companhia",
            Status = "Published",
            AllowMultipleChoices = false,
            ResultsVisibility = "AfterVote",
            IsFeatured = false,
            PublishedAtUtc = new DateTime(2026, 06, 21, 12, 0, 0, DateTimeKind.Utc),
            ClosesAtUtc = new DateTime(2026, 06, 29, 18, 0, 0, DateTimeKind.Utc),
            Options =
            [
                new UpsertPollOptionRequest { Label = "Portal" },
                new UpsertPollOptionRequest { Label = "Teams" }
            ]
        });

        var poll = await createResponse.Content.ReadFromJsonAsync<PollAdminDto>();
        Assert.NotNull(poll);

        var voteResponse = await _client.PostAsJsonAsync($"/api/polls/{poll.Id}/vote", new SubmitPollVoteRequest
        {
            OptionIds = [poll.Options[0].Id]
        });

        Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);
        var voted = await voteResponse.Content.ReadFromJsonAsync<PollDto>();
        Assert.NotNull(voted);
        Assert.True(voted.HasVoted);
        Assert.True(voted.ResultsVisible);
        Assert.Equal(1, voted.TotalVotes);
        Assert.True(voted.Options[0].IsSelected);
        Assert.Equal(100, voted.Options[0].Percentage);

        var secondVoteResponse = await _client.PostAsJsonAsync($"/api/polls/{poll.Id}/vote", new SubmitPollVoteRequest
        {
            OptionIds = [poll.Options[1].Id]
        });

        Assert.Equal(HttpStatusCode.BadRequest, secondVoteResponse.StatusCode);
    }

    [Fact]
    public async Task PollsAdminEndpoint_RequiresPollAdminPermissionForPortalUsers()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        UsePortalAuth(portalSession);

        var forbiddenResponse = await _client.GetAsync("/api/admin/polls");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        await PromotePortalUserToHrManagerAsync();

        var allowedResponse = await _client.GetAsync("/api/admin/polls");
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task NotificationsEndpoint_ListsRealPersistedSourcesAndMarksRead()
    {
        await EnsureLdapEnabledAsync();

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var communicationResponse = await _client.PostAsJsonAsync("/api/communications", new UpsertCommunicationRequest
        {
            Category = "RH",
            Priority = "Alta prioridade",
            Title = "Novo comunicado para notificacao",
            Summary = "Resumo persistido que deve aparecer como notificacao.",
            Body = "Corpo persistido.",
            Audience = "Toda a companhia",
            Channel = "Portal",
            Status = "Publicado",
            AttachmentLabel = "Abrir comunicado",
            Owner = "Recursos Humanos",
            IsFeatured = false,
            PublishedAt = new DateTime(2026, 06, 24, 10, 0, 0, DateTimeKind.Utc)
        });
        Assert.Equal(HttpStatusCode.Created, communicationResponse.StatusCode);

        var portalSession = await LoginPortalUserAsync();
        await PromotePortalUserToHrManagerAsync();
        UsePortalAuth(portalSession);

        var pollResponse = await _client.PostAsJsonAsync("/api/admin/polls", new UpsertPollRequest
        {
            Title = "Enquete real para notificacao",
            Summary = "Resumo da enquete persistida.",
            Body = "Corpo da enquete.",
            Audience = "Toda a companhia",
            Status = "Published",
            AllowMultipleChoices = false,
            ResultsVisibility = "AfterVote",
            IsFeatured = false,
            PublishedAtUtc = new DateTime(2026, 06, 24, 11, 0, 0, DateTimeKind.Utc),
            Options =
            [
                new UpsertPollOptionRequest { Label = "Opcao A" },
                new UpsertPollOptionRequest { Label = "Opcao B" }
            ]
        });
        Assert.Equal(HttpStatusCode.Created, pollResponse.StatusCode);

        ClearPortalAuth();
        UsePortalAuth(portalSession);

        var notifications = await _client.GetFromJsonAsync<NotificationListResponse>("/api/notifications");

        Assert.NotNull(notifications);
        Assert.Equal(2, notifications.Summary.TotalCount);
        Assert.Equal(2, notifications.Summary.UnreadCount);
        Assert.Contains(notifications.Items, item => item.SourceType == "communication" && item.Title == "Novo comunicado para notificacao");
        Assert.Contains(notifications.Items, item => item.SourceType == "poll" && item.Title == "Enquete real para notificacao");
        Assert.True(notifications.Summary.CategoryCounts.ContainsKey("RH"));
        Assert.True(notifications.Summary.CategoryCounts.ContainsKey("Enquetes"));

        var firstNotificationId = notifications.Items[0].Id;
        var markReadResponse = await _client.PostAsync($"/api/notifications/{firstNotificationId}/read", content: null);
        Assert.Equal(HttpStatusCode.NoContent, markReadResponse.StatusCode);

        var refreshed = await _client.GetFromJsonAsync<NotificationListResponse>("/api/notifications");

        Assert.NotNull(refreshed);
        Assert.Equal(2, refreshed.Summary.TotalCount);
        Assert.Equal(1, refreshed.Summary.UnreadCount);
        Assert.Contains(refreshed.Items, item => item.Id == firstNotificationId && item.IsRead);

        var markAllResponse = await _client.PostAsync("/api/notifications/read-all", content: null);
        Assert.Equal(HttpStatusCode.OK, markAllResponse.StatusCode);

        var allRead = await _client.GetFromJsonAsync<NotificationListResponse>("/api/notifications");
        Assert.NotNull(allRead);
        Assert.Equal(0, allRead.Summary.UnreadCount);
        Assert.All(allRead.Items, item => Assert.True(item.IsRead));
    }

    [Fact]
    public async Task MoodSurveyEndpoint_RegistersDailyVoteWithAuditTrailAndBlocksSecondAttempt()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Remove("X-Forwarded-For");
        _client.DefaultRequestHeaders.Remove("Origin");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "10.30.40.50");
        _client.DefaultRequestHeaders.Add("Origin", "http://127.0.0.1:3020");

        var initial = await _client.GetFromJsonAsync<MoodSurveyTodayResponse>("/api/mood-survey/today");
        Assert.NotNull(initial);
        Assert.False(initial.HasVoted);
        Assert.Equal(3, initial.Items.Count);
        Assert.All(initial.Items, item => Assert.Equal(0, item.VoteCount));

        var voteResponse = await _client.PostAsJsonAsync("/api/mood-survey/vote", new SubmitMoodSurveyVoteRequest
        {
            OptionKey = "motivated"
        });
        Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);

        var voted = await voteResponse.Content.ReadFromJsonAsync<MoodSurveyTodayResponse>();
        Assert.NotNull(voted);
        Assert.True(voted.HasVoted);
        Assert.Equal("motivated", voted.SelectedOptionKey);
        Assert.False(string.IsNullOrWhiteSpace(voted.ThankYouMessage));
        Assert.Equal(1, voted.Items.First(item => item.Key == "motivated").VoteCount);

        var refreshed = await _client.GetFromJsonAsync<MoodSurveyTodayResponse>("/api/mood-survey/today");
        Assert.NotNull(refreshed);
        Assert.True(refreshed.HasVoted);
        Assert.Equal("motivated", refreshed.SelectedOptionKey);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.ThankYouMessage));

        var secondVoteResponse = await _client.PostAsJsonAsync("/api/mood-survey/vote", new SubmitMoodSurveyVoteRequest
        {
            OptionKey = "good"
        });
        Assert.Equal(HttpStatusCode.BadRequest, secondVoteResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var auditEntries = await dbContext.MoodSurveyAuditLogs
            .Where(item => item.PortalUserId == portalSession.User.Id)
            .ToListAsync();

        Assert.Single(auditEntries);
        Assert.Equal("HumorRegistrado", auditEntries[0].ActionType);
        Assert.Equal("motivated", auditEntries[0].OptionKey);
        Assert.Equal("10.30.40.50", auditEntries[0].IpAddress);
        Assert.Equal("http://127.0.0.1:3020", auditEntries[0].Origin);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var usersPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?page=1&pageSize=5");
        Assert.NotNull(usersPayload);
        Assert.Equal(1, usersPayload.Summary.MoodSurveyEvents);
        Assert.Contains(usersPayload.RecentMoodSurveyEntries, item => item.OptionKey == "motivated");
    }

    [Fact]
    public async Task MoodSurveyDashboardEndpoint_ReturnsDistributionByDepartmentAndPeriod()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var voteResponse = await _client.PostAsJsonAsync("/api/mood-survey/vote", new SubmitMoodSurveyVoteRequest
        {
            OptionKey = "good"
        });
        Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");

        var dashboard = await _client.GetFromJsonAsync<MoodSurveyDashboardResponse>("/api/admin/mood-survey/dashboard");
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.Summary.TotalVotes);
        Assert.Equal(1, dashboard.Summary.GoodCount);
        Assert.Contains(dashboard.Departments, item => item.Department == "Recursos Humanos");
        Assert.NotEmpty(dashboard.DailyTrend);
    }

    [Fact]
    public async Task MoodSurveyDashboardEndpoint_RequiresHrPermissionForPortalUsers()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var forbiddenResponse = await _client.GetAsync("/api/mood-survey/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var usersPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>("/api/admin/portal-users?query=roberto&page=1&pageSize=5");
        var user = Assert.Single(usersPayload!.Items.Where(item => item.Login == "roberto.almeida@liotecnica.com.br"));

        var roleResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/role", new UpdatePortalUserRoleRequest
        {
            Role = "HrManager"
        });
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var allowedResponse = await _client.GetAsync("/api/mood-survey/dashboard");
        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
    }

    [Fact]
    public async Task MoodSurveyFeedbackEndpoint_SeedsMessagesSupportsCrudAndRandomVoteFeedback()
    {
        await EnsureLdapEnabledAsync();
        var portalSession = await LoginPortalUserAsync();
        await PromotePortalUserToHrManagerAsync();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var seededMessages = await _client.GetFromJsonAsync<MoodSurveyFeedbackMessageListResponse>("/api/mood-survey/feedback-messages");
        Assert.NotNull(seededMessages);
        Assert.Equal(60, seededMessages.Items.Count);
        Assert.Equal(20, seededMessages.Items.Count(item => item.OptionKey == "motivated"));
        Assert.Equal(20, seededMessages.Items.Count(item => item.OptionKey == "good"));
        Assert.Equal(20, seededMessages.Items.Count(item => item.OptionKey == "tired"));

        var createResponse = await _client.PostAsJsonAsync("/api/mood-survey/feedback-messages", new UpsertMoodSurveyFeedbackMessageRequest
        {
            OptionKey = "motivated",
            Message = "Mensagem personalizada de teste.",
            SortOrder = 99,
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<MoodSurveyFeedbackMessageDto>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/mood-survey/feedback-messages/{created.Id}", new UpsertMoodSurveyFeedbackMessageRequest
        {
            OptionKey = "motivated",
            Message = "Mensagem personalizada atualizada.",
            SortOrder = 100,
            IsActive = true
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var voteResponse = await _client.PostAsJsonAsync("/api/mood-survey/vote", new SubmitMoodSurveyVoteRequest
        {
            OptionKey = "motivated"
        });
        Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);

        var voted = await voteResponse.Content.ReadFromJsonAsync<MoodSurveyTodayResponse>();
        Assert.NotNull(voted);
        Assert.False(string.IsNullOrWhiteSpace(voted.ThankYouMessage));
        Assert.Contains(voted.ThankYouMessage, seededMessages.Items.Select(item => item.Message).Append("Mensagem personalizada atualizada."));

        var deleteResponse = await _client.DeleteAsync($"/api/mood-survey/feedback-messages/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AgendaEndpoint_ReturnsPersistedEventsForAuthenticatedPortalUser()
    {
        await EnsureLdapEnabledAsync();

        var portalSession = await LoginPortalUserAsync();
        await SeedAgendaEventsAsync(portalSession.User.Id);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);

        var agenda = await _client.GetFromJsonAsync<AgendaDayResponse>("/api/agenda");

        Assert.NotNull(agenda);
        Assert.Equal(2, agenda.TotalCount);
        Assert.Contains(agenda.Items, item => item.Title == "Daily RH");
        Assert.Contains(agenda.Items, item => item.Title == "Comite de Pessoas" && item.Location == "Microsoft Teams");
        Assert.DoesNotContain(agenda.Items, item => item.Title == "Evento de outro colaborador");
    }

    private async Task<AdminLoginResponse> LoginAdminAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/admin/auth/login", new AdminLoginRequest("super-admin", "Liotec@2026"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var adminSession = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(adminSession);
        return adminSession;
    }

    private async Task PromotePortalUserToHrManagerAsync(string login = "roberto.almeida@liotecnica.com.br")
    {
        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var usersPayload = await _client.GetFromJsonAsync<PortalUserAdminListResponse>($"/api/admin/portal-users?query={Uri.EscapeDataString(login)}&page=1&pageSize=5");
        var user = Assert.Single(usersPayload!.Items.Where(item => item.Login == login));

        var roleResponse = await _client.PatchAsJsonAsync($"/api/admin/portal-users/{user.Id}/role", new UpdatePortalUserRoleRequest
        {
            Role = "HrManager"
        });
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    private void UsePortalAuth(PortalLoginResponse portalSession)
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", portalSession.Token);
    }

    private void ClearPortalAuth()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
    }

    private async Task<string> UploadFeedAssetAsync(string fileName, string contentType, byte[] bytes)
    {
        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        formData.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync("/api/feed/assets", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<FeedAssetUploadResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Url));
        return payload.Url;
    }

    private async Task<string> UploadPollAssetAsync(string assetType, string fileName, string contentType, byte[] bytes)
    {
        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        formData.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync($"/api/admin/polls/assets/{assetType}", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PollAssetUploadResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Url));
        return payload.Url;
    }

    private async Task EnsureLdapEnabledAsync()
    {
        var adminSession = await LoginAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminSession.Token);

        var ldapConfig = new UpsertLdapConfigurationRequest
        {
            IsEnabled = true,
            Server = "dc-virtual-02.liotecnica.com.br",
            Port = 389,
            UseLdaps = false,
            UseStartTls = true,
            IgnoreCertificateValidation = true,
            BaseDn = "DC=liotecnica,DC=com,DC=br",
            UserSearchBase = "OU=Usuarios,DC=liotecnica,DC=com,DC=br",
            NetbiosDomain = "LIOTECNICA",
            LoginFormat = "email-or-upn-or-samaccountname",
            BindDn = "CN=servico-hub,OU=ServiceAccounts,DC=liotecnica,DC=com,DC=br",
            ServiceAccountPassword = "Senha@123",
            SearchFilter = "(|(mail={0})(userPrincipalName={0})(sAMAccountName={0}))",
            DisplayNameAttribute = "displayName"
        };

        var saveResponse = await _client.PutAsJsonAsync("/api/admin/ldap", ldapConfig);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<Guid> SeedFeedPostForAnotherPortalUserAsync(string text)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var referenceUser = await dbContext.PortalUsers
            .AsNoTracking()
            .FirstAsync(item => item.Login == "roberto.almeida@liotecnica.com.br");

        var otherUserId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        dbContext.PortalUsers.Add(new PortalUser
        {
            Id = otherUserId,
            Login = "colega.teste@liotecnica.com.br",
            DisplayName = "Colega Teste",
            Department = "Sistemas",
            Role = referenceUser.Role,
            ModulePermissionsJson = referenceUser.ModulePermissionsJson,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        dbContext.FeedPosts.Add(new FeedPost
        {
            Id = postId,
            PortalUserId = otherUserId,
            Text = text,
            CreatedAtUtc = now
        });

        await dbContext.SaveChangesAsync();
        return postId;
    }

    private async Task<PortalLoginResponse> LoginPortalUserAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/ldap/login", new LdapLoginRequest
        {
            Login = "roberto.almeida@liotecnica.com.br",
            Password = "Liotec@2026"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var portalLogin = await loginResponse.Content.ReadFromJsonAsync<PortalLoginResponse>();
        Assert.NotNull(portalLogin);
        return portalLogin;
    }

    private async Task SeedAgendaEventsAsync(Guid portalUserId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var timeZone = ResolveSaoPauloTimeZoneForTests();
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        (DateTime StartAtUtc, DateTime EndAtUtc) ToUtcFromNow(double addHours, double durationHours)
        {
            var startLocal = nowLocal.AddHours(addHours);
            var endLocal = startLocal.AddHours(durationHours);
            return (
                TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), timeZone),
                TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), timeZone));
        }

        var daily = ToUtcFromNow(1, 0.5);
        var comite = ToUtcFromNow(2, 1);
        var otherUser = ToUtcFromNow(3, 1);

        dbContext.AgendaEvents.AddRange(
            new AgendaEvent
            {
                Id = Guid.NewGuid(),
                PortalUserId = null,
                Title = "Daily RH",
                Description = "Alinhamento diario do time.",
                Location = "Sala RH",
                Source = "Manual",
                Audience = "Toda a companhia",
                IsActive = true,
                StartAtUtc = daily.StartAtUtc,
                EndAtUtc = daily.EndAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            },
            new AgendaEvent
            {
                Id = Guid.NewGuid(),
                PortalUserId = portalUserId,
                Title = "Comite de Pessoas",
                Description = "Pauta semanal de pessoas.",
                Location = "Microsoft Teams",
                Source = "Manual",
                Audience = "Usuario autenticado",
                IsActive = true,
                StartAtUtc = comite.StartAtUtc,
                EndAtUtc = comite.EndAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            },
            new AgendaEvent
            {
                Id = Guid.NewGuid(),
                PortalUserId = Guid.NewGuid(),
                Title = "Evento de outro colaborador",
                Description = "Nao deve aparecer para o usuario logado.",
                Location = "Sala 2",
                Source = "Manual",
                Audience = "Outro usuario",
                IsActive = true,
                StartAtUtc = otherUser.StartAtUtc,
                EndAtUtc = otherUser.EndAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();
    }

    private static TimeZoneInfo ResolveSaoPauloTimeZoneForTests()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
    }

    private static async Task ResetDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalRhDbContext>();
        var adminAuthService = scope.ServiceProvider.GetRequiredService<IAdminAuthService>();
        var ldapConfigurationService = scope.ServiceProvider.GetRequiredService<ILdapConfigurationService>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        await adminAuthService.EnsureDefaultSuperAdminAsync(CancellationToken.None);
        await ldapConfigurationService.EnsureDefaultConfigurationAsync(CancellationToken.None);

        var moodSurveyFeedbackService = scope.ServiceProvider.GetRequiredService<IMoodSurveyFeedbackService>();
        await moodSurveyFeedbackService.EnsureSeedAsync(CancellationToken.None);

        var quickLinkService = scope.ServiceProvider.GetRequiredService<IQuickLinkService>();
        await quickLinkService.EnsureSeedAsync(CancellationToken.None);

        var portalUserSeedService = scope.ServiceProvider.GetRequiredService<IPortalUserSeedService>();
        await portalUserSeedService.EnsureSeedAsync(CancellationToken.None);
    }
}
