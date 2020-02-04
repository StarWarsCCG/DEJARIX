using Dejarix.App.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dejarix.App
{
    public class ConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PrimaryDatabase");
        }

        public void BuildOptions(DbContextOptionsBuilder builder)
        {
            builder.UseNpgsql(_connectionString);
            builder.EnableSensitiveDataLogging();
        }

        public DejarixDbContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<DejarixDbContext>();
            BuildOptions(builder);
            return new DejarixDbContext(builder.Options);
        }
    }
}