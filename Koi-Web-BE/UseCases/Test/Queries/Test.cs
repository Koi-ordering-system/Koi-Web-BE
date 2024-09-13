using MediatR;

namespace Koi_Web_BE.UseCases.Test.Queries;

public abstract class Test
{
    public class Query : IRequest<Response>;

    public class Response;
    
    public class Handler : IRequestHandler<Query, Response>
    {
        public Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Response());
        }
    }
}