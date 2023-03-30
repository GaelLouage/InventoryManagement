using InventoryManagementForms.ApiService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        // removes a selected item in a grid
        public static async void RemoveData<T>(this DataGrid data, string type, string propName, IHttpRequest<T> httpRequest, string apiRoute, Action<Task> updateData) where T : class
        {
            if (data.SelectedIndex < 0)
            {
                MessageBox.Show($"No prudct selected a {type}!");
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete the {type}?", "Confirmation", MessageBoxButton.YesNo);

            // Check if the user clicked the Yes button
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (data.SelectedItem is not T entity) return;
                var val = int.Parse(entity.GetType().GetProperty(propName).GetValue(entity).ToString());
                await data.DeleteItem<T>(val, httpRequest, apiRoute, updateData);
                MessageBox.Show($"Successfully deleted {type}.", "Success", MessageBoxButton.OK);
                return;
            }
            return;
        }
    }
}
