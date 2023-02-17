
namespace CG.EntityFrameworkCore.QuickStart;

/// <summary>
/// This class is a data-context for the demo application.
/// </summary>
internal class DemoDbContext : DbContext
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the set of customers.
    /// </summary>
    public virtual DbSet<CustomerEntity> Customers { get; set; }

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="DemoDbContext"/>
    /// class.
    /// </summary>
    /// <param name="options">The options to use with this data-context.</param>
    public DemoDbContext(
        DbContextOptions<DemoDbContext> options
        ) : base( options ) 
    {

    }

    #endregion
}
