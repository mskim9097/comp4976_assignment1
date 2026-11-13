using MEMS.Shared;

namespace MEMS.Tests;

public class SharedRuleTests
{
    private static SelectionRules.Choice C(int candidate, int position) => new(candidate, position);

    [Fact]
    public void A_position_accepts_at_most_its_seats()
    {
        // one mayor
        var selected = new[] { C(1, 10) };
        var message = SelectionRules.CheckAdd("Burnaby", selected, "Burnaby", C(2, 10), "Mayor", 1);
        Assert.Contains("Mayor in Burnaby has 1 seat", message);

        // eight councillors: the ninth is refused
        var councillors = Enumerable.Range(1, 8).Select(i => C(i, 20)).ToArray();
        Assert.Null(SelectionRules.CheckAdd("Burnaby", councillors.Take(7).ToArray(), "Burnaby", C(8, 20), "Councillor", 8));
        Assert.NotNull(SelectionRules.CheckAdd("Burnaby", councillors, "Burnaby", C(9, 20), "Councillor", 8));
    }

    [Fact]
    public void Positions_are_counted_separately()
    {
        var selected = new[] { C(1, 10) };
        Assert.Null(SelectionRules.CheckAdd("Burnaby", selected, "Burnaby", C(2, 20), "Councillor", 8));
    }

    [Fact]
    public void A_guide_belongs_to_one_municipality()
    {
        var selected = new[] { C(1, 10) };
        var message = SelectionRules.CheckAdd("Burnaby", selected, "Surrey", C(2, 99), "Mayor", 1);
        Assert.Contains("one municipality (Burnaby)", message);
        // An empty guide can start in any municipality.
        Assert.Null(SelectionRules.CheckAdd(null, [], "Surrey", C(2, 99), "Mayor", 1));
    }

    [Fact]
    public void The_same_candidate_cannot_be_added_twice()
    {
        Assert.NotNull(SelectionRules.CheckAdd("Burnaby", [C(1, 10)], "Burnaby", C(1, 10), "Mayor", 5));
    }

    [Theory]
    [InlineData("burnaby", true)]
    [InlineData("pitt-meadows", true)]
    [InlineData("surrey2", true)]
    [InlineData("Burnaby", false)]
    [InlineData("bad slug", false)]
    [InlineData("-start", false)]
    [InlineData("end-", false)]
    [InlineData("double--hyphen", false)]
    [InlineData("", false)]
    public void Slug_format(string slug, bool valid) => Assert.Equal(valid, TenantSlug.IsValid(slug));

    [Theory]
    [InlineData("auth", true)]
    [InlineData("admin", true)]
    [InlineData("candidates", true)]
    [InlineData("burnaby", false)]
    public void Reserved_slugs(string slug, bool reserved) => Assert.Equal(reserved, TenantSlug.IsReserved(slug));

    [Theory]
    [InlineData("Pitt Meadows", "pitt-meadows")]
    [InlineData("  Maple   Ridge ", "maple-ridge")]
    [InlineData("Port Coquitlam!", "port-coquitlam")]
    public void Slug_suggestion(string name, string expected) => Assert.Equal(expected, TenantSlug.FromName(name));

    [Fact]
    public void Municipality_names_are_tidied_and_provinces_checked()
    {
        Assert.Equal("Maple Ridge", Provinces.NormalizeName("  Maple   Ridge "));
        Assert.True(Provinces.IsValid("British Columbia"));
        Assert.False(Provinces.IsValid("BC"));
    }
}
