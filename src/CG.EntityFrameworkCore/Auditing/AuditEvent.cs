
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This class is an entity that represents an audit event.
/// </summary>
[Table("AuditEvents", Schema = "Audit")]
public class AuditEvent
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the primary key for the entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// This property contains the entity name for the event.
    /// </summary>
    public string EntityName { get; set; } = null!;

    /// <summary>
    /// This property contains the action type for the event.
    /// </summary>
    public string ActionType { get; set; } = null!;

    /// <summary>
    /// This property contains the user name for the event.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// This property contains the timestamp for the event.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// This property contains the entity identifier for the event.
    /// </summary>
    public string EntityId { get; set; } = null!;

    /// <summary>
    /// This property contains the user name for the event.
    /// </summary>
    public Dictionary<string, object?> Changes { get; set; } = new();

    #endregion
}
