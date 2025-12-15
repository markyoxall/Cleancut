using CleanCut.Domain.Common;

namespace CleanCut.Domain.Entities;

public class Country : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    private Country() { }

    public Country(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public void UpdateDetails(string name, string code)
    {
        Name = name;
        Code = code;
    }
}
