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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection.Metadata;
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
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using OfficeOpenXml;
using InventoryManagementForms.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Reflection;
using Infrastructuur.Constants;

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
        private readonly IHttpRequest<UserEntity> _httpRequestUser;
        private Task<List<ProductEntity>> productsTask;
        private Task<List<CategoryEntity>> categoriesTask;
        private Task<List<SupplierEntity>> supplierTask;
        private Task<List<InventoryItemEntity>> inventoryTask;
        private Task<List<UserEntity>> userTask;
        private ValidInventoryId inventoryId = new ValidInventoryId();
        private ProductStruct productStruct = new ProductStruct();
        private UserEntity _user;
        private string[] _roles = new string[2] { Role.SUPERADMIN, Role.ADMIN };
        public Dashboard(IHttpRequest<ProductEntity> httpRequestProduct, IHttpRequest<CategoryEntity> httpRequestCategory, IHttpRequest<InventoryItemEntity> httpRequestInventoryItem, IHttpRequest<SupplierEntity> httpRequestSupplier, IHttpRequest<UserEntity> httpRequestUser)
        {
            _httpRequestProduct = httpRequestProduct;
            _httpRequestCategory = httpRequestCategory;
            _httpRequestInventoryItem = httpRequestInventoryItem;
            _httpRequestSupplier = httpRequestSupplier;
            _httpRequestUser = httpRequestUser;
        }
        public Dashboard(UserEntity user) : this(new HttpRequest<ProductEntity>(Api.BASEURL),
                                 new HttpRequest<CategoryEntity>(Api.BASEURL),
                             new HttpRequest<InventoryItemEntity>(Api.BASEURL),
                                 new HttpRequest<SupplierEntity>(Api.BASEURL),
                                 new HttpRequest<UserEntity>(Api.BASEURL))
        {
            InitializeComponent();
            _user = user;

        }
        /*In this code, we start all the HTTP requests in parallel using Task.WhenAll, 
         * and then wait for them to complete with await. Once all the tasks have completed, 
         * we use Dispatcher.
         * Invoke to switch back to the UI thread and set the ItemsSource properties of the data grids.*/
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateData();
            // populate combobox with enum types
            cmbProduct.ItemsSource = Enum.GetValues(typeof(ProductOrder)).Cast<ProductOrder>();
            cmbCategory.ItemsSource = Enum.GetValues(typeof(InventoryManagementForms.Enums.Category)).Cast<InventoryManagementForms.Enums.Category>();
            cmbSupplier.ItemsSource = Enum.GetValues(typeof(Supplier)).Cast<Supplier>();
            cmbInventory.ItemsSource = Enum.GetValues(typeof(Inventory)).Cast<Inventory>();
            cmbUser.ItemsSource = Enum.GetValues(typeof(UserS)).Cast<UserS>();
            cmbUserRole.Items.Add(Role.SUPERADMIN);
            cmbUserRole.Items.Add(Role.ADMIN);
            cmbUpdateUserRole.Items.Add(Role.SUPERADMIN);
            cmbUpdateUserRole.Items.Add(Role.ADMIN);
            // hide forms
            //TODO: place in method
            dGridUpdateProductForm.Visibility = Visibility.Hidden;
            dPUpdateCategory.Visibility = Visibility.Hidden;
            dPUpdateSupplier.Visibility = Visibility.Hidden;
            dPUpdateInventory.Visibility = Visibility.Hidden;
            dGridUpdateUserForm.Visibility = Visibility.Hidden;


            // dashboard
            lblTotalProducts.Content = (await productsTask).Count();
            lblTotalInventory.Content  = (await inventoryTask).Count();
            lblTotalCategories.Content = (await categoriesTask).Count();
            lblTotalSupplier.Content = (await supplierTask).Count();
           
            // hide the user tab for non superadmins
            if (_user.Role is not Role.SUPERADMIN)
            {
                tbItemUsers.Visibility = Visibility.Hidden;
            }
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
        private async void txtUserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGUser.ItemsSource = (await userTask)
               .Where(x => x.UserName.StartsWith(txtUserSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                         x.Name.StartsWith(txtUserSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                                          x.Email.StartsWith(txtUserSearch.Text, StringComparison.OrdinalIgnoreCase));
        }

        #region ComboboxOrderData
        // combobox to order items
        //using reflection allows for more flexibility and ease of maintenance in terms of adding or removing properties to sort by
        private async void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productsTask is null)
            {
                return;
            }
            // order items 
            var items = await productsTask;
            var sortOption = (ProductOrder)cmbProduct.SelectedItem;
            var sortExpression = OrderDictionary.SortingOptionsProduct[sortOption];
            dGProducts.ItemsSource = items.OrderBy(sortExpression);
        }
        private async void cmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (supplierTask is null)
            {
                return;
            }

            var items = await supplierTask;
            var sortOption = (Supplier)cmbSupplier.SelectedItem;
            var sortExpression = OrderDictionary.SortingOptionsSupplier[sortOption];
            dGSupplier.ItemsSource = items.OrderBy(sortExpression);
        }

        private async void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoriesTask is null)
            {
                return;
            }

            var items = await categoriesTask;
            var sortOption = (InventoryManagementForms.Enums.Category)cmbCategory.SelectedItem;
            var sortExpression = OrderDictionary.SortingOptionsCategory[sortOption];
            dGCategories.ItemsSource = items.OrderBy(sortExpression);
        }
        private async void cmbInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inventoryTask is null)
            {
                return;
            }

            var items = await inventoryTask;
            var sortOption = (Inventory)cmbInventory.SelectedItem;
            var sortExpression = OrderDictionary.SortingOptionsInventory[sortOption];
            dGInventory.ItemsSource = items.OrderBy(sortExpression);
        }
        private async void cmbUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userTask is null) return;
            var items = await userTask;
            var sortOption = (UserS)cmbUser.SelectedItem;
            var sortExpression = OrderDictionary.SortingOptionsUser[sortOption];
            dGUser.ItemsSource = items.OrderBy(sortExpression);
        }
        #endregion
        #region Navigation
        // navigations product
        private void lblAddProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridAddProductForm.Visibility = Visibility.Visible;
            dGridUpdateProductForm.Visibility = Visibility.Hidden;
        }
        private void lblUpdateProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridAddProductForm.Visibility = Visibility.Hidden;
            dGridUpdateProductForm.Visibility = Visibility.Visible;
        }
        // navigation category
        private void lblAddCategory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddCategory.Visibility = Visibility.Visible;
            dPUpdateCategory.Visibility = Visibility.Hidden;
        }

        private void lblUpdateCategory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddCategory.Visibility = Visibility.Hidden;
            dPUpdateCategory.Visibility = Visibility.Visible;
        }
        // navigation supplier
        private void lblAddSupplier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddSupplier.Visibility = Visibility.Visible;
            dPUpdateSupplier.Visibility = Visibility.Hidden;
        }

        private void lblUpdateSupplier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddSupplier.Visibility = Visibility.Hidden;
            dPUpdateSupplier.Visibility = Visibility.Visible;
        }
        private void lblAddInventory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddInvetory.Visibility = Visibility.Visible;
            dPUpdateInventory.Visibility = Visibility.Hidden;
        }

        private void lblUpdateInventory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddInvetory.Visibility = Visibility.Hidden;
            dPUpdateInventory.Visibility = Visibility.Visible;
        }
        private void lblAddUser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridUserForm.Visibility = Visibility.Visible;
            dGridUpdateUserForm.Visibility = Visibility.Hidden;
        }

        private void lblUpdateUser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridUserForm.Visibility = Visibility.Hidden;
            dGridUpdateUserForm.Visibility = Visibility.Visible;
        }
        #endregion
        #region PDFDocs
        // pdf documents
        private void btnPdfProductGrid_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "Product";
            string suffix = ".xlsx";
            dGProducts.WriteToExcelFile(fileName, new ProductEntity());
            if (File.Exists($"{fileName}{suffix}"))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }
        private void btnPdfCategoryGrid_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "Category";
            string suffix = ".xlsx";
            dGCategories.WriteToExcelFile(fileName, new CategoryEntity());
            if (File.Exists($"{fileName}{suffix}"))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }

        private void btnPdfSupplierGrid_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "Supplier";
            string suffix = ".xlsx";
            dGSupplier.WriteToExcelFile(fileName, new SupplierEntity());
            if (File.Exists($"{fileName}{suffix}"))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }

        private void btnPdfInventoryGrid_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "Inventory";
            string suffix = ".xlsx";
            dGInventory.WriteToExcelFile<InventoryItemEntity>(fileName, new InventoryItemEntity(), _httpRequestProduct, _httpRequestCategory, _httpRequestSupplier);
            if (File.Exists($"{fileName}{suffix}"))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }
        private void btnPdfUserGrid_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "User";
            string suffix = ".xlsx";
            dGUser.WriteToExcelFile<UserEntity>(fileName, new UserEntity());
            if (File.Exists($"{fileName}{suffix}"))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }
        #endregion
        #region PrintButtons
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
        private void btnPrintUserGrid_Click(object sender, RoutedEventArgs e)
        {
            Printer.PrintData(dGUser, Data.User);
        }
        #endregion
        // product forms
        private async void btnProductAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddProductValidation(txtProductName.Text, txtProductDescription.Text, txtProductPrice.Text, txtProductQuantity.Text))
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
        private async void btnProductUpdateItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddProductValidation(txtUpdateProductName.Text, txtUpdateProductPrice.Text, txtUpdateProductDescription.Text, txtUpdateProductQuantity.Text))
            {
                return;
            }
            if (dGProducts.SelectedValue is null) return;
            //TODO: map this
            var selectedProduct = dGProducts.SelectedValue as ProductEntity;

            var productDto = new ProductDto();
            productDto.Name = txtUpdateProductName.Text;
            productDto.Description = txtUpdateProductDescription.Text;
            productDto.Price = decimal.Parse(txtUpdateProductPrice.Text);
            productDto.Quantity = int.Parse(txtUpdateProductQuantity.Text);
            var b = dGProducts.SelectedIndex;
            await _httpRequestProduct.UpdateRequest(ProductMapper.Map(productDto, (await productsTask).Max(x => x.ProductId) + 1), Api.UPDATEPRODUCT, selectedProduct.ProductId);
            await UpdateData();
            ClearAddProductForm();
        }

        private void dGProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dGProducts.SelectedValue is null) return;
            var selectedProduct = dGProducts.SelectedValue as ProductEntity;
            txtUpdateProductName.Text = selectedProduct.Name;
            txtUpdateProductDescription.Text = selectedProduct.Description;
            txtUpdateProductPrice.Text = selectedProduct.Price.ToString("F2");
            txtUpdateProductQuantity.Text = selectedProduct.Quantity.ToString();
        }
        private bool CheckAddProductValidation(string productName, string ProductPrice, string productDescription, string productQuantity)
        {
            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productDescription) ||
               string.IsNullOrEmpty(ProductPrice) || string.IsNullOrEmpty(productQuantity))
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            ProductPrice = ProductPrice.Replace(",", ".");
            if (!decimal.TryParse(ProductPrice, out productStruct.price))
            {

                MessageBox.Show("Product price has to a number!");
                return false;
            }

            if (!int.TryParse(productQuantity, out productStruct.quantity))
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
        private async void btnUpdateCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUpdateCategoryName.Text) && string.IsNullOrEmpty(txtUpdateCategoryDescription.Text))
            {
                MessageBox.Show("All input field are required!");
                return;
            }

            var selectedCategory = dGCategories.SelectedItem as CategoryEntity;
            if (selectedCategory is null) return;
            selectedCategory.Name = txtUpdateCategoryName.Text;
            selectedCategory.Description = txtUpdateCategoryDescription.Text;
            await _httpRequestCategory.UpdateRequest(selectedCategory, Api.UPDATECATEGORY, selectedCategory.CategoryId);
            await UpdateData();
            ClearAddCategoryForm();
        }
        private void dGCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dGCategories.SelectedItem is null) return;
            var category = dGCategories.SelectedItem as CategoryEntity;
            txtUpdateCategoryName.Text = category.Name;
            txtUpdateCategoryDescription.Text = category.Description;
        }
        // supplier form
        private async void btnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddSupplierValidation(txtSupplierName.Text, txtSupplierAddress.Text, txtSupplierPhone.Text, txtSupplierEmail.Text))
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
        private async void btnUpdateSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddSupplierValidation(txtUpdateSupplierName.Text, txtUpdateSupplierAddress.Text, txtUpdateSupplierPhone.Text, txtUpdateSupplierEmail.Text))
            {
                return;
            }
            var supplier = dGSupplier.SelectedItem as SupplierEntity;
            if (supplier is null) return;
            // TODO map this
            supplier.Name = txtUpdateSupplierName.Text;
            supplier.Address = txtUpdateSupplierAddress.Text;
            supplier.Phone = txtUpdateSupplierPhone.Text;
            supplier.Email = txtUpdateSupplierEmail.Text;
            await _httpRequestSupplier.UpdateRequest(supplier, Api.UPDATESUPPLIER, supplier.SupplierId);
            await UpdateData();
            ClearAddSupplier();
        }
        private void dGSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var supplier = dGSupplier.SelectedItem as SupplierEntity;
            if (supplier is null) return;
            txtUpdateSupplierName.Text = supplier.Name;
            txtUpdateSupplierAddress.Text = supplier.Address;
            txtUpdateSupplierEmail.Text = supplier.Email;
            txtUpdateSupplierPhone.Text = supplier.Phone;
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
        private async void btnUpdateInventoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidInventoryOnUpdate())
            {
                return;
            }
            var selectedInventory = (InventoryItemEntity)dGInventory.SelectedItem;
            if (selectedInventory is null)
            {
                return;
            }
            var productIdTask = (await productsTask).SingleOrDefault(x => x.ProductId == int.Parse(txtInventoryUpdateProductID.Text));
            var supplierIdTask = (await supplierTask).SingleOrDefault(x => x.SupplierId == int.Parse(txtInventoryUpdateSupplierID.Text));
            var categoryIdTask = (await categoriesTask).SingleOrDefault(x => x.CategoryId == int.Parse(txtInventoryUpdateCateogryID.Text));
            var inventoryIdTaks = (await inventoryTask).SingleOrDefault(x => x.InventoryItemId == selectedInventory.InventoryItemId);
            if (!ValidIdOnProductSupplierCateogry(productIdTask, supplierIdTask, categoryIdTask)) return;
            //TODO: map this
            var inventory = new InventoryItemEntity();
            inventory.Id = selectedInventory.Id;
            inventory.ProductId = productIdTask.ProductId;
            inventory.CategoryId = categoryIdTask.CategoryId;
            inventory.SupplierId = supplierIdTask.SupplierId;
            inventory.Quantity = inventoryId.quantity;
            inventory.Product = productIdTask;
            inventory.LastUpdated = DateTime.Now;
            inventory.DateAdded = inventoryIdTaks.DateAdded;
            inventory.Category = categoryIdTask;
            inventory.Supplier = supplierIdTask;
            await _httpRequestInventoryItem.UpdateRequest(inventory, Api.UPDATEINVENTORYITEM, selectedInventory.InventoryItemId);
            await UpdateData();
            ClearInventoryAddForm();
        }
        private void dGInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedInventoryItem = (InventoryItemEntity)dGInventory.SelectedItem;
            if (selectedInventoryItem is null) return;
            txtInventoryUpdateCateogryID.Text = selectedInventoryItem.CategoryId.ToString();
            txtInventoryUpdateProductID.Text = selectedInventoryItem.ProductId.ToString();
            txtInventoryUpdateQuantity.Text = selectedInventoryItem.Quantity.ToString();
            txtInventoryUpdateSupplierID.Text = selectedInventoryItem.SupplierId.ToString();
        }
        // user form 
        private async void btnUpdateUserAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValideUser(txtUpdateAddNameUser.Text, txtUpdateAddNameUser.Text, txtUpdateUserPassword.Password, txtUpdateUseremail.Text, txtUpdateUserAddress.Text, cmbUpdateUserRole)) return;
            var selectedUser = (UserEntity)dGUser.SelectedItem;
            if (selectedUser is null) return;
            selectedUser.UserName = txtUpdateAddNameUser.Text;
            selectedUser.Name = txtUpdateAddNameUser.Text;
            selectedUser.Role = _roles[cmbUpdateUserRole.SelectedIndex];
            selectedUser.Password = txtUpdateUserPassword.Password;
            selectedUser.Email = txtUpdateUseremail.Text;
            selectedUser.Address = txtUpdateUserAddress.Text;
            if (!txtUpdateUseremail.Text.IsValidEmail())
            {
                return;
            }
            selectedUser.Address = txtUpdateUserAddress.Text;
            await _httpRequestUser.UpdateRequest(selectedUser, Api.UPDATEUSERBYID, selectedUser.UserId);
            await UpdateData();
            ClearTextBoxes();
        }



        private async void btnUserAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValideUser(txtAddNameUser.Text, txtAddNameUser.Text, txtUserPassword.Password, txtUseremail.Text, txtUserAddress.Text, cmbUserRole)) return;
            var userToAdd = new UserEntity();
            userToAdd.UserName = txtAddUserNameUser.Text;
            userToAdd.Name = txtAddNameUser.Text;
            userToAdd.Role = _roles[cmbUserRole.SelectedIndex];
            userToAdd.Password = txtUserPassword.Password;
            userToAdd.Email = txtUseremail.Text;
            userToAdd.Address = txtUserAddress.Text;
            if (!txtUseremail.Text.IsValidEmail())
            {
                return;
            }
            await _httpRequestUser.PostRequest(userToAdd, Api.ADDUSER);
            await UpdateData();
            ClearTextBoxes();
        }
    
        private void dGUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = (UserEntity)dGUser.SelectedItem;
            if (selectedUser is null) return;
            txtUpdateAddUserNameUser.Text = selectedUser.UserName;
            txtUpdateAddNameUser.Text = selectedUser.Name;
            cmbUpdateUserRole.SelectedValue = selectedUser.Role;
            txtUpdateUserPassword.Password = selectedUser.Password;
            txtUpdateUseremail.Text = selectedUser.Email;
            txtUpdateUserAddress.Text = selectedUser.Address;
        }
        #region DeleteButtons
        // delete buttons
        private async void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dGProducts.SelectedItem is not ProductEntity productEntity) return;
            await dGProducts.DeleteItem<ProductEntity>(productEntity.ProductId, _httpRequestProduct, Api.DELETEPRODUCT, async task => await UpdateData());
        }

        private async void btnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = dGCategories.SelectedItem as CategoryEntity;
            if (selectedCategory is null) return;

            await dGCategories.DeleteItem<CategoryEntity>(selectedCategory.CategoryId, _httpRequestCategory, Api.DELETECATEGORY, async task => await UpdateData());
        }

        private async void btnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            var selectedSupplier = dGSupplier.SelectedItem as SupplierEntity;
            if (selectedSupplier is null) return;

            await dGSupplier.DeleteItem<SupplierEntity>(selectedSupplier.SupplierId, _httpRequestSupplier, Api.DELETESUPPLIER, async task => await UpdateData());
        }

        private async void btnDeleteInventoryItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedInventoryItem = dGInventory.SelectedItem as InventoryItemEntity;
            if (selectedInventoryItem is null) return;

            await dGInventory.DeleteItem<InventoryItemEntity>(selectedInventoryItem.InventoryItemId, _httpRequestInventoryItem, Api.DELETEINVENTORYITEM, async task => await UpdateData());
        }

        private async void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = dGUser.SelectedItem as UserEntity;
            if (selectedUser is null) return;
            await dGUser.DeleteItem<UserEntity>(selectedUser.UserId, _httpRequestUser, Api.DELETEBYUSERBYID, async task => await UpdateData());
        }
        #endregion
        //validations
        #region Validations
        private bool CheckAddSupplierValidation(string name, string address, string phone, string email)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) ||
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            if (!email.IsValidEmail())
            {
                MessageBox.Show("Email is not valid!");
                return false;

            }
            if (!phone.IsValidPhoneNumber())
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
            if (!int.TryParse(txtInventoryAddQuantity.Text, out inventoryId.quantity))
            {

                MessageBox.Show("quantity has to a number!");
                return false;
            }
            return true;
        }
        private bool CheckValidInventoryOnUpdate()
        {

            if (string.IsNullOrEmpty(txtInventoryUpdateProductID.Text) || string.IsNullOrEmpty(txtInventoryUpdateCateogryID.Text) ||
              string.IsNullOrEmpty(txtInventoryUpdateSupplierID.Text) || string.IsNullOrEmpty(txtInventoryUpdateQuantity.Text))
            {
                MessageBox.Show("All input field are required!");
                return false;
            }
            if (!int.TryParse(txtInventoryUpdateProductID.Text, out inventoryId.productId))
            {

                MessageBox.Show("Product id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryUpdateCateogryID.Text, out inventoryId.categoryId))
            {

                MessageBox.Show("category id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryUpdateSupplierID.Text, out inventoryId.supplierId))
            {

                MessageBox.Show("supplier id has to a number!");
                return false;
            }
            if (!int.TryParse(txtInventoryUpdateQuantity.Text, out inventoryId.quantity))
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
        private bool ValideUser(string userName, string name, string password, string email, string addres, ComboBox role)
        {
            if(string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Username is empty!");
                return false;
            }
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is empty!");
                return false;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("password is empty!");
                return false;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("email is empty!");
                return false;
            }
            if (string.IsNullOrEmpty(addres))
            {
                MessageBox.Show("addres is empty!");
                return false;
            }
            if (role.SelectedItem is null)
            {
                MessageBox.Show("role is empty!");
                return false;
            }
            return true;
        }
        #endregion
        // update grids
        private async Task UpdateData()
        {
            productsTask = _httpRequestProduct.GetRequestListAsync(Api.PRODUCT);
            categoriesTask = _httpRequestCategory.GetRequestListAsync(Api.CATEGORY);
            inventoryTask = _httpRequestInventoryItem.GetRequestListAsync(Api.INVENTORY);
            supplierTask = _httpRequestSupplier.GetRequestListAsync(Api.SUPPLIER);
            userTask = _httpRequestUser.GetRequestListAsync(Api.USERS);
            await Task.WhenAll(productsTask, categoriesTask, inventoryTask, supplierTask, userTask);

            Dispatcher.Invoke(() =>
            {
                dGProducts.ItemsSource = productsTask.Result;
                dGCategories.ItemsSource = categoriesTask.Result;
                dGInventory.ItemsSource = inventoryTask.Result;
                dGSupplier.ItemsSource = supplierTask.Result;
                dGUser.ItemsSource = userTask.Result;
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
        private void ClearTextBoxes()
        {
            txtUpdateAddNameUser.Text = string.Empty;
            txtUpdateAddUserNameUser.Text = string.Empty;
            txtUpdateUserAddress.Text = string.Empty;
            txtUpdateUserPassword.Password = string.Empty;
            txtUpdateUseremail.Text = string.Empty;


            txtAddNameUser.Text = string.Empty;
            txtAddUserNameUser.Text = string.Empty;
            txtUseremail.Text = string.Empty;
            txtUserPassword.Password = string.Empty;
            txtUserAddress.Text = string.Empty;
        }

        #endregion
        // tabitems
 

        private void sPProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemProduct.IsSelected = true;
        }

        private void sPCategory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemCategories.IsSelected = true;
        }

        private void sPTotalSupplier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemSupplier.IsSelected = true;
        }

        private void sPInventoryItems_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemInventory.IsSelected = true;
        }
    }
}
