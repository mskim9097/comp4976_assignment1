using System.Text.Json;
using Microsoft.JSInterop;

namespace MEMS.Public.Services;

// One chosen candidate. Everything needed to print the guide is kept here, so printing never calls the server.
public record BallotItem(
    string Slug, string MunicipalityName, string ElectionDate,
    int CandidateId, string CandidateName, string PositionName, string? PhotoUrl);

// ELECTION PRIVACY: a visitor's choices are kept ONLY in this browser tab's Session Storage.
//  - never written to SQL Server, never sent to any API (the only HTTP calls this site makes are GET requests for public data)
//  - removed automatically by the browser when the tab / session ends
//  - an administrator has no way to see them: they never leave the visitor's browser
//
// Visitors may choose any number of candidates, for any position (several mayors, for example), in any number of
// municipalities, so the list is kept per browser session and printed grouped by municipality.
public class BallotStore(IJSRuntime js)
{
    private const string StorageKey = "mems.ballot.v1";
    private List<BallotItem> _items = [];
    private bool _loaded;

    public event Action? Changed;

    public IReadOnlyList<BallotItem> Items => _items;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var json = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            _items = string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<BallotItem>>(json) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or JSException)
        {
            _items = []; // unreadable data is ignored, the visitor simply starts with an empty list
        }
    }

    public bool Contains(string slug, int candidateId) =>
        _items.Any(i => i.CandidateId == candidateId && i.Slug == slug);

    public async Task AddAsync(BallotItem item)
    {
        if (Contains(item.Slug, item.CandidateId)) return;
        _items = [.. _items, item];
        await SaveAsync();
    }

    public async Task RemoveAsync(string slug, int candidateId)
    {
        _items = _items.Where(i => !(i.CandidateId == candidateId && i.Slug == slug)).ToList();
        await SaveAsync();
    }

    public async Task ClearAsync()
    {
        _items = [];
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(_items));
        }
        catch (JSException)
        {
            // Storage blocked or full: the list still works for this page view.
        }
        Changed?.Invoke();
    }
}
