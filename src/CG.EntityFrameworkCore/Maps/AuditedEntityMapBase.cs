
namespace CG.EntityFrameworkCore.Maps;

/// <summary>
/// This class represents a base map for entity types derived from <see cref="AuditedEntityBase"/>
/// </summary>
/// <typeparam name="TEntity">The associated concrete <see cref="AuditedEntityBase"/></typeparam>
internal abstract class AuditedEntityMapBase<TEntity> : 
    EntityMapBase<TEntity>, 
    IEntityTypeConfiguration<TEntity>
    where TEntity : AuditedEntityBase
{
    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="AuditedEntityMapBase{TEntity}"/>
    /// class.
    /// </summary>
    /// <param name="modelBuilder">The model builder to use with this map.</param>
    public AuditedEntityMapBase(
        ModelBuilder modelBuilder
        ) : base(modelBuilder)
    {

    }

    #endregion

    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method configures the <typeparamref name="TEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to use for the operation.</param>
    public override void Configure(
        EntityTypeBuilder<TEntity> builder
        )
    {
        // Setup the property.
        builder.Property(e => e.CreatedOnUtc)
            .HasDefaultValue(DateTime.UtcNow)
            .IsRequired();

        // Setup the property.
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(32)
            .IsUnicode(false)
            .IsRequired();

        // Setup the property.
        builder.Property(e => e.LastUpdatedBy)
            .HasMaxLength(32)
            .IsUnicode(false);

        // Setup the property.
        builder.Property(e => e.LastUpdatedOnUtc);

        // Setup the index.
        builder.HasIndex(e => new
        {
            e.CreatedOnUtc,
            e.CreatedBy,
            e.LastUpdatedBy,
            e.LastUpdatedOnUtc
        },
        $"IX_{typeof(TEntity).Name}_Stats"
        );
    }

    #endregion
}
