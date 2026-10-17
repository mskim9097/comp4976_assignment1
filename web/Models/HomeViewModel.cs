namespace MunicipalElections.Web.Models;

public class HomeViewModel
{
    public List<HomeMunicipalityViewModel> Municipalities { get; set; } = [];
}

public class HomeMunicipalityViewModel
{
    public int MunicipalityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateTime ElectionDate { get; set; }
    public string? Description { get; set; }
    public string? LogoFileName { get; set; }
    public int CandidateCount { get; set; }
}
