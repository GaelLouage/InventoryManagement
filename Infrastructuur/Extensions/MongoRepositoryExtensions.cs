using Infrastructuur.Constants;
using Infrastructuur.Entities;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Json;

namespace Infrastructuur.Extensions
{
    public static class MongoRepositoryExtensions
    {
        // use where T class : to ensure that T is a reference type. 
        private static IServiceCollection MongoRepositorySetter<T>(this IServiceCollection services, string databaseName, string collectionName, string connectionString, string myMongoDBConnection) where T : class
        {
            return services.AddScoped<IRepository<T>>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                return new MongoRepository<T>(configuration, collectionName, databaseName, connectionString,myMongoDBConnection);
            });
        }
        public static IServiceCollection AddMongoRepository(this IServiceCollection services, string databaseName, string connectionString, string myMongoDBConnection) 
        {
            return services
                   .MongoRepositorySetter<CategoryEntity>(databaseName, DbCollection.CATEGORY, connectionString, myMongoDBConnection)
                   .MongoRepositorySetter<InventoryItemEntity>(databaseName, DbCollection.INVENTORY, connectionString, myMongoDBConnection)
                   .MongoRepositorySetter<ProductEntity>(databaseName, DbCollection.PRODUCT, connectionString, myMongoDBConnection)
                   .MongoRepositorySetter<SupplierEntity>(databaseName, DbCollection.SUPPLIER, connectionString, myMongoDBConnection)
                    .MongoRepositorySetter<UserEntity>(databaseName, DbCollection.USER, connectionString, myMongoDBConnection); 
        }
        public static string AddMongoConnectionString(this IConfiguration configuration, string connectionString, string myMongoDBConnection)
        {
                 configuration = new ConfigurationBuilder()
                .AddJsonFile(connectionString)
                .Build();

            return  configuration.GetConnectionString(myMongoDBConnection);

        }
    }
}
