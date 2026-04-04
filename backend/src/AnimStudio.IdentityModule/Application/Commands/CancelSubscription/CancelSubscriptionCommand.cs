using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid TeamId, bool Immediately = false) : IRequest<Result<bool>>;
