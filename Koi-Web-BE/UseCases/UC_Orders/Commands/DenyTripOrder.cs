using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.UseCases.UC_Orders.Commands;

public class DenyTripOrder
{
    public record Response();

    public record Command(Guid Id) : IRequest<Result<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.OrderTrips
                .Where(ot => ot.Id.Equals(request.Id))
                .ExecuteUpdateAsync(e => e.SetProperty(ot => ot.IsApproved, false), cancellationToken);
            if (result == 0) return Result<Response>.Fail(new NotFoundException("Order not found."));
            return Result<Response>.Succeed(null!);
        }
    }
}