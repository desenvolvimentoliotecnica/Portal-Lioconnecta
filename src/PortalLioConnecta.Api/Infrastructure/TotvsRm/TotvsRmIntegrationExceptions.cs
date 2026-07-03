namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public abstract class TotvsRmIntegrationException : Exception
{
    protected TotvsRmIntegrationException(string message)
        : base(message)
    {
    }
}

public sealed class TotvsRmIntegrationDisabledException : TotvsRmIntegrationException
{
    public TotvsRmIntegrationDisabledException()
        : base("Integracao TOTVS RM desabilitada.")
    {
    }
}

public sealed class TotvsRmIntegrationMisconfiguredException : TotvsRmIntegrationException
{
    public TotvsRmIntegrationMisconfiguredException(string message)
        : base(message)
    {
    }
}

public sealed class TotvsRmIntegrationUnavailableException : TotvsRmIntegrationException
{
    public TotvsRmIntegrationUnavailableException()
        : base("Integracao TOTVS RM indisponivel.")
    {
    }
}
