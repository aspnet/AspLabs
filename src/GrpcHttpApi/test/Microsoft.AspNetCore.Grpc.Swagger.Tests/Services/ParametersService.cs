using System.Threading.Tasks;
using Grpc.Core;
using Params;

namespace Microsoft.AspNetCore.Grpc.Swagger.Tests.Services;

public class ParametersService : Parameters.ParametersBase
{
    public override Task<Response> DemoParametersOne(RequestOne requestId, ServerCallContext ctx)
    {
        return Task.FromResult(new Response {Message = "DemoParametersOne Response"});
    }
    
    public override Task<Response> DemoParametersTwo(RequestOne requestId, ServerCallContext ctx)
    {
        return Task.FromResult(new Response {Message = "DemoParametersTwo Response"});
    }

    public override Task<Response> DemoParametersThree(RequestTwo request, ServerCallContext ctx)
    {
        return Task.FromResult(new Response {Message = "DemoParametersThree Response "});
    }
    
    public override Task<Response> DemoParametersFour(RequestTwo request, ServerCallContext ctx)
    {
        return Task.FromResult(new Response {Message = "DemoParametersFour Response"});
    }
}