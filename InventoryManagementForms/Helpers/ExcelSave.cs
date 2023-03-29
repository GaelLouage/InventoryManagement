using Infrastructuur.Entities;
using InventoryManagementForms.Extensions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InventoryManagementForms.Helpers
{
    public static class ExcelSave
    {
        public static void ExcelSaver(this DataGrid data,string name)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = name;
            saveFileDialog.DefaultExt = "xlsx";
            // Set the filter for the file type
            saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            // Set the initial directory to the user's Documents folder
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Show the SaveFileDialog and get the result
            bool? result = saveFileDialog.ShowDialog();
            // Check if the user clicked the Save button
            if (result != true)
            {
                return;
            }
            // Get the selected file name and path
            string filePath = saveFileDialog.FileName;
            data.WriteToExcelFile(filePath, new ProductEntity());
            filePath = filePath.Replace("\\", "/");
            if (File.Exists(@"" + filePath))
            {
                MessageBox.Show("File succesfully created");
                return;
            }
            MessageBox.Show("Error creating excel file.");
        }
    }
}
