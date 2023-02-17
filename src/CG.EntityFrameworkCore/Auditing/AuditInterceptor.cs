
namespace CG.EntityFrameworkCore.Auditing;

/// <summary>
/// This class is an EFCORE interceptor that records data changes, for 
/// auditing purposes.
/// </summary>
internal class AuditInterceptor : SaveChangesInterceptor
{
    // *******************************************************************
    // Fields.
    // *******************************************************************

    #region Fields

    /// <summary>
    /// This field contains the manual entity cache for this interceptor.
    /// </summary>
    internal protected static readonly Dictionary<string, AuditedEntityAttribute> _manualCache = new();

    /// <summary>
    /// This field contains the service provider for this interceptor.
    /// </summary>
    internal protected readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// This field contains the decoratedType cache for this interceptor.
    /// </summary>
    internal protected readonly Dictionary<Type, AuditedEntityAttribute> _typeCache = new();

    /// <summary>
    /// This field contains the logger for this interceptor.
    /// </summary>
    internal protected readonly ILogger<AuditInterceptor> _logger;

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="AuditInterceptor"/>
    /// class.
    /// </summary>
    public AuditInterceptor(
        IServiceProvider serviceProvider,
        ILogger<AuditInterceptor> logger
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(serviceProvider, nameof(serviceProvider))
            .ThrowIfNull(logger, nameof(logger));

        // Save the reference(s).
        _logger = logger;
        _serviceProvider = serviceProvider;

        // Construct our cache.
        BuildTypeCache();
    }

    #endregion
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <inheritdoc/>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(eventData, nameof(eventData));

        // Log what we are about to do.
        _logger.LogDebug(
            "Changes intercepted by the: {name} interceptor",
            nameof(AuditInterceptor)
            );

        // Record the event.
        await RecordAuditEvent(
            eventData,
            cancellationToken
            ).ConfigureAwait(false);

