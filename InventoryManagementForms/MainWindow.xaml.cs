using Infrastructuur.Entities;
using Infrastructuur.Extensions;
using InventoryManagementForms.ApiService.Classes;
using InventoryManagementForms.ApiService.Interfaces;
using InventoryManagementForms.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
        private readonly IHttpRequest<UserEntity> _httpRequestUser;
        private List<UserEntity> _users;
        private UserEntity user;
        public MainWindow(IHttpRequest<UserEntity> httpRequestUser)
        {
            _httpRequestUser = httpRequestUser;
        }

        public MainWindow() :this(new HttpRequest<UserEntity>(Api.BASEURL))
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _users = await _httpRequestUser.GetRequestListAsync(Api.USERS);
        }
        private  void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtUsername.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                CheckIfUserExist();
            }
            else
            {
                MessageBox.Show("TextFields are empty!");
            }
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // close the application because it ensures that all resources are properly released and any remaining background threads are terminated.
            Application.Current.Shutdown();
        }
        // TODO: my methods set it to another folder
        private void CheckIfUserExist()
        {
             user = _users.SingleOrDefault(x => x.UserName == txtUsername.Text && HashPassword.VerifyPassword(txtPassword.Password, x.Password));
            if (user is not null)
            {
                //TODO: make the login logic with algorithm HMCA2
                var dashBoard = new Dashboard(user);
                dashBoard.Show();
                this.Close();
            } else
            {
                MessageBox.Show("Wrong username or password!");
            }
        }
    }
}
