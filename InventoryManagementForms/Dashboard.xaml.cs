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
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.Charts;
using InventoryManagementForms.Classes;
using iTextSharp.text.pdf.qrcode;
using ZXing;
using ZXing.Common;
using System.Drawing;
using ZXing.QrCode;
using Microsoft.Office.Interop.Excel;
using SharpCompress.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using System.DirectoryServices;
using DocumentFormat.OpenXml.Bibliography;
using System.Globalization;


namespace InventoryManagementForms
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : System.Windows.Window
    {
        private readonly IHttpRequest<ProductEntity> _httpRequestProduct;
        private readonly IHttpRequest<CategoryEntity> _httpRequestCategory;
        private readonly IHttpRequest<InventoryItemEntity> _httpRequestInventoryItem;
        private readonly IHttpRequest<SupplierEntity> _httpRequestSupplier;
        private readonly IHttpRequest<UserEntity> _httpRequestUser;
        private readonly IHttpRequest<OrderEntity> _httpRequestOrder;
        private Task<List<ProductEntity>> productsTask;
        private Task<List<CategoryEntity>> categoriesTask;
        private Task<List<SupplierEntity>> supplierTask;
        private Task<List<InventoryItemEntity>> inventoryTask;
        private Task<List<UserEntity>> userTask;
        private Task<List<OrderEntity>> orderTask;
        private ValidInventoryId inventoryId = new ValidInventoryId();
        private ProductStruct productStruct = new ProductStruct();
        private UserEntity _user;
        private string[] _roles = new string[2] { Role.SUPERADMIN, Role.ADMIN };
        // charts product
        public ObservableCollection<ProductChart> Products { get; set; } = new ObservableCollection<ProductChart>();
        public List<ProductEntity> ProductsList { get; set; } = new List<ProductEntity>();
        // order status chart 
        public ObservableCollection<OrderStatusChartEntity> OrderStatusChartEntities = new ObservableCollection<OrderStatusChartEntity>();
        public Dashboard(IHttpRequest<ProductEntity> httpRequestProduct, IHttpRequest<CategoryEntity> httpRequestCategory, IHttpRequest<InventoryItemEntity> httpRequestInventoryItem, IHttpRequest<SupplierEntity> httpRequestSupplier, IHttpRequest<UserEntity> httpRequestUser, IHttpRequest<OrderEntity> httpRequestOrder)
        {
            _httpRequestProduct = httpRequestProduct;
            _httpRequestCategory = httpRequestCategory;
            _httpRequestInventoryItem = httpRequestInventoryItem;
            _httpRequestSupplier = httpRequestSupplier;
            _httpRequestUser = httpRequestUser;
            _httpRequestOrder = httpRequestOrder;
        }
        public Dashboard(UserEntity user) : this(new HttpRequest<ProductEntity>(Api.BASEURL),
                                 new HttpRequest<CategoryEntity>(Api.BASEURL),
                             new HttpRequest<InventoryItemEntity>(Api.BASEURL),
                                 new HttpRequest<SupplierEntity>(Api.BASEURL),
                                 new HttpRequest<UserEntity>(Api.BASEURL),
                                 new HttpRequest<OrderEntity>(Api.BASEURL))
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
            // TODO make all those in separate methods
            await UpdateData();
            ComboboxPopulator();
            GridVisibility();

            await SetDashboardTabs();
            // hide the user tab for non superadmins
            SuperAdminAccesTabs();
            HideTabItems();
            // charts section
            // hide charts
            chartProducts.Visibility = Visibility.Hidden;
            lbProductLessThan5.Visibility = Visibility.Hidden;
            sPWarningProductsQuantity.Visibility = Visibility.Hidden;
            lbProductLessThan5.Visibility = Visibility.Hidden;
            chartColumProducts.ItemsSource = Products;

            Products.AddRange<ProductChart>((await productsTask).Select(p => new ProductChart { Name = p.Name, Quantity = p.Quantity, }).ToList());
            ProductsList.AddRange((await productsTask).ToList());

            chartProducts.DataContext = this;
            // add data to the listbox
            lbProductLessThan5.AddRange<ProductEntity>(ProductsList.Where(x => x.Quantity < 5).ToList(), new() { ProductProperty.NAME, ProductProperty.QUANTITY });
            //chart revenue
            // get all orders groupby date month and add all revenues on date date together 
            var orders = await orderTask;
            var od  = new List<RevenueChartEntity>()
             {
            new RevenueChartEntity() { XValue = "Jan", YValue = orders.Where(x => x.OrderDate.Month == 1).Sum(x => x.TotalAmount)},
            new RevenueChartEntity() { XValue = "Feb", YValue = orders.Where(x => x.OrderDate.Month == 2).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Mar", YValue = orders.Where(x => x.OrderDate.Month == 3).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Apr", YValue = orders.Where(x => x.OrderDate.Month == 4).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "May", YValue = orders.Where(x => x.OrderDate.Month == 5).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Jun", YValue = orders.Where(x => x.OrderDate.Month == 6).Sum(x => x.TotalAmount) },
              new RevenueChartEntity() { XValue = "Jul", YValue = orders.Where(x => x.OrderDate.Month == 7).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Aug", YValue = orders.Where(x => x.OrderDate.Month == 8).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Sept", YValue = orders.Where(x => x.OrderDate.Month == 9).Sum(x => x.TotalAmount) },
              new RevenueChartEntity() { XValue = "Okt", YValue = orders.Where(x => x.OrderDate.Month == 10).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Nov", YValue = orders.Where(x => x.OrderDate.Month == 11).Sum(x => x.TotalAmount) },
            new RevenueChartEntity() { XValue = "Dec", YValue = orders.Where(x => x.OrderDate.Month == 12).Sum(x => x.TotalAmount) }
              };
            chartRevenue.ItemsSource = od;
            var statusC = orders.GroupBy(x => x.Status).ToList();
            OrderStatusChartEntities = new ObservableCollection<OrderStatusChartEntity>();
            foreach ( var status in statusC)
            {
                OrderStatusChartEntities.Add(new OrderStatusChartEntity { Status= status.Key, AmountCurrentStatus = status.Count() });
            }
   
            OrderStatusChart.ItemsSource = OrderStatusChartEntities;
        }
        private void lbProductLessThan5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // place in method extension
            byte[] byteArray;

            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 400,
                    Width = 400,
                    Margin = 0
                }
            };
            var productsLessThan5 = ProductsList.Where(x => x.Quantity < 5).ToList();
            ProductEntity? product = null;
            if (productsLessThan5.Count < lbProductLessThan5.SelectedIndex || productsLessThan5[lbProductLessThan5.SelectedIndex] == null)
            {
                return;
            }
            product = productsLessThan5[lbProductLessThan5.SelectedIndex];
            if (product is null) return;
            var pixelData = qrCodeWriter.Write($"{product.Name} \n {product.Quantity}");
            // creating a PNG bitmap from the raw pixel data; if only black and white colors are used it makes no difference if the raw pixel data is BGRA oriented and the bitmap is initialized with RGB
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                using (var ms = new MemoryStream())
                {
                    var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    try
                    {
                        // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                    // save to folder
                    //string fileGuid = Guid.NewGuid().ToString().Substring(0, 4);
                    //bitmap.Save(Server.MapPath("~/qrr") + "/file-" + fileGuid + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    // save to stream as PNG
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byteArray = ms.ToArray();
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(byteArray);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    barcodeProductImage.Source = image;
                }
            }
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
        private async void txtOrderSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            dGOrder.ItemsSource = (await orderTask).Where(x => x.Status.StartsWith(txtOrderSearch.Text, StringComparison.OrdinalIgnoreCase));
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
        private async void cmbOrderSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (orderTask is null) return;
            var selected = (OrderSort)cmbOrderSort.SelectedItem;
            var dic = OrderDictionary.SortingOrderOptions[selected];
            dGOrder.ItemsSource = (await orderTask).OrderBy(dic);

        }
        #endregion
        #region Navigation
        // navigations product
        private void btnAddProductGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string chartButton = btnChartProduct.Source.ToString().Split("/").Last();
            if (chartButton == "chart.png")
            {
                dGridAddProductForm.Visibility = Visibility.Visible;
                dGridUpdateProductForm.Visibility = Visibility.Hidden;
            }
        }
        private void btnupdateProductGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string chartButton = btnChartProduct.Source.ToString().Split("/").Last();
            if (chartButton == "chart.png")
            {
                dGridAddProductForm.Visibility = Visibility.Hidden;
                dGridUpdateProductForm.Visibility = Visibility.Visible;
            }
        }
        // navigation category
        private void btnAddCategoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddCategory.Visibility = Visibility.Visible;
            dPUpdateCategory.Visibility = Visibility.Hidden;
        }

        private void btnupdateCategorytGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddCategory.Visibility = Visibility.Hidden;
            dPUpdateCategory.Visibility = Visibility.Visible;
        }
        // navigation supplier
        private void btnAddSupplierGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddSupplier.Visibility = Visibility.Visible;
            dPUpdateSupplier.Visibility = Visibility.Hidden;
        }

        private void btnupdateSuppliertGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddSupplier.Visibility = Visibility.Hidden;
            dPUpdateSupplier.Visibility = Visibility.Visible;
        }
        // navigation inventory

        private void btnAddInventoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddInvetory.Visibility = Visibility.Visible;
            dPUpdateInventory.Visibility = Visibility.Hidden;
        }

        private void btnupdateInventoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddInvetory.Visibility = Visibility.Hidden;
            dPUpdateInventory.Visibility = Visibility.Visible;
        }
        // navigation add user
        private void btnAddUserGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridUserForm.Visibility = Visibility.Visible;
            dGridUpdateUserForm.Visibility = Visibility.Hidden;
        }

        private void btnupdateUserGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGridUserForm.Visibility = Visibility.Hidden;
            dGridUpdateUserForm.Visibility = Visibility.Visible;
        }
        // navigation add orders
        private void btnAddOrderGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddOrder.Visibility = Visibility.Visible;
            dPUpdateOrder.Visibility = Visibility.Hidden;
        }

        private void btnupdateOrdertGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dPAddOrder.Visibility = Visibility.Hidden;
            dPUpdateOrder.Visibility = Visibility.Visible;
        }
        #endregion
        #region PDFDocs
        // excel documents
        private void btnPdfProductGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGProducts.ExcelSaver("Product.xlsx");
        }

        private void btnPdfCategoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGCategories.ExcelSaver("Category.xlsx");
        }

        private void btnPdfSupplierGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGSupplier.ExcelSaver("Supplier.xlsx");
        }

        private void btnPdfInventoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGInventory.ExcelSaver("Inventory.xlsx");
        }
        private void btnPdfUserGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGUser.ExcelSaver("User.xlsx");
        }

        private void btnPdfOrderGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGOrder.ExcelSaver("Order.xlsx");
        }
        #endregion
        #region PrintButtons
        //print documents

        private void btnPrintProductGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGProducts, Data.Product);
        }
        private void btnPrintCategoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGCategories, Data.Category);
        }

        private void btnPrintSupplierGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGSupplier, Data.Supplier);
        }

        private void btnPrintInventoryGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGInventory, Data.Inventory);
        }
        private void btnPrintUserGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGUser, Data.User);
        }
        private void btnPrintOrderGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Printer.PrintData(dGOrder, Data.Order);
        }

        #endregion
        // product forms

        private async void btnProductAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckAddProductValidation(txtProductName.Text, txtProductPrice.Text, txtProductDescription.Text, txtProductQuantity.Text))
            {
                return;
            }
            txtUpdateProductPrice.Text = txtUpdateProductPrice.Text.Replace(",", ".");
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
            productDto.Price = productStruct.price;
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
            /* NumberStyles.Number flag and the CultureInfo.InvariantCulture object to ensure that the decimal separator is always a period character.*/
            if (!decimal.TryParse(ProductPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out productStruct.price))
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
            if (!ValideUser(txtUpdateAddUserNameUser.Text, txtUpdateAddNameUser.Text, txtUpdateUserPassword.Password, txtUpdateUseremail.Text, txtUpdateUserAddress.Text, cmbUpdateUserRole)) return;
            var selectedUser = (UserEntity)dGUser.SelectedItem;
            if (selectedUser is null) return;
            selectedUser.UserName = txtUpdateAddUserNameUser.Text;
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
            if (!ValideUser(txtAddUserNameUser.Text, txtAddNameUser.Text, txtUserPassword.Password, txtUseremail.Text, txtUserAddress.Text, cmbUserRole)) return;
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
        // order form

        private void btnOrderUpdateItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void dGOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOrder = (OrderEntity)dGOrder.SelectedItem;
            if (selectedOrder is null) return;
            txtUpdateCustomerId.Text = selectedOrder.CustomerId.ToString();
            dPickerOrderUpdater.SelectedDate = selectedOrder.OrderDate;
            txtUpdateOrderStatus.Text = selectedOrder.Status;
        }

        #region DeleteButtons
        // delete buttons

        private void btnRemoveProductGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGProducts.RemoveData<ProductEntity>(nameof(ProductOrder), nameof(ProductOrder.ProductId), _httpRequestProduct, Api.DELETEPRODUCT, async task => await UpdateData());
        }

        private void btnRemoveCategory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGCategories.RemoveData<CategoryEntity>(nameof(InventoryManagementForms.Enums.Category), nameof(InventoryManagementForms.Enums.Category.CategoryId), _httpRequestCategory, Api.DELETECATEGORY, async task => await UpdateData());
        }

        private void btnRemoveSupplier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGSupplier.RemoveData<SupplierEntity>(nameof(Supplier), nameof(Supplier.SupplierId), _httpRequestSupplier, Api.DELETESUPPLIER, async task => await UpdateData());
        }

        private void btnRemoveInventoryItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGInventory.RemoveData<InventoryItemEntity>(nameof(Inventory), nameof(Inventory.InventoryItemId), _httpRequestInventoryItem, Api.DELETEINVENTORYITEM, async task => await UpdateData());
        }

        private void btnRemoveUser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGInventory.RemoveData<UserEntity>(nameof(UserS), nameof(InventoryManagementForms.Enums.UserS.UserId), _httpRequestUser, Api.DELETEBYUSERBYID, async task => await UpdateData());
        }
        private void btnRemoveOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dGOrder.RemoveData<OrderEntity>(nameof(OrderSort), nameof(InventoryManagementForms.Enums.OrderSort.OrderId), _httpRequestOrder, Api.DELETEORDER, async task => await UpdateData());

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
            if (string.IsNullOrEmpty(userName))
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
            orderTask = _httpRequestOrder.GetRequestListAsync(Api.ORDERS);
            await Task.WhenAll(productsTask, categoriesTask, inventoryTask, supplierTask, userTask, orderTask);

            Dispatcher.Invoke(() =>
            {
                dGProducts.ItemsSource = productsTask.Result;
                dGCategories.ItemsSource = categoriesTask.Result;
                dGInventory.ItemsSource = inventoryTask.Result;
                dGSupplier.ItemsSource = supplierTask.Result;
                dGUser.ItemsSource = userTask.Result;
                dGOrder.ItemsSource = orderTask.Result;
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
        // chart buttons
        private void btnChartProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var chartLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", "chart.png");
            var dataLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", "data.png");
            var imageName = dataLocation.Split("\\").Last();
            var sourceName = btnChartProduct.Source.ToString().Split("/").Last();
            txtProductSearch.Visibility = Visibility.Hidden;
            sPWarningProductsQuantity.Visibility = Visibility.Visible;
            cmbProduct.Visibility = Visibility.Hidden;
            dGProducts.Visibility = Visibility.Hidden;
            dGridUpdateProductForm.Visibility = Visibility.Hidden;
            chartProducts.Visibility = Visibility.Visible;
            dGProducts.Visibility = Visibility.Hidden;
            dGridAddProductForm.Visibility = Visibility.Hidden;
            dGridAddProductForm.Visibility = Visibility.Hidden;
            dGridAddProductForm.Visibility = Visibility.Hidden;
            lbProductLessThan5.Visibility = Visibility.Visible;
            barcodeProductImage.Visibility = Visibility.Visible;
            cmbSortProductChart.Visibility = Visibility.Visible;
            if (sourceName == imageName)
            {
                dGProducts.Visibility = Visibility.Visible;
                chartProducts.Visibility = Visibility.Hidden;
                dGridAddProductForm.Visibility = Visibility.Visible;
                dGridUpdateProductForm.Visibility = Visibility.Hidden;
                cmbSortProductChart.Visibility = Visibility.Hidden;
                lbProductLessThan5.Visibility = Visibility.Hidden;
                btnChartProduct.Source = new BitmapImage(new Uri(chartLocation));
                txtProductSearch.Visibility = Visibility.Visible;
                cmbProduct.Visibility = Visibility.Visible;
                sPWarningProductsQuantity.Visibility = Visibility.Hidden;
                barcodeProductImage.Visibility = Visibility.Hidden;
                return;
            }
            btnChartProduct.Source = new BitmapImage(new Uri(dataLocation));
        }
        // chart order
        private async void cmbSortProductChart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var sort = (ChartOrder)cmbSortProductChart.SelectedItem;
            if (sort == null) return;
            var sorted = OrderDictionary.SortingChartOptions[sort];
            chartColumProducts.ItemsSource = null;
            if (sort == ChartOrder.QUANTITYPLUS)
            {
                chartColumProducts.ItemsSource = Products.OrderByDescending<ProductChart>(Products.ToList(), sorted);
                return;
            }
            chartColumProducts.ItemsSource = Products.OrderBy<ProductChart>(Products.ToList(), sorted);
        }

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


        private void sPUsers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemUsers.IsSelected = true;
        }
        private void sPCustomers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemCustomer.IsSelected = true;
        }

        private void sPOrders_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemOrders.IsSelected = true;
        }
        // populate the comboboxes
        private void ComboboxPopulator()
        {
            // populate combobox with enum types
            cmbProduct.ItemsSource = Enum.GetValues(typeof(ProductOrder)).Cast<ProductOrder>();
            cmbCategory.ItemsSource = Enum.GetValues(typeof(InventoryManagementForms.Enums.Category)).Cast<InventoryManagementForms.Enums.Category>();
            cmbSupplier.ItemsSource = Enum.GetValues(typeof(Supplier)).Cast<Supplier>();
            cmbInventory.ItemsSource = Enum.GetValues(typeof(Inventory)).Cast<Inventory>();
            cmbUser.ItemsSource = Enum.GetValues(typeof(UserS)).Cast<UserS>();
            cmbSortProductChart.ItemsSource = Enum.GetValues(typeof(ChartOrder)).Cast<ChartOrder>();
            cmbOrderSort.ItemsSource = Enum.GetValues(typeof(OrderSort)).Cast<OrderSort>();
            // user roles adder to user form
            cmbUserRole.Items.Add(Role.SUPERADMIN);
            cmbUserRole.Items.Add(Role.ADMIN);
            cmbUpdateUserRole.Items.Add(Role.SUPERADMIN);
            cmbUpdateUserRole.Items.Add(Role.ADMIN);
        }
        private void GridVisibility()
        {
            // grids visibility
            dGridUpdateProductForm.Visibility = Visibility.Hidden;
            dPUpdateCategory.Visibility = Visibility.Hidden;
            dPUpdateSupplier.Visibility = Visibility.Hidden;
            dPUpdateInventory.Visibility = Visibility.Hidden;
            dGridUpdateUserForm.Visibility = Visibility.Hidden;
            dPUpdateOrder.Visibility = Visibility.Hidden;
            // combobox chart product
            cmbSortProductChart.Visibility = Visibility.Hidden;
        }
        private async Task SetDashboardTabs()
        {

            // dashboard
            lblTotalProducts.Content = (await productsTask).Count();
            lblTotalInventory.Content = (await inventoryTask).Count();
            lblTotalCategories.Content = (await categoriesTask).Count();
            lblTotalSupplier.Content = (await supplierTask).Count();
            lblTotalUsers.Content = (await userTask).Count();
            lblTotalOrders.Content = (await orderTask).Count();
        }
        private void SuperAdminAccesTabs()
        {
            if (_user.Role is not Role.SUPERADMIN)
            {
                tbItemUsers.Visibility = Visibility.Hidden;
                sPUsers.Visibility = Visibility.Hidden;
            }
        }
        // navigation tab
        private void sPNavHome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbNavigation.IsSelected = true;

        }

        private void sPNavProducts_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemProduct.IsSelected = true;
        }

        private void sPNavCategories_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemCategories.IsSelected = true;
        }
        private void sPNavSuppliers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemSupplier.IsSelected = true;
        }

        private void sPNavInventory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemInventory.IsSelected = true;
        }

        private void sPNavUser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemUsers.IsSelected = true;
        }
        private void sPNavCustomer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tbItemCustomer.IsSelected = true;
        }

        private void sPNavOrders_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            tItemOrders.IsSelected = true;
        }

        private void sPNavClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var login = new MainWindow();
            login.Show();
            this.Close();
        }

        // hide tabitems 
        private void HideTabItems()
        {
            tbNavigation.Visibility = Visibility.Hidden;
            tbItemProduct.Visibility = Visibility.Hidden;
            tbItemCategories.Visibility = Visibility.Hidden;
            tItemInventory.Visibility = Visibility.Hidden;
            tItemSupplier.Visibility = Visibility.Hidden;
            tbItemUsers.Visibility = Visibility.Hidden;
            tItemOrders.Visibility = Visibility.Hidden;
            tbItemCustomer.Visibility = Visibility.Hidden;
            if (_user.Role is not Role.SUPERADMIN)
            {
                tbItemUsers.Visibility = Visibility.Hidden;
                sPNavClose.Visibility = Visibility.Hidden;
                return;
            }
        }
    }
}
