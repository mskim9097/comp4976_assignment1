namespace MunicipalElections.Web.Areas.Admin.Models;

public class DashboardViewModel
{
    public int MunicipalityCount { get; set; }
    public int PositionCount { get; set; }
    public int CandidateCount { get; set; }
    public bool IsSuperAdmin { get; set; }
    public List<string> MunicipalityNames { get; set; } = [];
}
