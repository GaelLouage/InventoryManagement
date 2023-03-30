using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Mappers
{
    public static class CustomerMapper
    {
        public static CustomerEntity Map(CustomerDto supplierDto, ObjectId id, int customerId)
        {
            return new()
            {
                Id = id,
                CustomerId = customerId,
                Name = supplierDto.Name,
                Address = supplierDto.Address,
                Phone = supplierDto.Phone,
                Email = supplierDto.Email,
            };
        }
        // on create
        public static CustomerEntity Map(CustomerDto supplierDto, int supplierId)
        {
            return new()
            {
                CustomerId = supplierId,
                Name = supplierDto.Name,
                Address = supplierDto.Address,
                Phone = supplierDto.Phone,
                Email = supplierDto.Email,
            };
        }
    }
}
