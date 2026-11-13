namespace MEMS.Shared;

// Field limits used by the forms (client), the API (server) and the entity configuration (database),
// so the three layers always agree.
public static class Limits
{
    public const int NameMax = 100;
    public const int SlugMax = 50;
    public const int ProvinceMax = 50;
    public const int DescriptionMax = 2000;
    public const int WelcomeMessageMax = 500;
    public const int FileNameMax = 255;
    public const int ProfileMax = 2000;
    public const int WebsiteMax = 500;
    public const int EmailMax = 200;
    public const int SeatsMin = 1;
    public const int SeatsMax = 100;
    public const long ImageMaxBytes = 5 * 1024 * 1024;
}
