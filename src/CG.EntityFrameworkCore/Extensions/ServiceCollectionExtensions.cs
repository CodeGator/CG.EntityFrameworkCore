
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// This class contains extension methods related to the <see cref="IServiceCollection"/>
/// type.
/// </summary>
public static class ServiceCollectionExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method adds services required to support entity framework core
    /// auditing.
    /// </summary>
    /// <param name="serviceCollection">The service collection to use for
    /// the operation.</param>
    /// <param name="configuration">The configuration to use for the operation.</param>
    /// <param name="sectionPath">The configuration section path to use for 
    /// the operation. Defaults to <c>DAL</c>.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to use 
    /// for the operation.</param>
    /// <returns>The value of the <paramref name="serviceCollection"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments are missing, or invalid.</exception>
    /// <exception cref="InvalidDataException">This exception is thrown 
    /// whenever the assembly name is missing, or empty, for the provider.
    /// </exception>
    public static IServiceCollection AddDataAccess(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        string sectionPath = "DAL",
        ILogger? bootstrapLogger = null
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection))
            .ThrowIfNull(configuration, nameof(configuration))
            .ThrowIfNullOrEmpty(sectionPath, nameof(sectionPath));

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Grabbing the '{section}' section for the data access layer",
            sectionPath
            );

        // Get the section we need to target.
        var baseSection = configuration.GetSection(
            sectionPath
            );

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Configuring DAL options from the '{section}' section, " +
            "for the data access layer",
            sectionPath
            );

        // Configure the DAL options.
        serviceCollection.ConfigureOptions<DataAcessLayerOptions>(
            baseSection,
            out var dalOptions
            );

        // Remember the configuration section's path.
        dalOptions.SectionPath = sectionPath;

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Getting the provider name from the '{section}' section, " +
            "for the data access layer",
            sectionPath
            );

        // Get the provider name from the options.
        var providerName = dalOptions.Provider.Trim();

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Getting the '{section}:{prov}' section, for the data " +
            "access layer",
            sectionPath,
            providerName
            );

        // Get the configuration section for the provider.
        var providerSection = baseSection.GetSection(
            providerName
            );

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Getting the assembly name for the provider, for the " +
            "data access layer"
            );

        // Get the assembly name from the configuration.
        var assemblyName = providerSection["AssemblyName"];

        // Sanity check the assembly name.
        if (string.IsNullOrEmpty(assemblyName))
        {
            // Panic!!
            throw new InvalidDataException(
                $"The '{sectionPath}:AssemblyName' section is required but " +
                "wasn't found in the configuration!"
                );
        }

        // Build the extension method name.
        var methodName = $"Add{providerName}DataAccess";

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Searching for an extension method named: '{name}' in " +
            "assembly: {asm}, for the data access layer",
            methodName,
            assemblyName
            );

        // Load the assembly (if needed) and look for a matching extension
        //   method, by name, with the parameters that match our convention.
        var methodInfo = AppDomain.CurrentDomain.ExtensionMethods(
            extensionType: typeof(IServiceCollection),
            extensionMethodName: methodName,
            assemblyWhiteList: assemblyName,
            parameterTypes: new[] 
            { 
                typeof(string), typeof(ILogger)
            });

        // Did we find a match?
        if (methodInfo is not null && methodInfo.Any())
        {
            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Calling extension method named: '{name}' in assembly: " +
                "{asm}, for the data access layer",
                methodName,
                assemblyName
                );

            // Call the provider's extension method.
#pragma warning disable CS8601 // Possible null reference assignment.
            methodInfo.First().Invoke(
                null,
                new object[]
                {
                    serviceCollection,
                    providerSection.Path,
                    bootstrapLogger
                });
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        else
        {
            // Tell the world what we didn't do.
            bootstrapLogger?.LogError(
                "Failed to locate an extension method named: '{name}' in " +
                "assembly: {asm}! Common causes of this error: (1) The " +
                "assembly should be located where the .NET assembly loader " +
                "can find it, at runtime. (2) The extension method should be " +
                "public, with parameters and a name that match our convention.",
                methodName,
                assemblyName
                );
        }

        // Return the service collection.
        return serviceCollection;
    }

    // *******************************************************************

    /// <summary>
    /// This method adds auditing support for the data-access library.
    /// </summary>
    /// <param name="serviceCollection">The service collection to use for 
    /// the operation.</param>
    /// <param name="optionsDelegate">The delegate for specifying options 
    /// for the audit data-context.</param>
    /// <param name="manualEntities">The delegate for specifying manual 
    /// entity types that needs auditing services but can't be decorated 
    /// with a <see cref="AuditedEntityAttribute"/> attribute.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to use 
    /// for the operation.</param>
    /// <returns>The value of the <paramref name="serviceCollection"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments are missing, or invalid.</exception>
    public static IServiceCollection AddAuditing(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsDelegate,
        Func<IEnumerable<string>>? manualEntities = null,
        ILogger? bootstrapLogger = null
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection))
            .ThrowIfNull(optionsDelegate, nameof(optionsDelegate));

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Registering interceptor(s) for DAL auditing"
            );

        // Register our interceptor.
        serviceCollection.AddSingleton<AuditInterceptor>();

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Registering the audit data-context, for DAL auditing"
            );

        // Register our audit data-context.
        serviceCollection.AddDbContext<AuditDbContext>((context, options) =>
        {
            // Give the caller the chance to modify the options.
            optionsDelegate.Invoke(context, options);
        });

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Registering the service(s) we'll need, for DAL auditing"
            );

        // Register the service(s) we'll need.
        serviceCollection.AddHttpContextAccessor();

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Registering the repsitory(s) we'll need, for DAL auditing"
            );

        // Register the repository(s) we'll need.
        serviceCollection.AddScoped<IAuditRepository, AuditRepository>();

        // Do we need to deal with manual entities?
        if (manualEntities is not null)
        {
            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Fetching any manual entity types, for DAL auditing"
                );

            // Get any manual entity types.
            var entityTypes = manualEntities?.Invoke() 
                ?? Array.Empty<string>();

            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Registering  {count} manual entity types, for DAL auditing",
                entityTypes.Count()
                );

            // Loop through the manual entity types.
            foreach (var entityType in entityTypes.Distinct()) 
            {
                // Add a default attribute for this type.
                AuditInterceptor._manualCache[entityType]
                    = new AuditedEntityAttribute();
            }            
        }

        // Return the service collection.
        return serviceCollection;
    }

    #endregion
}