        // Give the base class a chance.
        return await base.SavingChangesAsync(
            eventData, 
            result, 
            cancellationToken
            ).ConfigureAwait(false);
    }

    // *******************************************************************

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(eventData, nameof(eventData));

        // Log what we are about to do.
        _logger.LogDebug(
            "Changes intercepted by the: {name} interceptor",
            nameof(AuditInterceptor)
            );

        // Record the event.
        RecordAuditEvent(
            eventData
            ).Wait();

        // Give the base class a chance.
        return base.SavingChanges(
            eventData, 
            result
            );
    }

    #endregion

    // *******************************************************************
    // Protected methods.
    // *******************************************************************

    #region Protected methods

    /// <summary>
    /// This method records an audit event.
    /// </summary>
    /// <param name="eventData">The event data to use for the operation.</param>
    /// <param name="cancellationToken">A cancellation token that is monitored
    /// throughout the lifetime of the method.</param>
    /// <returns>A task to perform the operation.</returns>
    protected virtual async Task RecordAuditEvent(
        DbContextEventData eventData,
        CancellationToken cancellationToken = default
        )
    {
        try
        {
            // We never audit the audit data-context!
            if (eventData.Context?.GetType() == typeof(AuditDbContext))
            {
                // Log what we are about to do.
                _logger.LogWarning(
                    "Somehow we tried to audit the audit data context!"
                    );

                return; // Do nothing.
            }

            // Log what we are about to do.
            _logger.LogDebug(
                "Creating a DI scope"
                );

            // Create a DI scope.
            using var scope = _serviceProvider.CreateScope();

            // Log what we are about to do.
            _logger.LogDebug(
                "Creating a HTTP context accessor"
                );

            // Get the HTTP context accessor.
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

            // Log what we are about to do.
            _logger.LogDebug(
                "Fetching the current user"
                );

            // Get the user name (if possible).
            var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";

            // Log what we are about to do.
            _logger.LogDebug(
                "Creating an audit data-context"
                );

            // Get the audit data-context.
            using var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            // Log what we are about to do.
            _logger.LogDebug(
                "Detecting changes in the data-context"
                );

            // Tell the change track we need the changes.
            eventData.Context?.ChangeTracker.DetectChanges();

            // Log what we are about to do.
            _logger.LogDebug(
                "Fetching the changes from the data-context"
                );

            // Get the changes.
            var changes = eventData.Context?.ChangeTracker.Entries()
                ?? Array.Empty<EntityEntry>();

            // Loop through the changes.
            foreach (var entityEntry in changes)
            {
                // Get the entity type.
                var entityType = entityEntry.Entity.GetType();

                // Log what we are about to do.
                _logger.LogDebug(
                    "Processing change for entity: {name}",
                    entityType.Name
                    );

                // Ignore any unchanged entities.
                if (entityEntry.State == EntityState.Unchanged)
                {
                    // Log what we aren't going to do.
                    _logger.LogDebug(
                        "The entity state was a {change} so we are ignoring it",
                        nameof(EntityState.Unchanged)
                        );

                    continue; // Nothing to do.
                }

                // Ignore any unchanged entities.
                if (entityEntry.State == EntityState.Detached)
                {
                    // Log what we aren't going to do.
                    _logger.LogDebug(
                        "The entity state was a {change} so we are ignoring it",
                        nameof(EntityState.Detached)
                        );

                    continue; // Nothing to do.
                }

                AuditedEntityAttribute? attr = null;

                // Is the entity NOT decorated?
                if (!_typeCache.ContainsKey(entityType))
                {
                    // Is the entity NOT manually specified?
                    if (!_manualCache.ContainsKey(entityType.Name) &&
                        !_manualCache.ContainsKey(entityType.FullName ?? ""))
                    {
                        // Log what we aren't going to do.
                        _logger.LogDebug(
                            "The entity is not decorated with a {attr} attribute, so we are ignoring it",
                            nameof(AuditedEntityAttribute)
                            );

                        continue; // Nothing to do.
                    }
                    else
                    {
                        // Look for the corresponding attribute.
                        if (_manualCache.ContainsKey(entityType.Name))
                        {
                            // Recover the attribute for the entity.
                            attr = _manualCache[entityType.Name];
                        }
                        else if (_manualCache.ContainsKey(entityType.FullName ?? ""))
                        {
                            // Recover the attribute for the entity.
                            attr = _manualCache[entityType.FullName ?? ""];
                        }
                    }
                }
                else
                {
                    // Recover the attribute for the entity.
                    attr = _typeCache[entityType];
                }

                // Did we fail to get an attribute?
                if (attr is null)
                {
                    // Log what happened.
                    _logger.LogWarning(
                        "Failed to find an {attr} attributee for entity: {name}",
                        nameof(AuditedEntityAttribute),
                        entityType.Name
                        );
                    continue;
                }

                // Should we ignore deletes?
                if (entityEntry.State == EntityState.Deleted && !attr.RecordDeletes)
                {
                    // Log what we aren't going to do.
                    _logger.LogDebug(
                        "The entity state is {state} but the entity isn't configured to audit for deletes",
                        Enum.GetName<EntityState>(entityEntry.State)
                        );

                    continue; // Nothing to do.
                }

                // Should we ignore updates?
                if (entityEntry.State == EntityState.Modified && !attr.RecordUpdates)
                {
                    // Log what we aren't going to do.
                    _logger.LogDebug(
                        "The entity state is {state} but the entity isn't configured to audit for updates",
                        Enum.GetName<EntityState>(entityEntry.State)
                        );

                    continue; // Nothing to do.
                }

                // Should we ignore creates?
                if (entityEntry.State == EntityState.Added && !attr.RecordCreates)
                {
                    // Log what we aren't going to do.
                    _logger.LogDebug(
                        "The entity state is {state} but the entity isn't configured to audit for creates",
                        Enum.GetName<EntityState>(entityEntry.State)
                        );

                    continue; // Nothing to do.
                }

                // Build a model for the event.
                var auditEvent = new AuditEvent
                {
                    UserName = userName,
                    TimeStamp = DateTime.UtcNow,

                    ActionType = entityEntry.State == EntityState.Added 
                        ? "INSERT" 
                        : entityEntry.State == EntityState.Deleted ? "DELETE" : "UPDATE",

                    EntityId = $"{entityEntry.Properties.SingleOrDefault(p => 
                        p.Metadata.IsPrimaryKey())?.CurrentValue}",

                    EntityName = entityEntry.Metadata.ClrType.Name,
                    
                    Changes = entityEntry.Properties.Select(p =>
                        new { p.Metadata.Name, p.CurrentValue }
                        ).ToDictionary(i => i.Name, i => i.CurrentValue),
                };

                // Add the audit event.
                auditDbContext.AuditEvents.Add(
                    auditEvent
                    );

                // Save the changes.
                await auditDbContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // Log what happened.
            _logger.LogError(
                ex,
                "Failed to record an audit event!"
                );
        }
    }

    #endregion

    // *******************************************************************
    // Private methods.
    // *******************************************************************

    #region Private methods

    /// <summary>
    /// This method uses reflection to look for types that have been decorated
    /// with our <see cref="AuditedEntityAttribute"/> attribute. It then builds
    /// a table of those types, with their associated <see cref="AuditedEntityAttribute"/>
    /// instance.
    /// </summary>
    private void BuildTypeCache()
    {
        // We cache this information here because the association between entity
        //   types and their attribute(s) is, by nature, hard coded and not going
        //   to change until the next compile operation. So, it makes sense to 
        //   build this information once then use it over and over.

        // Log what we are about to do.
        _logger.LogDebug(
            "Checking for assemblies with types that are decorated with the: {name} attribute",
            nameof(AuditedEntityAttribute)
            );

        // Look for any assemblies with decorated entity types.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x =>
            x.GetTypes().Any(y => y.CustomAttributes.Any(y => y.AttributeType == typeof(AuditedEntityAttribute)))
            ).ToList();

        // Did we find any?
        if (assemblies.Any())
        {
            // Log what we are about to do.
            _logger.LogDebug(
                "Checking {count} assemblies for entity types decorated with the: {name} attribute",
                assemblies.Count,
                nameof(AuditedEntityAttribute)
                );

            // Loop through the assemblies
            foreach (var assembly in assemblies)
            {
                // Log what we are about to do.
                _logger.LogDebug(
                    "Checking assembly: {asm} for types that are decorated with the {name} attribute",
                    assembly.FullName,
                    nameof(AuditedEntityAttribute)
                    );

                // Look for any types decorated with the attribute.
                var decoratedTypes = assembly.ExportedTypes.Where(x =>
                    x.CustomAttributes.Any(y => y.AttributeType == typeof(AuditedEntityAttribute))
                    ).ToList();

                // Log what we are about to do.
                _logger.LogDebug(
                    "Caching {count} types from assembly: {asm} that are decorated with the: {name} attribute",
                    decoratedTypes.Count,
                    assembly.FullName,
                    nameof(AuditedEntityAttribute)
                    );

                // Loop through the decorated entity types.
                foreach (var decoratedType in decoratedTypes)
                {
                    // Log what we are about to do.
                    _logger.LogDebug(
                        "Caching type: {name} with associated: {name} attribute",
                        decoratedType.FullName,
                        nameof(AuditedEntityAttribute)
                        );

                    // Look for the associated custom attribute.
                    var customAttr = decoratedType.CustomAttributes.FirstOrDefault(x =>
                        x.AttributeType == typeof(AuditedEntityAttribute)
                        );

                    // Did we fail?
                    if (customAttr is null)
                    {
                        continue; // No attribute found!
                    }

                    // Look for the named argument.
                    var recordDeletes = customAttr.NamedArguments.FirstOrDefault(x =>
                            x.MemberName == nameof(AuditedEntityAttribute.RecordDeletes)
                            );

                    // Look for the named argument.
                    var recordUpdates = customAttr.NamedArguments.FirstOrDefault(x =>
                            x.MemberName == nameof(AuditedEntityAttribute.RecordUpdates)
                            );

                    // Recover the original attribute.
                    var attr = new AuditedEntityAttribute()
                    {
                        RecordDeletes = (bool)(recordDeletes.TypedValue.Value ?? false),
                        RecordUpdates = (bool)(recordUpdates.TypedValue.Value ?? false)
                    };

                    // Cache the decoratedType with its attribute.
                    _typeCache[decoratedType] = attr;
                }
            }
        }
        else
        {
            // Log what we are about to do.
            _logger.LogDebug(
                "No assemblies found with entity types decorated with the: {name} attribute",
                nameof(AuditedEntityAttribute)
                );
        }
    }

    #endregion
}
