using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FitbitWorkout
{
    /// <summary>
    /// Interaction logic for OAuthWindow.xaml
    /// </summary>
    public partial class OAuthWindow : Window
    {
        private readonly string _baseUrl = "https://www.fitbit.com/oauth2/authorize";
        private readonly string _redeemUrl = "https://api.fitbit.com/oauth2/token";
        private readonly string _redirectUri = "http://localhost:3939";
        private string _scope
        {
            get
            {
                return string.Join("%20", new string[] { "sleep", "weight", "nutrition", "activity" });
            }
        }

        public OAuthWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string authUri = $"{_baseUrl}?response_type=code&client_id={Settings.Default.ClientID}&scope={_scope}&redirect_uri={_redirectUri}";
            webBrowser.Navigate(authUri);
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.Query.Contains("code="))
            {
                string code = e.Uri.Query.Substring(1).Split('&')[0].Split('=')[1];

                var client = new RestClient(_redeemUrl);
                var request = new RestRequest(Method.POST);

                string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Settings.Default.ClientID}:{Settings.Default.ClientSecret}"));
                request.AddHeader("Authorization", $"Basic {base64String}");
                // request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                string requestBody = $"client_id={Settings.Default.ClientID}&grant_type=authorization_code&redirect_uri={_redirectUri}&code={code}";
                request.AddParameter("application/x-www-form-urlencoded", requestBody, ParameterType.RequestBody);
                
                var response = (RestResponse)client.Execute(request);
                var content = response.Content;

                // Parse content and get auth code
                Settings.Default.AccessToken = JObject.Parse(content)["access_token"].Value<string>();
                Settings.Default.Save();

                Close();
            }
        }
    }
}
