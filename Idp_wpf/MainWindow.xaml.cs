using IdentityModel.Client;
using System;
using System.Collections.Generic;
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

namespace Idp_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string _accessToken { get; set; }
        public string _userInfoEndPoint { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void getToken_Click(object sender, RoutedEventArgs e)
        {
            var uname = username.Text;
            var upwd = password.Password;
            if (string.IsNullOrWhiteSpace(uname))
            {
                MessageBox.Show("用户名不能为空！");
                return;
            }
            if (string.IsNullOrWhiteSpace(upwd))
            {
                MessageBox.Show("密码不能为空！");
                return;
            }
                
            //request access token
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                MessageBox.Show(disco.Error);
                return;
            }
            _userInfoEndPoint = disco.UserInfoEndpoint;

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address=disco.TokenEndpoint,
                ClientId= "wpf client",
                ClientSecret= "wpf client",
                Scope= "scope1 openid profile address phone email",
                UserName=uname,
                Password=upwd
            });
            if (tokenResponse.IsError)
            {
                MessageBox.Show(tokenResponse.Error);
                return;
            }
            _accessToken = tokenResponse.AccessToken;
            accessToken.Text = tokenResponse.Json.ToString();
        }

        private async void getApiResult_Click(object sender, RoutedEventArgs e)
        {
            //call api resource
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(_accessToken);
            var response = await apiClient.GetAsync("https://localhost:5003/identity");
            if (!response.IsSuccessStatusCode)
                MessageBox.Show(response.StatusCode.ToString());
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                apiResult.Text = content;
            }
        }

        private async void getIdeneityResult_Click(object sender, RoutedEventArgs e)
        {
            //call identity resource
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(_accessToken);
            var response = await apiClient.GetAsync(_userInfoEndPoint);
            if (!response.IsSuccessStatusCode)
                MessageBox.Show(response.StatusCode.ToString());
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                identityResult.Text = content;
            }
        }
    }
}
