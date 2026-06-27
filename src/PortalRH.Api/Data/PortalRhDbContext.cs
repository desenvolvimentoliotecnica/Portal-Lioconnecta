using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Models;

namespace PortalRH.Api.Data;

public class PortalRhDbContext : DbContext
{
    public PortalRhDbContext(DbContextOptions<PortalRhDbContext> options)
        : base(options)
    {
    }

    public DbSet<Communication> Communications => Set<Communication>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminSession> AdminSessions => Set<AdminSession>();
    public DbSet<LdapConfiguration> LdapConfigurations => Set<LdapConfiguration>();
    public DbSet<MicrosoftGraphConfiguration> MicrosoftGraphConfigurations => Set<MicrosoftGraphConfiguration>();
    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();
    public DbSet<PortalSession> PortalSessions => Set<PortalSession>();
    public DbSet<PortalUserLoginEvent> PortalUserLoginEvents => Set<PortalUserLoginEvent>();
    public DbSet<PortalUserAdminAuditLog> PortalUserAdminAuditLogs => Set<PortalUserAdminAuditLog>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PortalUserNotificationRead> PortalUserNotificationReads => Set<PortalUserNotificationRead>();
    public DbSet<AgendaEvent> AgendaEvents => Set<AgendaEvent>();
    public DbSet<MoodSurveyVote> MoodSurveyVotes => Set<MoodSurveyVote>();
    public DbSet<MoodSurveyAuditLog> MoodSurveyAuditLogs => Set<MoodSurveyAuditLog>();
    public DbSet<MoodSurveyFeedbackMessage> MoodSurveyFeedbackMessages => Set<MoodSurveyFeedbackMessage>();
    public DbSet<CommunicationLike> CommunicationLikes => Set<CommunicationLike>();
    public DbSet<CommunicationShare> CommunicationShares => Set<CommunicationShare>();
    public DbSet<CommunicationInteractionAuditLog> CommunicationInteractionAuditLogs => Set<CommunicationInteractionAuditLog>();
    public DbSet<FeedPost> FeedPosts => Set<FeedPost>();
    public DbSet<FeedPostAuditLog> FeedPostAuditLogs => Set<FeedPostAuditLog>();
    public DbSet<FeedPostLike> FeedPostLikes => Set<FeedPostLike>();
    public DbSet<FeedPostShare> FeedPostShares => Set<FeedPostShare>();
    public DbSet<FeedPostSave> FeedPostSaves => Set<FeedPostSave>();
    public DbSet<CommunicationSave> CommunicationSaves => Set<CommunicationSave>();
    public DbSet<FeedPostMedia> FeedPostMedia => Set<FeedPostMedia>();
    public DbSet<FeedPostMediaComment> FeedPostMediaComments => Set<FeedPostMediaComment>();
    public DbSet<FeedPostComment> FeedPostComments => Set<FeedPostComment>();
    public DbSet<FeedPostCommentMention> FeedPostCommentMentions => Set<FeedPostCommentMention>();
    public DbSet<FeedPostMention> FeedPostMentions => Set<FeedPostMention>();
    public DbSet<QuickLink> QuickLinks => Set<QuickLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortalRhDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
