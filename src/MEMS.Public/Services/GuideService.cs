using System.Text.Json;
using MEMS.Shared;
using MEMS.Shared.Dtos;
using Microsoft.JSInterop;

namespace MEMS.Public.Services;

public record GuideEntry(int CandidateId, string Name, int PositionId, string PositionName, string? PhotoUrl);

public class GuideState
{
    public string? Slug { get; set; }
    public string? MunicipalityName { get; set; }
    public DateOnly? ElectionDate { get; set; }
    public List<GuideEntry> Entries { get; set; } = [];
}

// The voter's personal guide.
// Election privacy: the choices live only in the browser's session storage. They are never written to the database
// and never sent to the Api (this class has no HttpClient), so nobody on the server side can see them, and the browser
// removes them when the session ends (the tab is closed).
public class GuideService(IJSRuntime js)
{
    private const string Key = "mems.guide";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public event Action? Changed;

    public async Task<GuideState> GetAsync()
    {
        try
        {
            var raw = await js.InvokeAsync<string?>("sessionStorage.getItem", Key);
            return raw is null ? new GuideState() : JsonSerializer.Deserialize<GuideState>(raw, Json) ?? new GuideState();
        }
        catch (JsonException)
        {
            return new GuideState();
        }
    }

    // Returns the reason when the candidate cannot be added, otherwise null.
    public async Task<string?> AddAsync(MunicipalityDto municipality, CandidateDto candidate, IReadOnlyList<PositionDto> positions)
    {
        var guide = await GetAsync();
        var seats = positions.FirstOrDefault(p => p.PositionId == candidate.PositionId)?.SeatsAvailable ?? 0;
        var choices = guide.Entries.Select(e => new SelectionRules.Choice(e.CandidateId, e.PositionId)).ToList();

        var problem = SelectionRules.CheckAdd(
            guide.MunicipalityName, choices, municipality.Name, new SelectionRules.Choice(candidate.CandidateId, candidate.PositionId),
            candidate.PositionName, seats);
        if (problem is not null)
        {
            return problem;
        }

        // The first choice decides which municipality the guide is for.
        guide.Slug = municipality.Slug;
        guide.MunicipalityName = municipality.Name;
        guide.ElectionDate = municipality.ElectionDate;
        guide.Entries.Add(new GuideEntry(candidate.CandidateId, candidate.Name, candidate.PositionId, candidate.PositionName, candidate.PhotoUrl));
        await SaveAsync(guide);
        return null;
    }

    public async Task RemoveAsync(int candidateId)
    {
        var guide = await GetAsync();
        guide.Entries.RemoveAll(e => e.CandidateId == candidateId);
        if (guide.Entries.Count == 0)
        {
            guide = new GuideState();
        }
        await SaveAsync(guide);
    }

    public async Task ClearAsync() => await SaveAsync(new GuideState());

    private async Task SaveAsync(GuideState guide)
    {
        if (guide.Entries.Count == 0)
        {
            await js.InvokeVoidAsync("sessionStorage.removeItem", Key);
        }
        else
        {
            await js.InvokeVoidAsync("sessionStorage.setItem", Key, JsonSerializer.Serialize(guide, Json));
        }
        Changed?.Invoke();
    }
}
