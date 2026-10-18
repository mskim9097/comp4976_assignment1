using Microsoft.AspNetCore.Mvc.Rendering;

namespace MunicipalElections.Web.Models;

public class PositionBrowseViewModel
{
    public int? MunicipalityId { get; set; }
    public List<SelectListItem> Municipalities { get; set; } = [];
    public List<PositionBrowseMunicipality> Groups { get; set; } = [];
}

public class PositionBrowseMunicipality
{
    public Municipality Municipality { get; set; } = null!;
    public List<PositionBrowseItem> Positions { get; set; } = [];
}

public class PositionBrowseItem
{
    public Position Position { get; set; } = null!;
    public int CandidateCount { get; set; }
}
