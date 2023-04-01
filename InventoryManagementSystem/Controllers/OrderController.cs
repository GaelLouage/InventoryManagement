using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Mappers;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<OrderEntity> _repository;
        private readonly IRepository<InventoryItemEntity> _repositoryInventoryItem;
        private readonly IRepository<ProductEntity> _repositoryProduct;
        private readonly IRepository<CustomerEntity> _repositoryCustomers;
        // caching categories
        private readonly IMemoryCache _memoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public OrderController(IRepository<OrderEntity> repository, IMemoryCache memoryCache, IRepository<InventoryItemEntity> repositoryInventoryItem, IRepository<ProductEntity> repositoryProduct, IRepository<CustomerEntity> repositoryCustomers)
        {
            _repository = repository;
            _memoryCache = memoryCache;
            _repositoryInventoryItem = repositoryInventoryItem;
            _repositoryProduct = repositoryProduct;
            _repositoryCustomers = repositoryCustomers;
        }
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            // keeping data in memory
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<OrderEntity> orderEntity))
            {
                return Ok(orderEntity);
            }
            var customers = await _repository.GetAllAsync();
            //set a timespan to keep the data into memory (10min)
            _memoryCache.Set(_resetCacheToken, customers, TimeSpan.FromMinutes(10));
            if (customers == null) return NotFound();
            return Ok(customers);
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            var products = (await _repositoryProduct.GetAllAsync());
            var inventoryItems = (await _repositoryInventoryItem.GetAllAsync());
            var orders = (await _repository.GetAllAsync());
            var customers = (await _repositoryCustomers.GetAllAsync());
            // custome exist
            if (!customers.Any(x => x.CustomerId == orderDto.CustomerId)) return NotFound($"Customer with id {orderDto} not found!");
            // list of inventory items where ordersItems.invId contains = true
            var existingInventoryItems = inventoryItems
            .Where(x => orderDto.Items
            .Select(x => x.InventoryItemId)
            .Contains(x.InventoryItemId))
                .ToList();
            if (inventoryItems is null) return NotFound("No inventoryitems found!");

            // items of orderDto

            var inventoryItemsByProductId = existingInventoryItems.ToDictionary(x => x.InventoryItemId);

            foreach (var item in orderDto.Items)
            {
                // lookup inventory item for this product
                if (!inventoryItemsByProductId.TryGetValue(item.InventoryItemId, out var inventoryItem))
                {
                    // error list instead of badrequest
                    return BadRequest($"Product not found in inventory: {item.InventoryItemId}");
                }
                if (inventoryItem.Product.Quantity < item.Quantity)
                {
                    // error list instead of badrequest
                    return BadRequest($"Not enough items in stock: {inventoryItem.Product.Name}");
                }
                inventoryItem.Product.Quantity -= item.Quantity;
                item.TotalAmount += inventoryItem.Product.Price * item.Quantity;

                // update product and inventory item in database
                await Task.WhenAll(
                    _repositoryProduct.UpdateAsync(x => x.ProductId == inventoryItem.ProductId, inventoryItem.Product),
                    _repositoryInventoryItem.UpdateAsync(x => x.InventoryItemId == inventoryItem.InventoryItemId, inventoryItem)
                );
                item.OrderItemId = orderDto.Items.Count();
                item.Price = inventoryItem.Product.Price;
            }
            orderDto.TotalAmount = orderDto.Items.Sum(x => x.TotalAmount);
            orderDto.OrderId = 1;
            if (orders.Count() > 0)
            {
                orderDto.OrderId = orders.Max(x => x.OrderId) + 1;
            }
            await _repository.AddAsync(OrderMapper.Map(orderDto, orderDto.OrderId));
            // removes the mem cache to reset it
            _memoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetOrderById), new { id = orderDto.CustomerId }, orderDto);
        }

        [HttpGet("GetOrderById/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {

            var orderS = await _repository.GetByIdAsync(x => x.OrderId == id);
            if (orderS == null) return NotFound();
            return Ok(orderS);
        }

        [HttpPatch("UpdateOrderById/{id}")]
        public async Task<IActionResult> UpdateOrderById(int id, OrderDto orderDto)
        {
            var existingOrder = await _repository.GetByIdAsync(x => x.OrderId == id);
            foreach (var order in existingOrder.Items)
            {
                var inv = (await _repositoryInventoryItem.GetByIdAsync(x => x.InventoryItemId == order.InventoryItemId));
                var product = await _repositoryProduct.GetByIdAsync(x => x.ProductId == inv.ProductId);
                if (inv == null && product == null) continue;
                inv.Product.Quantity += order.Quantity;
                product.Quantity += order.Quantity;

                await _repositoryInventoryItem.UpdateAsync(x => x.InventoryItemId == order.InventoryItemId, inv);
                await _repositoryProduct.UpdateAsync(x => x.ProductId == inv.ProductId, product);
     
            }
            foreach (var order in orderDto.Items)
            {
                var inv = (await _repositoryInventoryItem.GetByIdAsync(x => x.InventoryItemId == order.InventoryItemId));
                var product = await _repositoryProduct.GetByIdAsync(x => x.ProductId == inv.ProductId);
                if (inv == null || product == null) continue;
                inv.Product.Quantity -= order.Quantity;
                product.Quantity -= order.Quantity;

                await _repositoryInventoryItem.UpdateAsync(x => x.InventoryItemId == order.InventoryItemId, inv);
                await _repositoryProduct.UpdateAsync(x => x.ProductId == inv.ProductId, product);
            }
            if (existingOrder == null) return NotFound();
            existingOrder = OrderMapper.Map(orderDto,existingOrder.Id, id);

            await _repository.UpdateAsync(x => x.OrderId == id, existingOrder);
            _memoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
        [HttpDelete("DeleteOrderById/{id}")]
        public async Task<IActionResult> DeleteOrderById(int id)
        {
            var existingOrder = await _repository.GetByIdAsync(x => x.OrderId == id);
            foreach (var order in existingOrder.Items)
            {
                var inv = (await _repositoryInventoryItem.GetByIdAsync(x => x.InventoryItemId == order.InventoryItemId));
                var product = await _repositoryProduct.GetByIdAsync(x => x.ProductId == inv.ProductId);
                if (inv == null && product == null) continue;
                inv.Product.Quantity += order.Quantity;
                product.Quantity += order.Quantity;

                await _repositoryInventoryItem.UpdateAsync(x => x.InventoryItemId == order.InventoryItemId, inv);
                await _repositoryProduct.UpdateAsync(x => x.ProductId == inv.ProductId, product);

            }
            if (existingOrder is null) return NotFound();
            _memoryCache.Remove(_resetCacheToken);
            await _repository.DeleteAsync(x => x.OrderId == id);
            return NoContent();
        }
    }
}
