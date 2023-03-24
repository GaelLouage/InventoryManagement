using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Extensions;
using Infrastructuur.Mappers;
using InventoryManagementForms.ApiService.Classes;
using InventoryManagementForms.ApiService.Interfaces;
using InventoryManagementForms.Enums;
using InventoryManagementForms.Helpers;
using InventoryManagementForms.Structs;
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
        private ValidInventoryId inventoryId = new ValidInventoryId();
        private ProductStruct productStruct = new ProductStruct();
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
         
            await UpdateData();
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
        // product forms
        private async void btnProductAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddProductValidation())
            {
                return;
            }
            var productDto = new ProductDto();
            productDto.Name = txtProductName.Text;
            productDto.Description = txtProductDescription.Text;
            productDto.Price = productStruct.price;
            productDto.Quantity = productStruct.quantity;
            await _httpRequestProduct.PostRequest(ProductMapper.Map(productDto, (await productsTask).Max(x => x.ProductId) + 1), Api.ADDPRODUCT);
            await UpdateData();
            ClearAddProductForm();
        }
        private bool CheckAddProductValidation()
        {
            if (string.IsNullOrEmpty(txtProductName.Text) || string.IsNullOrEmpty(txtProductDescription.Text) ||
               string.IsNullOrEmpty(txtProductPrice.Text) || string.IsNullOrEmpty(txtProductQuantity.Text))
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            txtProductPrice.Text = txtProductPrice.Text.Replace(",", ".");
            if (!decimal.TryParse(txtProductPrice.Text, out productStruct.price))
            {

                MessageBox.Show("Product price has to a number!");
                return false;
            }

            if (!int.TryParse(txtProductQuantity.Text, out productStruct.quantity))
            {
                MessageBox.Show("ProductQuantity has to a number!");
                return false;
            }
            return true;
        }


        // cateogory Forms
        private async void btnCategoryAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCategoryName.Text) && string.IsNullOrEmpty(txtCategoryDescription.Text))
            {
                MessageBox.Show("All input field are required!");
                return;
            }
            var category = new CategoryEntity();
            category.Name = txtCategoryName.Text;
            category.Description = txtCategoryDescription.Text;
            await _httpRequestCategory.PostRequest(category, Api.ADDCATEGORY);
            await UpdateData();
            ClearAddCategoryForm();
        }
        // supplier form
        private async void btnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddSupplierValidation())
            {
                return;
            }
            var supplier = new SupplierEntity();
            supplier.Name = txtSupplierName.Text;
            supplier.Address = txtSupplierAddress.Text;
            supplier.Phone = txtSupplierPhone.Text;
            supplier.Email = txtSupplierEmail.Text;
            await _httpRequestSupplier.PostRequest(supplier, Api.ADDSUPPLIER);
            await UpdateData();
            ClearAddSupplier();

        }
    
        // invenotry form
        private async void btnAddInventoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidInventory())
            {
                return;
            }
      
            var productIdTask = (await productsTask).SingleOrDefault(x => x.ProductId == inventoryId.productId);
            var supplierIdTask = (await supplierTask).SingleOrDefault(x => x.SupplierId == inventoryId.supplierId);
            var categoryIdTask = (await categoriesTask).SingleOrDefault(x => x.CategoryId == inventoryId.categoryId);
            if (!ValidIdOnProductSupplierCateogry(productIdTask, supplierIdTask, categoryIdTask)) return;
            var inventory = new InventoryItemEntity();
            inventory.ProductId = productIdTask.ProductId;
            inventory.CategoryId = categoryIdTask.CategoryId;
            inventory.SupplierId = supplierIdTask.SupplierId;
            inventory.Quantity = inventoryId.quantity;
          
            await _httpRequestInventoryItem.PostRequest(inventory, Api.ADDINVENTORYITEM);
            await UpdateData();
            ClearInventoryAddForm();

        }
        #region Validations
        private bool CheckAddSupplierValidation()
        {
            if (string.IsNullOrEmpty(txtSupplierName.Text) || string.IsNullOrEmpty(txtSupplierAddress.Text) ||
                string.IsNullOrEmpty(txtSupplierPhone.Text) || string.IsNullOrEmpty(txtSupplierEmail.Text))
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            if (!txtSupplierEmail.Text.IsValidEmail())
            {
                MessageBox.Show("Email is not valid!");
                return false;

            }
            if (!txtSupplierPhone.Text.IsValidPhoneNumber())
            {
                MessageBox.Show("Phone number is not valid!");
                return false;
            }
            return true;
        }
        private bool CheckValidInventory()
        {
         
            if (string.IsNullOrEmpty(txtInventoryAddProductID.Text) || string.IsNullOrEmpty(txtInventoryAddCateogryID.Text) ||
              string.IsNullOrEmpty(txtInventoryAddSupplierID.Text) || string.IsNullOrEmpty(txtInventoryAddQuantity.Text) || !dtInvetoryAddDateAdded.SelectedDate.HasValue)
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            if (!int.TryParse(txtInventoryAddProductID.Text, out inventoryId.productId))
            {

                MessageBox.Show("Product id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryAddCateogryID.Text, out inventoryId.categoryId))
            {

                MessageBox.Show("category id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryAddSupplierID.Text, out inventoryId.supplierId))
            {

                MessageBox.Show("supplier id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryAddQuantity.Text, out  inventoryId.quantity))
            {

                MessageBox.Show("quantity has to a number!");
                return false;
            }
            return true;
        }
        private bool ValidIdOnProductSupplierCateogry(ProductEntity productIdTask, SupplierEntity supplier, CategoryEntity category)
        {
            if (productIdTask is null)
            {
                MessageBox.Show($"No product With id {inventoryId.productId}");
                return false;
            }
            if (supplier is null)
            {
                MessageBox.Show($"No supplier With id {inventoryId.supplierId}");
                return false;
            }
            if (category is null)
            {
                MessageBox.Show($"No category With id {inventoryId.categoryId}");
                return false;
            }
            return true;
        }
        #endregion
        private async Task UpdateData()
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
        #region clearForms
        // clear forms methods
        private void ClearAddProductForm()
        {
            txtProductName.Text = string.Empty;
            txtProductPrice.Text = string.Empty;
            txtProductDescription.Text = string.Empty;
            txtProductQuantity.Text = string.Empty;
        }
        private void ClearAddCategoryForm()
        {
            txtCategoryName.Text = string.Empty;
            txtCategoryDescription.Text = string.Empty;
        }
        private void ClearAddSupplier()
        {
            txtSupplierName.Text = string.Empty;
            txtSupplierAddress.Text = string.Empty;
            txtSupplierPhone.Text = string.Empty;
            txtSupplierEmail.Text = string.Empty;
        }
        private void ClearInventoryAddForm()
        {
            txtInventoryAddCateogryID.Text = string.Empty;
            txtInventoryAddProductID.Text = string.Empty;
            txtInventoryAddSupplierID.Text = string.Empty;
            txtInventoryAddQuantity.Text = string.Empty;
            dtInvetoryAddDateAdded.Text = string.Empty;
        }
        #endregion

        private void lblAddProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridAddProductForm.Visibility = Visibility.Visible;
        }
    }
}
