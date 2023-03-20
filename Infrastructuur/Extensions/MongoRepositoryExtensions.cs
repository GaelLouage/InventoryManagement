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

namespace Infrastructuur.Extensions
{
    public static class MongoRepositoryExtensions
    {
        // use where T class : to ensure that T is a reference type. 
        private static IServiceCollection MongoRepositorySetter<T>(this IServiceCollection services, string databaseName, string collectionName) where T : class
        {
            return services.AddScoped<IRepository<T>>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                return new MongoRepository<T>(configuration, collectionName, databaseName);
            });
        }
        public static IServiceCollection AddMongoRepository(this IServiceCollection services, string databaseName) 
        {
            return services
                   .MongoRepositorySetter<CategoryEntity>(databaseName, DbCollection.CATEGORY)
                   .MongoRepositorySetter<InventoryItemEntity>(databaseName, DbCollection.INVENTORY)
                   .MongoRepositorySetter<ProductEntity>(databaseName, DbCollection.PRODUCT)
                   .MongoRepositorySetter<SupplierEntity>(databaseName, DbCollection.SUPPLIER);
        }
    }
}
