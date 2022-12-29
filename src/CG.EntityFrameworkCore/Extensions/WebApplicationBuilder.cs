
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// This class contains extension methods related to the <see cref="WebApplication"/>
/// type.
/// </summary>
public static class WebApplicationExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method uses the information in the <see cref="DataAcessLayerOptions"/>
    /// options to call the <c>UseXXXDataAcess</c> method for the currently 
    /// configured provider, where the <c>XXX</c> part is dynamically replaced 
    /// with the value of the <see cref="DataAcessLayerOptions.Provider"/> 
    /// field. 
    /// </summary>
    /// <param name="webApplication">The web application to use for the 
    /// operation.</param>
    /// <returns>The value of the <paramref name="webApplication"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments are missing, or invalid.</exception>
    /// <exception cref="InvalidDataException">This exception is thrown 
    /// whenever a provider's configuration is incomplete, or contains
    /// invalid information.</exception>
    /// <remarks>
    /// <para>
    /// The method only functions while running in a development environment.
    /// For use in non-development environments, the method returns without 
    /// actually doing anything.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates using the method in a typical ASP.NET application:
    /// <code>
    ///     var builder = WebApplication.CreateBuilder(args);
    ///     
    ///     // Normal builder configuration code removed, for clarity
    /// 
    ///     var app = builder.Build();
    ///
    ///     // Normal app startup code removed, for clarity
    ///     
    ///     app.UseDataAccess();
    ///     
    ///     app.Run();
    /// </code>
    /// </example>
    public static WebApplication UseDataAccess(
        this WebApplication webApplication
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(webApplication, nameof(webApplication));

        // Log what we are about to do.
        webApplication.Logger.LogDebug(
            "Checking the environment for DAL startup."
            );

        // We only do stuff in a development environment.
        if (!webApplication.Environment.IsDevelopment())
        {
            // Return the application.
            return webApplication;
        }

        // Log what we are about to do.
        webApplication.Logger.LogDebug(
            "Fetching the DAL options from the DI container."
            );

        // Get the DAL options.
        var dalOptions = webApplication.Services.GetRequiredService<
            IOptions<DataAcessLayerOptions>
            >().Value;

        // Get the provider name from the options.
        var providerName = dalOptions.Provider.Trim();

        // Get the path to the provider's configuration section.
        var providerSectionPath = $"{dalOptions.SectionPath.Trim()}:{providerName}";

        // Sanity check the provider's section path.
        if (string.IsNullOrEmpty(providerSectionPath))
        {
            // Panic!!
            throw new InvalidDataException(
                $"The 'SectionPath' property on the DAL options is required but " +
                "wasn't provided!"
                );
        }

        // Tell the world what we are about to do.
        webApplication.Logger.LogDebug(
            "Getting the '{path}' section",
            providerSectionPath
            );
        
        // Get the configuration section for the provider.
        var providerSection = webApplication.Configuration.GetSection(
            providerSectionPath
            );

        // Tell the world what we are about to do.
        webApplication.Logger.LogDebug(
            "Getting the assembly name for the provider"
            );

        // Get the assembly name from the configuration.
        var assemblyName = providerSection["AssemblyName"];

        // Sanity check the assembly name.
        if (string.IsNullOrEmpty(assemblyName))
        {
            // Panic!!
            throw new InvalidDataException(
                $"The '{providerSectionPath}:AssemblyName' section is required but " +
                "wasn't found in the configuration!"
                );
        }

        // Build the extension method name.
        var methodName = $"Use{providerName}DataAccess";

        // Tell the world what we are about to do.
        webApplication.Logger.LogDebug(
            "Searching for an extension method named: '{name}' in " +
            "assembly: {asm}",
            methodName,
            assemblyName
            );

        // Load the assembly (if needed) and look for a matching extension
        //   method, by name.
        var methodInfo = AppDomain.CurrentDomain.ExtensionMethods(
            extensionType: typeof(WebApplication),
            extensionMethodName: methodName,
            assemblyWhiteList: assemblyName
            );

        // Did we find a match?
        if (methodInfo is not null && methodInfo.Any())
        {
            // Tell the world what we are about to do.
            webApplication.Logger.LogDebug(
                "Calling extension method named: '{name}' in assembly: {asm}",
                methodName,
                assemblyName
                );

            // Call the provider's extension method.
#pragma warning disable CS8601 // Possible null reference assignment.
            methodInfo.First().Invoke(
                null,
                new object[] { webApplication }
                );
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        else
        {
            // Tell the world what we didn't do.
            webApplication.Logger.LogError(
                "Failed to locate an extension method named: '{name}' in " +
                "assembly: {asm}! Common causes of this error: (1) The " +
                "assembly should be located where the .NET assembly loader " +
                "can find it, at runtime. (2) The extension method should be " +
                "public, with a name that matches our convention.",
                methodName,
                assemblyName
                );
        }

        // Return the application.
        return webApplication;
    }

    #endregion
}
