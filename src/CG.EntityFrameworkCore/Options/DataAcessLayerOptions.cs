
namespace CG.EntityFrameworkCore.Options;

/// <summary>
/// This class contains configuration settings for a data-access layer
/// (DAL).
/// </summary>
public class DataAcessLayerOptions
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the name of the currently selected provider.
    /// </summary>
    [Required]
    public string Provider { get; set; } = null!;

    /// <summary>
    /// This property is for internal use, only. It contains the path to 
    /// the provider's configuration section. 
    /// </summary>
    public string SectionPath { get; set; } = null!;

    /// <summary>
    /// This property directs the DAL to drop the underlying database on 
    /// startup.
    /// </summary>
    public bool DropDatabaseOnStartup { get; set; }

    /// <summary>
    /// This property directs the DAL to migrate the underlying database 
    /// on startup.
    /// </summary>
    public bool MigrateDatabaseOnStartup { get; set; }

    #endregion
}
