
namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// This class is a base implementation of the <see cref="IDesignTimeDbContextFactory{TContext}"/>
/// interface.
/// </summary>
/// <typeparam name="TContext">The type of associated data-context.</typeparam>
/// <remarks>
/// <para>
/// The class reads a local 'appSettings.json' file with the connection information 
/// required to create a <typeparamref name="TContext"/> instance, at runtime.
/// </para>
/// <para>
/// The <see cref="OnConfigureDataContextOptions(DbContextOptionsBuilder{TContext}, IConfiguration)"/>
/// method is called, internally, to configure the data-context options using
/// the settings that were read in the 'appSettings.json' file."/>
/// </para>
/// </remarks>
/// <example>
/// This examples demonstrates deriving from <see cref="DesignTimeDbContextFactory{TContext}"/>
/// to form a concrete factory class:
/// <code>
/// class MySqlServerFactory : DesignTimeDbContextFactory{MyDbContext}
/// {
///    protected override OnConfigureDataContextOptions(
///        DbContextOptionsBuilder{MyDbContext} optionsBuilder,
///        IConfiguration configuration
///        )
///     {
///         var connectionString = configuration["MyConnectionString"];
///         optionsBuilder.UseSqlServer(connectionString);
///     }
/// }
/// </code>
/// </example>
public abstract class DesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method is called to create a <typeparamref name="TContext"/>
    /// instance, at design-time.
    /// </summary>
    /// <param name="args">Optional arguments for the operation.</param>
    /// <returns>An instance of <typeparamref name="TContext"/>.</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown
    /// whenever the method fails to locate a public constructor on the 
    /// data-context class that accepts a <see cref="DbContextOptions{TContext}"/>
    /// instance.</exception>
    public virtual TContext CreateDbContext(string[] args)
    {
        // Create the configuration.
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appSettings.json");
        var configuration = configBuilder.Build();

        // Create the data-context options builder.
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        // Give derived classes a chance to configure the data-context
        // options.
        OnConfigureDataContextOptions(
            optionsBuilder,
            configuration
            );

        // Get the ctor information.
        var ci = typeof(TContext).GetConstructor(new[]
        {
            typeof(DbContextOptions<TContext>)
        });

        // Did we fail?
        if (ci is null)
        {
            // Panic!!
            throw new InvalidOperationException(
                $"Unable to locate a ctor on the '{typeof(TContext).Name}' " +
                $"type that accepts a single '{typeof(DbContextOptions<TContext>).Name}' " +
                "parameter."
                );
        }

        // Invoke the ctor.
        var dataContext = ci?.Invoke(
            new object[] { optionsBuilder.Options }
            ) as TContext;

        // Did we fail?
        if (dataContext is null)
        {
            // Panic!!
            throw new InvalidOperationException(
                $"Unable to create an instance of '{typeof(TContext).Name}'."
                );
        }

        // Return the results
#pragma warning disable CS8603 // Possible null reference return.
        return dataContext;
#pragma warning restore CS8603 // Possible null reference return.
    }

    #endregion

    // *******************************************************************
    // Protected methods.
    // *******************************************************************

    #region Protected methods

    /// <summary>
    /// This method is called by the factory to configure the given <see cref="DbContextOptionsBuilder{TContext}"/>
    /// object, from the specified <see cref="IConfiguration"/> object.
    /// </summary>
    /// <param name="optionsBuilder">The data-context options builder to
    /// use for the operation.</param>
    /// <param name="configuration">The configuration object to use for 
    /// the operation.</param>
    protected abstract void OnConfigureDataContextOptions(
        DbContextOptionsBuilder<TContext> optionsBuilder,
        IConfiguration configuration
        );

    #endregion
}
