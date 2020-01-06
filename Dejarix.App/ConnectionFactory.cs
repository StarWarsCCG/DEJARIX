using Dejarix.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dejarix.App
{
    public class ConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
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