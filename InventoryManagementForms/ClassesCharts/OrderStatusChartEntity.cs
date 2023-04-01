using iTextSharp.text.pdf.security;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InventoryManagementForms.Classes
{
    public class OrderStatusChartEntity  
    {
        public string? Status { get; set; }
        public int? AmountCurrentStatus { get; set; }

        public SolidColorBrush Interior => Status == "Send" ? new SolidColorBrush(System.Windows.Media.Colors.Green) :
            Status == "In progress" ? new SolidColorBrush(System.Windows.Media.Colors.LightBlue) :
            Status == "Process" ? new SolidColorBrush(System.Windows.Media.Colors.Brown) : new SolidColorBrush(System.Windows.Media.Colors.LightGreen);

    }
}
