
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This interface represents a repository of audit events.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// This method queries for audit events that match the given criteria.
    /// </summary>
    /// <param name="criteria">The LINQ criteria for the query.</param>
    /// <param name="cancellationToken">A cancellation token that is monitored
    /// throughout the lifetime of the operation.</param>
    /// <returns>A task to perform the operation that returns the results of
    /// the query.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments are missing, or invalid.</exception>
    Task<IQueryable<AuditEvent>> FindAllAsync(
        Expression<Func<AuditEvent, bool>>? criteria = null,
        CancellationToken cancellationToken = default
        );
}
