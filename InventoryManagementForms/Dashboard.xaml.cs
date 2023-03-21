using Infrastructuur.Entities;
using InventoryManagementForms.ApiService.Classes;
using InventoryManagementForms.ApiService.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace InventoryManagementForms
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window 
    {
        private readonly IHttpRequest<ProductEntity> _httpRequestProduct;
        private readonly IHttpRequest<CategoryEntity> _httpRequestCategory;
        private readonly IHttpRequest<InventoryItemEntity> _httpRequestInventoryItem;
        private readonly IHttpRequest<SupplierEntity> _httpRequestSupplier;

        public Dashboard(IHttpRequest<ProductEntity> httpRequestProduct, IHttpRequest<CategoryEntity> httpRequestCategory, IHttpRequest<InventoryItemEntity> httpRequestInventoryItem, IHttpRequest<SupplierEntity> httpRequestSupplier)
        {
            _httpRequestProduct = httpRequestProduct;
            _httpRequestCategory = httpRequestCategory;
            _httpRequestInventoryItem = httpRequestInventoryItem;
            _httpRequestSupplier = httpRequestSupplier;
        }
        public Dashboard() : this(new HttpRequest<ProductEntity>("https://localhost:7071/api/"),
            new HttpRequest<CategoryEntity>("https://localhost:7071/api/"),
            new HttpRequest<InventoryItemEntity>("https://localhost:7071/api/"),
            new HttpRequest<SupplierEntity>("https://localhost:7071/api/"))
        {
            InitializeComponent();
        }
        /*In this code, we start all the HTTP requests in parallel using Task.WhenAll, 
         * and then wait for them to complete with await. Once all the tasks have completed, 
         * we use Dispatcher.
         * Invoke to switch back to the UI thread and set the ItemsSource properties of the data grids.*/
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var productsTask = _httpRequestProduct.GetRequestListAsync("Product/GetAllProducts");
            var categoriesTask = _httpRequestCategory.GetRequestListAsync("Category/GetAllCategories");
            var inventoryTask = _httpRequestInventoryItem.GetRequestListAsync("Inventory/GetAllInventoryItems");
            var supplierTask = _httpRequestSupplier.GetRequestListAsync("Supplier/GetAllSuppliers");

            await Task.WhenAll(productsTask, categoriesTask, inventoryTask, supplierTask);

            Dispatcher.Invoke(() =>
            {
                dGProducts.ItemsSource = productsTask.Result;
                dGCategories.ItemsSource = categoriesTask.Result;
                dGInventory.ItemsSource = inventoryTask.Result;
                dGSupplier.ItemsSource = supplierTask.Result;
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}
