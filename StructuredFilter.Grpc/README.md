# StructuredFilter.Grpc

* Implement gRPC's FilterManager to query filter information, example:

```csharp
public class FilterManager : v1.FilterManager.FilterManagerBase
{
    public override Task<GetFiltersResponse> GetFilters(EmptyRequest request, ServerCallContext context)
    {
        var resp = new GetFiltersResponse();
        // The key is the filter target, and the value is the JSON serialized string of the filter metadata dictionary
        resp.FilterInfos.Add("Players", JsonSerializer.Serialize(playerFilterService.GetSceneFilterInfos()));
        resp.FilterInfos.Add("Teams", JsonSerializer.Serialize(teamFilterService.GetSceneFilterInfos()));

        return Task.FromResult(resp);
    }
}
```