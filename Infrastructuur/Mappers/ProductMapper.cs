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
    public static class ProductMapper
    {
        public static ProductEntity Map(ProductDto productDto, ObjectId id, int productId)
        {
            return new()
            {
                Id = id,
                ProductId= productId,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Quantity = productDto.Quantity,
            };
        }
        // method overloading for : map for creating product
        public static ProductEntity Map(ProductDto productDto, int productId)
        {
            return new()
            {
                ProductId = productId,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Quantity = productDto.Quantity,
            };
        }
    }
}