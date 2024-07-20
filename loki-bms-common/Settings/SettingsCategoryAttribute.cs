using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common.Settings;

/// <summary>
/// Attribute used to identify and manage application-level settings
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SettingsCategoryAttribute: Attribute
{
    /// <summary>
    /// The title of this settings class as seen in the Settings UI
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// A short (3-4 character) version of the <see cref="Title"/>, used if the UI can't display the entire title
    /// </summary>
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// A summary of the purpose and effects of settings in this class
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How this settings class is replicated on a LOKI network structure
    /// </summary>
    public NetworkBehavior NetworkBehavior { get; set; } = NetworkBehavior.None;

    public SettingsCategoryAttribute (string title, string description = "", string abbreviation = "", NetworkBehavior networkBehavior = NetworkBehavior.None)
    {
        Title = title;
        Abbreviation = abbreviation;
        Description = description;
        NetworkBehavior = networkBehavior;
    }
}

public enum NetworkBehavior
{
    None = 0,
    ServerSetsIfEmpty = 1,
    ServerIsAuthoritative = 2,
    ClientMayUpdate = 3,
}
