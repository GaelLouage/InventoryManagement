using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryManagementForms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //TODO: make the login logic with algorithm HMCA2
            var dashBoard = new Dashboard();
            dashBoard.Show();
            this.Close();
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // close the application because it ensures that all resources are properly released and any remaining background threads are terminated.
            Application.Current.Shutdown();
        }
    }
}
