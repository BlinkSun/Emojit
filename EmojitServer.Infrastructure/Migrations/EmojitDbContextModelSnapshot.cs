using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EmojitServer.Infrastructure.Persistence;

#nullable disable

namespace EmojitServer.Infrastructure.Migrations
{
    /// <summary>
    /// Represents the Entity Framework Core model snapshot for the Emojit database context.
    /// </summary>
    [DbContext(typeof(EmojitDbContext))]
    internal sealed class EmojitDbContextModelSnapshot : ModelSnapshot
    {
        /// <inheritdoc />
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
#pragma warning restore 612, 618
        }
    }
}
