using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Mappers
{
    public class SupplierMapper
    {
        public static SupplierEntity Map(SupplierDto supplierDto, ObjectId id, int supplierId)
        {
            return new()
            {
                Id= id,
                SupplierId= supplierId,
                Name = supplierDto.Name,
                Address= supplierDto.Address,
                Phone= supplierDto.Phone,
                Email= supplierDto.Email,
            };
        }
        // on create
        public static SupplierEntity Map(SupplierDto supplierDto, int supplierId)
        {
            return new()
            {
                SupplierId = supplierId,
                Name = supplierDto.Name,
                Address = supplierDto.Address,
                Phone = supplierDto.Phone,
                Email = supplierDto.Email,
            };
        }
    }
}
