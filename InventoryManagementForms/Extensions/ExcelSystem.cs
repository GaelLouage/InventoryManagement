using ClosedXML;
using Infrastructuur.Entities;
using InventoryManagementForms.ApiService.Interfaces;
using InventoryManagementForms.Enums;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventoryManagementForms.Extensions
{
    public static class ExcelSystem
    {
        public static async void WriteToExcelFile<T>(this DataGrid dataGrid, string fileName, T type   , IHttpRequest<ProductEntity>? request = null, IHttpRequest<CategoryEntity>? request2 = null, IHttpRequest<SupplierEntity>? request3 = null)
        {

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // Set the LicenseContext property

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (int i = 1; i <= dataGrid.Columns.Count; i++)
                {
                    worksheet.Cells[1, i].Value = dataGrid.Columns[i - 1].Header;
                }
               
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {

                    for (int j = 0; j < dataGrid.Columns.Count; j++)
                    {

                        var columnName = dataGrid.Columns[j].SortMemberPath;
                        var property = dataGrid.Items[i].GetType().GetProperty(columnName);

                        if (property == null)
                        {
                            continue;
                        }
                        var value = property.GetValue(dataGrid.Items[i], null);

                        if (value == null)
                        {
                            continue;
                        }
                        if (value is DateTime)
                        {
                            value = ((DateTime)value).ToString("dd/MM/yyyy");
                        }
                        value = await SetInventoryObject(type, request, request2, request3, j, value);
                        worksheet.Cells[i + 2, j + 1].Value = value;
                    }
                }
                package.SaveAs(new FileInfo($"{fileName}.xlsx"));
            }
        }

        private static async Task<object> SetInventoryObject<T>(T type, IHttpRequest<ProductEntity>? request, IHttpRequest<CategoryEntity>? request2, IHttpRequest<SupplierEntity>? request3, int j, object? value)
        {
            if (type is InventoryItemEntity)
            {
                switch (j)
                {
                    case 7:
                        value = (await request.GetRequestListAsync(Api.PRODUCT)).Select(x => x.Name);
                        break;
                    case 8:
                        value = (await request2.GetRequestListAsync(Api.CATEGORY)).Select(x => x.Name);
                        break;
                    case 9:
                        value = (await request3.GetRequestListAsync(Api.SUPPLIER)).Select(x => x.Name);
                        break;
                }
            }

            return value;
        }
    }
}
