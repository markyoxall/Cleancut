namespace CleanCut.WinApp.Services.Management;

public class UserPreferences
{
    public List<string>? ColumnOrder { get; set; }
    public Dictionary<string, int>? ColumnWidths { get; set; }
    public Dictionary<string, string>? CustomSettings { get; set; }
}
