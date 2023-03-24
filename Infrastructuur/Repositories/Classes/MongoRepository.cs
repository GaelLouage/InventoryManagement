using Infrastructuur.Extensions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Repositories.Interfaces
{
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        public MongoRepository(IConfiguration configuration, string collectionName, string databaseName,string connectionString, string myMongoDBConnection)
        {
            var client = new MongoClient(MongoRepositoryExtensions.AddMongoConnectionString(configuration,connectionString,myMongoDBConnection));
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<T> AddAsync(T entity)
        {
           await _collection.InsertOneAsync(entity);
           return entity;
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var filter = Builders<T>.Filter.Where(predicate);
            await _collection.DeleteOneAsync(filter);
        }
        public async Task DeleteRangeAsync(Expression<Func<T, bool>> predicate)
        {
            var filter = Builders<T>.Filter.Where(predicate);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task<T> UpdateAsync(Expression<Func<T, bool>> predicate, T entity)
        {
            var filter = Builders<T>.Filter.Where(predicate);
            if (filter is null) return default(T);
            await _collection.ReplaceOneAsync(filter, entity);
            return entity;
        }
        // valueTask instead of task for better performance
        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate) =>
            await _collection.Find(predicate).FirstOrDefaultAsync();
        

        public async Task<IEnumerable<T>> GetAllAsync() =>
             await _collection.Find(Builders<T>.Filter.Empty).ToListAsync() ?? Enumerable.Empty<T>();
       
    }
}
