﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.ApiService.Interfaces
{
    public interface IHttpRequest<T>
    {
        Task<List<T>> GetRequestListAsync(string endPoint);
        Task<T> GetRequestByIdAsync(int id, string endPoint);
    }
}
