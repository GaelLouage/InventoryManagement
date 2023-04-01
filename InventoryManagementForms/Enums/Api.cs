using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.Enums
{
    public static class Api
    {
        //base url
        public const string BASEURL = "https://localhost:7071/api/";
        //inventory
        public const string INVENTORY = "Inventory/GetAllInventoryItems";
        public const string ADDINVENTORYITEM = "Inventory/CreateInventoryItem";
        public const string UPDATEINVENTORYITEM = "Inventory/UpdateInventoryItem/";
        public const string DELETEINVENTORYITEM = "Inventory/DeleteInventoryItem/";
        // supplier
        public const string SUPPLIER = "Supplier/GetAllSuppliers";
        public const string ADDSUPPLIER = "Supplier/CreateSupplier";
        public const string UPDATESUPPLIER = "Supplier/UpdateSupplierById/";
        public const string DELETESUPPLIER = "Supplier/DeleteCategoryById/";
        // products
        public const string PRODUCT = "Product/GetAllProducts";
        public const string ADDPRODUCT = "Product/CreateProduct";
        public const string UPDATEPRODUCT = "Product/UpdateProduct/";
        public const string DELETEPRODUCT = "Product/DeleteProduct/";
        // users
        public const string USERS = "User/GetAllUsers";
        public const string USERSBYCREDENTIALS = "User/GetUserByUserNameAndPassWord";
        public const string UPDATEUSERBYID= "User/UpdateUserById/";
        public const string DELETEBYUSERBYID = "User/DeleteUserById/";
        public const string ADDUSER = "User/CreateUser";
        //category
        public const string ADDCATEGORY = "Category/CreateCategory";
        public const string CATEGORY = "Category/GetAllCategories";
        public const string UPDATECATEGORY = "Category/UpdateCategoryById/";
        public const string DELETECATEGORY = "Category/DeleteCategoryById/";
        // orders
        public const string ORDERS = "Order/GetAllOrders";
        public const string ADDORDER = "Order/CreateOrder";
        public const string ORDREBYID= "Order/GetOrderById/";
        public const string UPDATEORDER = "Order/UpdateOrderById/";
        public const string DELETEORDER = "Order/DeleteOrderById/";
    }
}
