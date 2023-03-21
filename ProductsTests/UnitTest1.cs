using Infrastructuur.Entities;
using MongoDB.Driver;
using Moq;

namespace ProductsTests
{
    public class UnitTest1
    {
        private readonly Mock<IMongoCollection<ProductEntity>> _collectionMock;
        private readonly MongoRepository<TestEntity> _repository;

        public UnitTest1()
        {
            _collectionMock = new Mock<IMongoCollection<TestEntity>>();
            _repository = new MongoRepository<TestEntity>(
                configuration: null,
                collectionName: "testCollection",
                databaseName: "testDatabase",
                connectionString: "testConnectionString",
                myMongoDBConnection: "testMongoDBConnection"
            );
            _repository.GetType()
                .GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_repository, _collectionMock.Object);
        }
    }
}