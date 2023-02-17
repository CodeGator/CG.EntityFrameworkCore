
// This project quickly demonstrates the audit features of the CG.EntityFrameworkCore
//   NUGET package, in a typical hosted (albeit non web based) environment.

var builder = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        // Adds general auditing support.
        services.AddAuditing(
            (context, options) =>
            {
                options.UseInMemoryDatabase("demo-audit");
            },
            manualEntities: () => new[] { "CustomerEntity" },
            bootstrapLogger: BootstrapLogger.Instance()
            );

        services.AddDbContext<DemoDbContext>((context, options) =>
        {
            options.UseInMemoryDatabase("demo")
                .UseAuditing( // <-- Adds auditing to the specific data-context.
                    serviceProvider: context,
                    bootstrapLogger: BootstrapLogger.Instance()
                    );
        });        
    });

await builder.Build().RunDelegateAsync(async (host, token) =>
{
    var dbc = host.Services.GetRequiredService<DemoDbContext>();
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Hit 'Q' to exit");
        Console.WriteLine("Hit 'C' to clear the console");
        Console.WriteLine("Hit 'A' to dump audit records");
        Console.WriteLine("Hit anything else to add another customer");

        var key = Console.ReadKey();
        Console.WriteLine();

        if (key.KeyChar == 'Q' || key.KeyChar == 'q')
        {
            break;
        }

        if (key.KeyChar == 'C' || key.KeyChar == 'c')
        {
            Console.Clear();
            continue;
        }

        else if (key.KeyChar == 'A' || key.KeyChar == 'a')
        {
            var repo = host.Services.GetRequiredService<IAuditRepository>();
            var query = await repo.FindAllAsync();

            Console.WriteLine($"dumping {query.Count()} audit records");
            foreach (var e in query)
            {
                Console.WriteLine($"{e.TimeStamp}, {e.Id}, {e.UserName}, " +
                    $"{e.EntityName}, {e.ActionType}" +
                    string.Join("-", e.Changes.Select(x => $"{x.Key}^{x.Value}"))
                    );
            }
            continue;
        }

        var entityEntry = dbc.Customers.Add(
            new CustomerEntity()
            {
                CustomerNumber = $"{Guid.NewGuid():N}".ToUpper()
            });

        await dbc.SaveChangesAsync();
        
        Console.WriteLine($"new customer added: {entityEntry.Entity.Id}");
    }
});