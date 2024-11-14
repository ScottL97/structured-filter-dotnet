# StructuredFilter.Grpc

* Implement gRPC's FilterManager to query filter information, example:

```csharp
public class FilterManager : v1.FilterManager.FilterManagerBase
{
    public override async Task<GetFiltersResponse> GetFilters(GetFiltersRequest request, ServerCallContext context)
    {
        return request.ServiceType switch
        {
            "PlayerService" => new GetFiltersResponse
            {
                FilterInfo = JsonSerializer.Serialize(playerFilterService.GetSceneFilterInfos())
            },
            "TeamService" => new GetFiltersResponse
            {
                FilterInfo = JsonSerializer.Serialize(teamFilterService.GetSceneFilterInfos())
            },
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "wrong service type"))
        };
    }
}
```