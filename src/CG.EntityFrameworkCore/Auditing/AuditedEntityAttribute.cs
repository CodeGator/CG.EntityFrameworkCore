
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This class decorates an entity type that should be audited.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)] 
public class AuditedEntityAttribute : Attribute
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property indicates whether or not creates should be audited.
    /// <c>true</c> to audit creates, <c>false</c> otherwise.
    /// </summary>
    public bool RecordCreates { get; set; } = true;

    /// <summary>
    /// This property indicates whether or not updates should be audited.
    /// <c>true</c> to audit updates, <c>false</c> otherwise.
    /// </summary>
    public bool RecordUpdates { get; set; } = true;

    /// <summary>
    /// This property indicates whether or not deletes should be audited.
    /// <c>true</c> to audit deletes, <c>false</c> otherwise.
    /// </summary>
    public bool RecordDeletes { get; set; } = true;

    #endregion
}
