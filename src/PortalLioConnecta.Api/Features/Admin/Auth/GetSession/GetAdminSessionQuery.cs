using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.Auth;

namespace PortalLioConnecta.Api.Features.Admin.Auth.GetSession;

public sealed record GetAdminSessionQuery(string Token) : IRequest<AdminSessionDto?>;
