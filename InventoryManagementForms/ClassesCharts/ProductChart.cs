using MongoDB.Bson;
using Newtonsoft.Json;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.Classes
{
    public class ProductChart
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public ChartColorPalette Color => Quantity > 50 ?  ChartColorPalette.BlueChrome : Quantity < 50 ? ChartColorPalette.FloraHues: ChartColorPalette.RedChrome;
  
    }
}
