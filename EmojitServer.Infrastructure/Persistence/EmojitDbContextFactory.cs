using System;
using System.IO;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace EmojitServer.Infrastructure.Persistence;

/// <summary>
/// Provides a design-time factory for creating <see cref="EmojitDbContext"/> instances.
/// </summary>
public sealed class EmojitDbContextFactory : IDesignTimeDbContextFactory<EmojitDbContext>
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EmojitServer;Trusted_Connection=True;TrustServerCertificate=True;";

    /// <inheritdoc />
    public EmojitDbContext CreateDbContext(string[] args)
    {
        try
        {
            IConfigurationRoot configuration = BuildConfiguration();
            string connectionString = configuration.GetConnectionString("EmojitDatabase") ?? DefaultConnectionString;

            DbContextOptionsBuilder<EmojitDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer(connectionString);

            return new EmojitDbContext(optionsBuilder.Options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to create the Emojit database context for design-time services.", ex);
        }
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        string basePath = Directory.GetCurrentDirectory();

        ConfigurationBuilder builder = new();
        builder.SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine("..", "EmojitServer.Api", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("..", "EmojitServer.Api", "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
