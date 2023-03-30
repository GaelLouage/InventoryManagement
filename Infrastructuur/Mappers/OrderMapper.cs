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
    public static class OrderMapper
    {
        public static OrderEntity Map(OrderDto orderDto, ObjectId id, int orderId)
        {
            return new()
            {
                Id = id,
                OrderId = orderId,
                CustomerId = orderDto.CustomerId,
                OrderDate = orderDto.OrderDate,
                Status = orderDto.Status,
                TotalAmount = orderDto.TotalAmount,
                Items = orderDto.Items
            };
        }
        // on create
        public static OrderEntity Map(OrderDto orderDto, int orderId)
        {
            return new()
            {
                OrderId = orderId,
                CustomerId = orderDto.CustomerId,
                OrderDate = orderDto.OrderDate,
                Status = orderDto.Status,
                TotalAmount = orderDto.TotalAmount,
                Items =orderDto.Items
            };
        }
    }
}