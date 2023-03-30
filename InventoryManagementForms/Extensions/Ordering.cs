using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.Extensions
{
    public static class Ordering
    {
        public static ObservableCollection<T> OrderBy<T>(this ObservableCollection<T> collection, List<T> data, Func<T, object> del) where T: class
        {
            data = collection.ToList();
            data = data.OrderBy(del).ToList();
            var obs = new ObservableCollection<T>();    
            foreach(var item in data)
            {
                obs.Add(item);
            }
            return obs;
        }
        public static ObservableCollection<T> OrderByDescending<T>(this ObservableCollection<T> collection, List<T> data, Func<T, object> del) where T : class
        {
            data = collection.ToList();
            data = data.OrderByDescending(del).ToList();
            var obs = new ObservableCollection<T>();
            foreach (var item in data)
            {
                obs.Add(item);
            }
            return obs;
        }
    }
}
