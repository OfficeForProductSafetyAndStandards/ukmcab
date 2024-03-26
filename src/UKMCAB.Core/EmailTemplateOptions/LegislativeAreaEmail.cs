namespace UKMCAB.Core.EmailTemplateOptions;

public class LegislativeAreaEmail
{
    public string dluhc { get; set; } = null!;
    public string mhra { get; set; } = null!;
    public string mcga { get; set; } = null!;
    public string dftr { get; set; } = null!;
    public string dftp { get; set; } = null!;
    public string opss_ogd { get; set; } = null!;
}

/// <summary>
/// Returns dictionary of OGD roleIds and email addresses. Converts "opss_ogd" role ID to "opss (ogd)".
/// </summary>
public static class LegislativeAreaEmailExtensions
{
    public static Dictionary<string, string> ToDictionary(this LegislativeAreaEmail emailAddresses)
    {
        return new Dictionary<string, string>
        {
            { "dluhc", emailAddresses.dluhc },
            { "mhra", emailAddresses.mhra },
            { "mcga", emailAddresses.mcga },
            { "dftr", emailAddresses.dftr },
            { "dftp", emailAddresses.dftp },
            { "opss (ogd)", emailAddresses.opss_ogd },
        };
    }
}