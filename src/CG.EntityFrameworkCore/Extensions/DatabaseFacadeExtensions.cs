
namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// This class contains extension methods related to the <see cref="DatabaseFacade"/>
/// type.
/// </summary>
public static partial class DatabaseFacadeExtensions
{
    // *******************************************************************
    // Fields.
    // *******************************************************************

    #region Fields

    /// <summary>
    /// This field contains a shared dictionary of parsed database names.
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _databaseNames = 
        new ConcurrentDictionary<string, string>();

    /// <summary>
    /// This field contains a shared dictionary of parsed server names.
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _serverNames =
        new ConcurrentDictionary<string, string>();

    #endregion

    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method returns the database name using the connection string
    /// from the given <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseFacade">The database facade to use for the
    /// operation.</param>
    /// <returns>The value the database name used in the connection string,
    /// or an empty string if the database wasn't specified in the connection
    /// string.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments is missing, or invalid.</exception>
    public static string GetDatabaseName(
        this DatabaseFacade databaseFacade
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(databaseFacade, nameof(databaseFacade));

        // Get the connection string.
        var connectionString = databaseFacade.GetConnectionString();

        // Did we fail?
        if (connectionString == null)
        {
            return ""; // Nothing left to do.
        }

        // Look for the parsed database name in the local cache.
        if (_databaseNames.TryGetValue(connectionString, out var databaseName))
        {
            return databaseName;
        }
        
        // Parse the connection string.
        var parser = new DbConnectionStringBuilder()
        {
            ConnectionString = connectionString
        };

        // Is the database name in the connection string?
        if (parser.ContainsKey("database"))
        {
            // Get the database name.
            databaseName = $"{parser["database"]}";

            // Update the cache.
            _databaseNames.TryAdd(connectionString, databaseName);  

            // Return the database name.
            return databaseName;
        }

        // Can't find the database name.
        return "";
    }

    // *******************************************************************

    /// <summary>
    /// This method returns the server name using the connection string
    /// from the given <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseFacade">The database facade to use for the
    /// operation.</param>
    /// <returns>The value the server used in the connection string, or 
    /// an empty string if the server wasn't specified in the connection
    /// string.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments is missing, or invalid.</exception>
    public static string GetServerName(
        this DatabaseFacade databaseFacade
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(databaseFacade, nameof(databaseFacade));

        // Get the connection string.
        var connectionString = databaseFacade.GetConnectionString();

        // Did we fail?
        if (connectionString == null)
        {
            return ""; // Nothing left to do.
        }

        // Look for the parsed server name in the local cache.
        if (_serverNames.TryGetValue(connectionString, out var serverName))
        {
            return serverName;
        }

        // Parse the connection string.
        var parser = new DbConnectionStringBuilder()
        {
            ConnectionString = connectionString
        };

        // Is the server name in the connection string?
        if (parser.ContainsKey("server"))
        {
            // Get the database name.
            serverName = $"{parser["server"]}";

            // Update the cache.
            _serverNames.TryAdd(connectionString, serverName);

            // Return the server name.
            return serverName;
        }

        // Can't find the server name.
        return "";
    }

    #endregion
}
