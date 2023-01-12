
namespace CG.EntityFrameworkCore.Repositories;

/// <summary>
/// This class is a base implementation of an Entity Framework repository.
/// </summary>
/// <typeparam name="TRepository">The type of associated concrete repository.</typeparam>
/// <typeparam name="TContext">The type of associated data-context.</typeparam>
public abstract class RepositoryBase<TRepository, TContext> 
    where TRepository : RepositoryBase<TRepository, TContext>
    where TContext : DbContext
{
    // *******************************************************************
    // Fields.
    // *******************************************************************

    #region Fields

    /// <summary>
    /// This field contains the data-context for this repository.
    /// </summary>
    internal protected readonly TContext _dbContext = null!;

    /// <summary>
    /// This field contains the logger for this repository.
    /// </summary>
    internal protected readonly ILogger<TRepository> _logger = null!;

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="RepositoryBase{TRepository, TContext}"/>
    /// class.
    /// </summary>
    /// <param name="dbContext">The EFCORE data-context to use with this 
    /// repository.</param>
    /// <param name="logger">The logger to use with this repository.</param>
    public RepositoryBase(
        TContext dbContext,
        ILogger<TRepository> logger
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(dbContext, nameof(dbContext))
            .ThrowIfNull(logger, nameof(logger));

        // Save the reference(s).
        _dbContext = dbContext;
        _logger = logger;
    }

    #endregion
}
