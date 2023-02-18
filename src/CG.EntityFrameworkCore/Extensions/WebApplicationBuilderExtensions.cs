
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// This class contains extension methods related to the <see cref="WebApplicationBuilder"/>
/// type.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method uses the information in the specified configuration 
    /// section to call the <c>AddXXXDataAcess</c> method, for the currently 
    /// configured provider, where the <c>XXX</c> part is dynamically replaced 
    /// with the value of the <c>DAL:Provider</c> key, in the configuration. 
    /// </summary>
    /// <param name="webApplicationBuilder">The web application builder to
    /// use for the operation.</param>
    /// <param name="sectionPath">The configuration section path to use for 
    /// the operation. Defaults to <c>DAL</c>.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to use 
    /// for the operation.</param>
    /// <returns>The value of the <paramref name="webApplicationBuilder"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments are missing, or invalid.</exception>
    /// <exception cref="InvalidDataException">This exception is thrown 
    /// whenever the assembly name is missing, or empty, for the provider.
    /// </exception>
    /// <remarks>
    /// <para>
    /// There is a convention that says the target extension method, in your
    /// assembly, should look like this:
    /// <code>
    /// public static WebApplicationBuilder AddMyProviderDataAccess(
    ///     this WebApplicationBuilder webApplicationBuilder,
    ///     string sectionName = "DAL:MyProvider",
    ///     ILogger? bootstrapLogger = null
    ///     );
    /// </code>
    /// Where the <c>AddMyProviderDataAccess</c> method name is derived from
    /// the corresponding provider (<c>MyProvider</c> in this example).
    /// </para>
    /// <para>
    /// The <c>SectionName</c> parameter can by anything you like, but it should
    /// be legal a JSON name, since it must match a corresponding section in the 
    /// application's configuration. 
    /// </para>
    /// <para>
    /// The <c>bootstrapLogger</c> parameter can be provided if you want to 
    /// log the internal actions of the method. That can be useful for troubleshooting,
    /// when the method fails to find, or call, your extension method.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates using the method in a typical ASP.NET application:
    /// <code>
    ///     var builder = WebApplication.CreateBuilder(args);
    ///     
    ///     builder.AddDataAccess();
    /// 
    ///     // Normal builder configuration code removed, for clarity
    ///     
    ///     var app = builder.Build();
    ///
    ///     // Normal app startup code removed, for clarity
    ///     
    ///     app.Run();
    /// </code>
    /// </example>
    public static WebApplicationBuilder AddDataAccess(
        this WebApplicationBuilder webApplicationBuilder,
        string sectionPath = "DAL",
        ILogger? bootstrapLogger = null
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(webApplicationBuilder, nameof(webApplicationBuilder))
            .ThrowIfNullOrEmpty(sectionPath, nameof(sectionPath));

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Grabbing the '{section}' section for the data access layer",
            sectionPath
            );

        // Get the section we need to target.
        var baseSection = webApplicationBuilder.Configuration.GetSection(
            sectionPath
            );

        // Tell the world what we are about to do.
        bootstrapLogger?.LogDebug(
            "Configuring DAL options from the '{section}' section, " +
            "for the data access layer",
            sectionPath
            );

        // Configure the DAL options.
        webApplicationBuilder.Services.ConfigureOptions<DataAcessLayerOptions>(
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
            extensionType: typeof(WebApplicationBuilder),
            extensionMethodName: methodName,
            assemblyWhiteList: assemblyName,
            parameterTypes: new[] { typeof(string), typeof(ILogger) }
            ); 

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
                    webApplicationBuilder,
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

        // Return the application builder.
        return webApplicationBuilder;
    }

    #endregion
}

