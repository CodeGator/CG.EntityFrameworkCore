
namespace CG.EntityFrameworkCore.QuickStart;

/// <summary>
/// This class is an entity that represents a customer.
/// </summary>
[Table("Customers")]
//[AuditedEntity] // <-- Adds auditing for this entity type.
public class CustomerEntity
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property is the primary for the entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// This property contains the customer number for the entity.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string CustomerNumber { get; set; } = null!;
    
    #endregion
}
