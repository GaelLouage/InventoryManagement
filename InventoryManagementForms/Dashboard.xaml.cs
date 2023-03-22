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
        private Task<List<ProductEntity>> productsTask;
        private Task<List<CategoryEntity>> categoriesTask;
        private Task<List<SupplierEntity>> supplierTask;
        private Task<List<InventoryItemEntity>> inventoryTask;
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
             productsTask = _httpRequestProduct.GetRequestListAsync("Product/GetAllProducts");
             categoriesTask = _httpRequestCategory.GetRequestListAsync("Category/GetAllCategories");
             inventoryTask = _httpRequestInventoryItem.GetRequestListAsync("Inventory/GetAllInventoryItems");
             supplierTask = _httpRequestSupplier.GetRequestListAsync("Supplier/GetAllSuppliers");

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

        private async void txtProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGProducts.ItemsSource = (await productsTask)
                                     .Where(x => x.Name.ToLower().StartsWith(txtProductSearch.Text.ToLower()) || 
                                            x.Description.ToLower().StartsWith(txtProductSearch.Text.ToLower()));
        }

        private async void txtCategoriesSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGCategories.ItemsSource = (await categoriesTask)
                                        .Where(x => x.Name.ToLower().StartsWith(txtCategoriesSearch.Text.ToLower()) ||
                                            x.Description.ToLower().StartsWith(txtCategoriesSearch.Text.ToLower()));
        }

        private async void txtSupplierSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGSupplier.ItemsSource = (await supplierTask)
                                      .Where(x => x.Name.ToLower().StartsWith(txtSupplierSearch.Text.ToLower()) ||
                                          x.Address .ToLower().StartsWith(txtSupplierSearch.Text.ToLower()) ||
                                          x.Email.ToLower().StartsWith(txtSupplierSearch.Text.ToLower()));
        }

        private async void txtInventorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGInventory.ItemsSource = (await inventoryTask)
                                      .Where(x => x.Product.Name.ToLower().StartsWith(txtInventorySearch.Text.ToLower()) ||
                                             x.Category.Name.ToLower().StartsWith(txtInventorySearch.Text.ToLower()) ||
                                              x.Supplier.Name.ToLower().StartsWith(txtInventorySearch.Text.ToLower()));
        }
    }
}
