using Infrastructuur.Entities;
using InventoryManagementForms.ApiService.Classes;
using InventoryManagementForms.ApiService.Interfaces;
using InventoryManagementForms.Enums;
using InventoryManagementForms.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
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
using System.Windows.Markup;
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
        public Dashboard() : this(new HttpRequest<ProductEntity>(Api.BASEURL),
                                 new HttpRequest<CategoryEntity>(Api.BASEURL),
                             new HttpRequest<InventoryItemEntity>(Api.BASEURL),
                                 new HttpRequest<SupplierEntity>(Api.BASEURL))
        {
            InitializeComponent();
        }
        /*In this code, we start all the HTTP requests in parallel using Task.WhenAll, 
         * and then wait for them to complete with await. Once all the tasks have completed, 
         * we use Dispatcher.
         * Invoke to switch back to the UI thread and set the ItemsSource properties of the data grids.*/
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
             productsTask = _httpRequestProduct.GetRequestListAsync(Api.PRODUCT);
             categoriesTask = _httpRequestCategory.GetRequestListAsync(Api.CATEGORY);
             inventoryTask = _httpRequestInventoryItem.GetRequestListAsync(Api.INVENTORY);
             supplierTask = _httpRequestSupplier.GetRequestListAsync(Api.SUPPLIER);

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
        // sort documents
        private async void txtProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGProducts.ItemsSource = (await productsTask)
                                     .Where(x => x.Name.StartsWith(txtProductSearch.Text, StringComparison.OrdinalIgnoreCase) || 
                                            x.Description.StartsWith(txtProductSearch.Text, StringComparison.OrdinalIgnoreCase));
        }

        private async void txtCategoriesSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGCategories.ItemsSource = (await categoriesTask).Where(
                                          x => x.Name.StartsWith(txtCategoriesSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                          x.Description.StartsWith(txtCategoriesSearch.Text, StringComparison.OrdinalIgnoreCase));
        }
      
        private async void txtSupplierSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var suppliers = await supplierTask;

            var filteredSuppliers = suppliers
                .Where(x => x.Name.StartsWith(txtSupplierSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                            x.Address.StartsWith(txtSupplierSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                            x.Email.StartsWith(txtSupplierSearch.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            dGSupplier.ItemsSource = filteredSuppliers;
        }

        private async void txtInventorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGInventory.ItemsSource = (await inventoryTask)
                                      .Where(x => x.Product.Name.StartsWith(txtInventorySearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                             x.Category.Name.StartsWith(txtInventorySearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                              x.Supplier.Name.StartsWith(txtInventorySearch.Text, StringComparison.OrdinalIgnoreCase));
        }
        //print documents
        private void btnPrintProductGrid_Click(object sender, RoutedEventArgs e)
        {
            Printer.PrintData(dGProducts, Data.Product);
        }

        private void btnPrintCategoryGrid_Click(object sender, RoutedEventArgs e)
        {
            Printer.PrintData(dGCategories, Data.Category);
        }

        private void btnPrintSupplierGrid_Click(object sender, RoutedEventArgs e)
        {
            Printer.PrintData(dGSupplier, Data.Supplier);
        }

        private void btnPrintInventoryGrid_Click(object sender, RoutedEventArgs e)
        {
            Printer.PrintData(dGInventory, Data.Inventory);
        }
    }

}
