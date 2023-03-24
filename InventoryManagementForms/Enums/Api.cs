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
        // supplier
        public const string SUPPLIER = "Supplier/GetAllSuppliers";
        public const string ADDSUPPLIER = "Supplier/CreateSupplier";
        // products
        public const string PRODUCT = "Product/GetAllProducts";
        public const string ADDPRODUCT = "Product/CreateProduct";
        // users
        public const string USERS = "User/GetAllUsers";
        public const string USERSBYCREDENTIALS = "User/GetUserByUserNameAndPassWord";
        //category
        public const string ADDCATEGORY = "Category/CreateCategory";
        public const string CATEGORY = "Category/GetAllCategories";
    }
}
