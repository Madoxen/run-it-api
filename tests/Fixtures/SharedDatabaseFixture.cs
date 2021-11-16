using System;
using System.Data.Common;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Api.Test.Fixtures
{
    public class SharedDatabaseFixture : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _databaseInitialized;

        public SharedDatabaseFixture()
        {
            Connection = new Npgsql.NpgsqlConnection(@"Server=test_db;Database=dev;Username=admin;Password=admin");

            Seed();

            Connection.Open();
        }

        public DbConnection Connection { get; }

        public ApiContext CreateContext(DbTransaction transaction = null)
        {
            var context = new ApiContext(new DbContextOptionsBuilder<ApiContext>().UseNpgsql(Connection).Options);

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
        }

        private void Seed()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();

                        var user = new User()
                        {
                            Id = 1,
                        };

                        context.Users.Add(user);
                        context.SaveChanges();
                    }


                    _databaseInitialized = true;
                }
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}
