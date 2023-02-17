
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// This class contains extension methods related to the <see cref="DbContextOptionsBuilder"/>
/// type.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method adds an auditing interceptor to the given data-context
    /// options.
    /// </summary>
    /// <param name="contextOptionsBuilder">The data-context options builder
    /// to use for the operation.</param>
    /// <param name="serviceProvider">The service provider to use for the
    /// operation.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to use 
    /// for the operation.</param>
    /// <returns>The value of the <paramref name="contextOptionsBuilder"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    public static DbContextOptionsBuilder UseAuditing(
        this DbContextOptionsBuilder contextOptionsBuilder,
        IServiceProvider serviceProvider,
        ILogger? bootstrapLogger = null
        ) 
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(contextOptionsBuilder, nameof(contextOptionsBuilder))
            .ThrowIfNull(serviceProvider, nameof(serviceProvider));

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Adding interceptors, for DAL auditing"
            );

        // Add the interceptor(s).
        contextOptionsBuilder.AddInterceptors(
            serviceProvider.GetRequiredService<AuditInterceptor>()
            );

        // Return the builder.
        return contextOptionsBuilder;
    }

    #endregion
}
