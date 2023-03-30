using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Extensions;
using Infrastructuur.Mappers;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<CustomerEntity> _repository;
        // caching categories
        private readonly IMemoryCache _memoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public CustomerController(IRepository<CustomerEntity> repository, IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }
        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            // keeping data in memory
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<CustomerEntity> cat))
            {
                return Ok(cat);
            }
            var customers = await _repository.GetAllAsync();
            //set a timespan to keep the data into memory (10min)
            _memoryCache.Set(_resetCacheToken, customers, TimeSpan.FromMinutes(10));
            if (customers == null) return NotFound();
            return Ok(customers);
        }

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customer)
        {
            var custExist = (await _repository.GetAllAsync()).Any(x => x.Email == customer.Email);
            
            if(custExist)  return BadRequest($"User with {customer.Email} already exist!");
            if (!customer.Email.IsValidEmail()) return BadRequest("Invalid email");
            if (!customer.Phone.IsValidPhoneNumber()) return BadRequest("Ivalid phone number!");
            var customers  = (await _repository.GetAllAsync()).ToList();
            customer.CustomerId = 1;
            if (customers.Count > 0)
            {
             
                customer.CustomerId = customers.Max(x => x.CustomerId) + 1;
            }
            await _repository.AddAsync(CustomerMapper.Map(customer, customer.CustomerId));
            // removes the mem cache to reset it
            _memoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetCutomerById), new { id = customer.CustomerId }, customer);
        }

        [HttpGet("GetCutomerById/{id}")]
        public async Task<IActionResult> GetCutomerById(int id)
        {
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<CustomerEntity> cat))
            {
                var memCustomer = cat.FirstOrDefault(x => x.CustomerId == id);
                if (memCustomer == null) return NotFound();
                return Ok(memCustomer);
            }
            var category = await _repository.GetByIdAsync(x => x.CustomerId == id);
            _memoryCache.Set(_resetCacheToken, category, TimeSpan.FromMinutes(10));
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPut("UpdateCustomerById/{id}")]
        public async Task<IActionResult> UpdateCustomerById(int id, CustomerDto customer)
        {
            var existingCustomer = await _repository.GetByIdAsync(x => x.CustomerId == id);
            if (existingCustomer == null) return NotFound();
            if (!customer.Email.IsValidEmail()) return BadRequest("Invalid email");
            if (!customer.Phone.IsValidPhoneNumber()) return BadRequest("Ivalid phone number!");
            existingCustomer = CustomerMapper.Map(customer, existingCustomer.Id, customer.CustomerId);

            await _repository.UpdateAsync(x => x.CustomerId == id, existingCustomer);
            _memoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
        [HttpDelete("DeleteCustomerById/{id}")]
        public async Task<IActionResult> DeleteCustomerById(int id)
        {
            var existingCustomer = await _repository.GetByIdAsync(x => x.CustomerId == id);
            if (existingCustomer is null) return NotFound();
            _memoryCache.Remove(_resetCacheToken);
            await _repository.DeleteAsync(x => x.CustomerId == id);
            return NoContent();
        }
    }
}

