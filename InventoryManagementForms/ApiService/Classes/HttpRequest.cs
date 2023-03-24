using DnsClient;
using Infrastructuur.Entities;
using InventoryManagementForms.ApiService.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.ApiService.Classes
{
    public class HttpRequest<T> : IHttpRequest<T>
    {
        private readonly string _baseUrl;

        public HttpRequest(string url)
        {
            _baseUrl = url;
        }

        public  async Task<List<T>> GetRequestListAsync(string endPoint)
        {
            using (var _httpClientHandler = new HttpClientHandler())
            {
                _httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
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
            using (var _httpClientHandler = new HttpClientHandler())
            {
                _httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var httpClient = new HttpClient(_httpClientHandler))
                {
                    var response = await httpClient.GetAsync($"{_baseUrl}{endPoint}/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(content);
                    }
                }
            }
            return default(T);
        }
        public async Task<T> GetRequestByCredentials(LoginRequestEnity login, string endPoint)
        {
            using (var _httpClientHandler = new HttpClientHandler())
            {
                _httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var httpClient = new HttpClient(_httpClientHandler))
                {
                    var json = JsonConvert.SerializeObject(login);

                    // Create a StringContent object with the JSON data
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{_baseUrl}{endPoint}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<T>(responseJson);
                    }
                }
            }
            return default(T);
        }

        public async Task<T> PostRequest(T item,string endPoint)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                // ssl error fixer
                httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var json = JsonConvert.SerializeObject(item);

                    // Create a StringContent object with the JSON data
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{_baseUrl}{endPoint}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<T>(responseJson);
                    }
                }
            }
            return default(T);
        }
    }
}
