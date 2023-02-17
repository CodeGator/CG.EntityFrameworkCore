
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This class is an EFCORE data-context for recording audit events.
/// </summary>
internal class AuditDbContext : DbContext
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the set of audit events.
    /// </summary>
    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="AuditDbContext"/>
    /// class.
    /// </summary>
    /// <param name="options">The options to use with this data-context.</param>
    public AuditDbContext(
        DbContextOptions<AuditDbContext> options
        ) : base(options)
    {

    }

    #endregion

    // *******************************************************************
    // Protected methods.
    // *******************************************************************

    #region Protected methods

    /// <summary>
    /// This method is called to build the data model.
    /// </summary>
    /// <param name="modelBuilder">The model builder to use for the operation.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Convert the changes dictionary to/from JSON.
        modelBuilder.Entity<AuditEvent>()
            .Property(e => e.Changes)
            .HasConversion(
                value => JsonSerializer.Serialize(
                    value, 
                    new JsonSerializerOptions() 
                ),
                serializedValue => JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    serializedValue,
                    new JsonSerializerOptions()
                    ) ?? new Dictionary<string, object?>()
                );

        // Create an index for the table.
        modelBuilder.Entity<AuditEvent>().HasIndex(e =>
            new
            {
                e.Changes,
                e.UserName,
                e.EntityName,
                e.EntityId,
                e.ActionType,
                e.TimeStamp
            });

        // Give the base class a chance.
        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
