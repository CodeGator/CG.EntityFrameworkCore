
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This class is a default implementation of the <see cref="IAuditRepository"/>
/// interface.
/// </summary>
internal class AuditRepository : IAuditRepository
{
    // *******************************************************************
    // Fields.
    // *******************************************************************

    #region Fields

    /// <summary>
    /// This field contains the logger for this repository.
    /// </summary>
    internal protected readonly AuditDbContext _auditDbContext = null!;

    /// <summary>
    /// This field contains the logger for this repository.
    /// </summary>
    internal protected readonly ILogger<IAuditRepository> _logger = null!;

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="AuditRepository"/>
    /// class.
    /// </summary>
    /// <param name="auditDbContext">The audit data-context to use with 
    /// this repository.</param>
    /// <param name="logger">The logger to use with this repository.</param>
    public AuditRepository(
        AuditDbContext auditDbContext,
        ILogger<IAuditRepository> logger
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(auditDbContext, nameof(auditDbContext))
            .ThrowIfNull(logger, nameof(logger));

        // Save the reference(s).
        _auditDbContext = auditDbContext;
        _logger = logger;
    }

    #endregion

    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <inheritdoc/>
    public virtual Task<IQueryable<AuditEvent>> FindAllAsync(
        Expression<Func<AuditEvent, bool>>? criteria = null,
        CancellationToken cancellationToken = default
        )
    {
        try
        {
            // Should we supply a default criteria?
            if (criteria is null)
            {
                criteria = x => true;
            }

            // Defer to the data-context for the query.
            var query = _auditDbContext.AuditEvents.Where(
                criteria
                );

            // Return the results.
            return Task.FromResult(query);
        }
        catch (Exception ex)
        {
            // Log what happened.
            _logger.LogError(
                ex,
                "Failed to query for audit events!"
                );

            // Provide better context.
            throw new RepositoryException(
                innerException: ex,
                message: "Failed to query for audit events!"
                );
        }
    }

    #endregion
}
