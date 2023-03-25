using Infrastructuur.Entities;
using System;
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
        Task<T> GetRequestByCredentials(LoginRequestEnity login, string endPoint);
        Task<T> PostRequest(T item, string endPoint);
        Task<T> UpdateRequest(T item, string endPoint, int id);
        Task<T> DeleteRequest(string endPoint, int id);
    }
}
