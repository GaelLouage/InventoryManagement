using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Infrastructuur.Entities;
using InventoryManagementForms.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InventoryManagementForms.Extensions
{
    public static class AddRanges
    {

        public static void AddRange<T>(this ListBox lb, List<T> data, List<string>? properties = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            foreach (var item in data)
            {
              
                var sb = new StringBuilder();
                foreach (var property in properties)
                {
                    var prop1 = item?.GetType().GetProperty(property)?.GetValue(item);
                    var propName = item?.GetType().GetProperty(property).Name;
                    sb.AppendLine($"{propName}: \t {prop1}");
                }
                sb.AppendLine();
                lb.Items.Add(sb.ToString());
                lb.Items.Add(new Separator());
            }
        }
        public static void AddRange<T>(this ObservableCollection<T> range, List<T> data) 
        {
            foreach (var item in  data)
            {
                range.Add(item);
            }
        }

        //public static void AddRange<T>(this ListBox lb, List<T> data, string prop1Name, string prop2Name)
        //{
        //    if (data == null)
        //    {
        //        throw new ArgumentNullException(nameof(data));
        //    }

        //    foreach (var item in data)
        //    {
        //        var prop1 = item?.GetType().GetProperty(prop1Name)?.GetValue(item);
        //        var prop2 = item?.GetType().GetProperty(prop2Name)?.GetValue(item);

        //        if (prop1 == null || prop2 == null)
        //        {
        //            continue;
        //        }

        //        var sb = new StringBuilder();
        //        sb.AppendLine($"{prop1Name}: \t {prop1}");
        //        sb.AppendLine($"{prop2Name}: \t {prop2}");
        //        sb.AppendLine();

        //        lb.Items.Add(sb.ToString());
        //        lb.Items.Add(new Separator());
        //    }
        //}
    }
}
