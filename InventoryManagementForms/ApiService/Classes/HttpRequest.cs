using Infrastructuur.Entities;
using InventoryManagementForms.ApiService.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.ApiService.Classes
{
    public class HttpRequest<T> : IHttpRequest<T>
    {
        public readonly HttpClientHandler _httpClientHandler;
        private readonly string _baseUrl;

        public HttpRequest(string url)
        {
            _httpClientHandler = new HttpClientHandler();
            _httpClientHandler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true;
            _baseUrl = url;
        }

        public  async Task<List<T>> GetRequestListAsync(string endPoint)
        {
            using (_httpClientHandler)
            {
                // allow the bad certificate
               
                using (var httpClient = new HttpClient(_httpClientHandler))
                {
                    var response = await httpClient.GetAsync(_baseUrl + endPoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<T>>(content);
                    }
                }
            }
            return new List<T>();
        }
        public  async Task<T> GetRequestByIdAsync(int id, string endPoint)
        {
            using (_httpClientHandler)
            {
                using (var httpClient = new HttpClient(_httpClientHandler))
                {
                    var response = await httpClient.GetAsync("${_baseUrl}{endPoint}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(content);
                    }
                }
            }
            return default(T);
        }
    }
}
