using Infrastructuur.Entities;
using Infrastructuur.Repositories.Interfaces;
using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }
        [Fact]
        public async Task GetAllProducts_ReturnsOkResult_WithProductsInCache()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { ProductId = 1, Name = "Product 1", Price = 10 },
                new ProductEntity { ProductId = 2, Name = "Product 2", Price = 20 },
                new ProductEntity { ProductId = 3, Name = "Product 3", Price = 30 }
            };
            var mockInventoryRepository = new Mock<IRepository<InventoryItemEntity>>();
            var mockProductRepository = new Mock<IRepository<ProductEntity>>();
            var mockMemoryCache = new Mock<IMemoryCache>();
            var cancellationTokenSource = new CancellationTokenSource();
            mockMemoryCache
                .Setup(c => c.TryGetValue(cancellationTokenSource, out It.IsAny<List<ProductEntity>>()))
                .Returns(true)
                .Callback((CancellationTokenSource cts, out List<ProductEntity> cache) =>
                {
                    cache = products;
                });
            var controller = new ProductController(
                mockInventoryRepository.Object,
                mockProductRepository.Object,
                mockMemoryCache.Object
            );

            // Act
            var result = await controller.GetAllProducts();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(products, okResult.Value);
        }
    }
}