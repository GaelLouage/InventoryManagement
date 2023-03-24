using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.Enums
{
    public static class Api
    {
        public const string BASEURL = "https://localhost:7071/api/";
        public const string PRODUCT = "Product/GetAllProducts";
        public const string CATEGORY = "Category/GetAllCategories";
        public const string INVENTORY = "Inventory/GetAllInventoryItems";
        public const string SUPPLIER = "Supplier/GetAllSuppliers";
        // products
        public const string ADDPRODUCT = "Product/CreateProduct";
        // users
        public const string USERS = "User/GetAllUsers";
        public const string USERSBYCREDENTIALS = "User/GetUserByUserNameAndPassWord";
    }
}
