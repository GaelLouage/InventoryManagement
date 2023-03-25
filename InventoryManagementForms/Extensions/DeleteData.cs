using InventoryManagementForms.ApiService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InventoryManagementForms.Extensions
{
    public static class DeleteData
    {
        public static async Task DeleteItem<T>(this DataGrid? dataGrid, int id, IHttpRequest<T> request, string delete, Action<Task> updateDataAction) where T : class
        {
            var selectedItem = dataGrid.SelectedItem as T;
            if (selectedItem is null) return;
            await request.DeleteRequest(delete, id);
            updateDataAction(Task.FromResult(true));
        }
    }
}
