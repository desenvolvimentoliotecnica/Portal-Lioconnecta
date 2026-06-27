namespace PortalRH.Api.Domain;

public static class FeedPostAuditActionTypes
{
    public const string PostCreated = "PublicacaoRegistrada";
    public const string PostDeleted = "PublicacaoRemovida";
    public const string LikeRegistered = "CurtidaRegistrada";
    public const string LikeRemoved = "CurtidaRemovida";
    public const string ShareRegistered = "CompartilhamentoRegistrado";
    public const string ShareRemoved = "CompartilhamentoRemovido";
    public const string SaveRegistered = "SalvamentoRegistrado";
    public const string SaveRemoved = "SalvamentoRemovido";
}

public static class FeedItemSources
{
    public const string UserPost = "UserPost";
    public const string Communication = "Communication";
}
