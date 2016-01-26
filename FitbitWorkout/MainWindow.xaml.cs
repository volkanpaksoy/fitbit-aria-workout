using Newtonsoft.Json.Linq;
using RestSharp;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitbitWorkout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FitbitWeightResult> WeightResultList { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            WeightResultList = new ObservableCollection<FitbitWeightResult>();

            var columnSeries1 = new ColumnSeries()
            {
                ItemsSource = WeightResultList,
                XBindingPath = "Date",
                YBindingPath = "Weight",
                EnableAnimation = true,
                ShowTooltip = true
            };

            var columnSeries2 = new ColumnSeries()
            {
                ItemsSource = WeightResultList,
                XBindingPath = "Date",
                YBindingPath = "Bmi",
                EnableAnimation = true,
                ShowTooltip = true
            };

            weightChart.Series.Add(columnSeries1);
            weightChart.Series.Add(columnSeries2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWeightData();
            }
            catch (HttpException httpEx) when (httpEx.GetHttpCode() == 401)
            {
                GetAuthorization();
                GetWeightData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dammit!", MessageBoxButton.OK);
            }
        }

        public void GetAuthorization()
        {
            new OAuthWindow().ShowDialog();
        }

        public void GetWeightData()
        {
            var endDate = DateTime.Today.ToString("yyyy-MM-dd");
            var startDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            var url = $"https://api.fitbit.com/1/user/{Settings.Default.UserID}/body/log/weight/date/{startDate}/{endDate}.json";
            var response = SendRequest(url);

            ParseWeightData(response);
        }

        public void ParseWeightData(string rawContent)
        {
            var weightJsonArray = JObject.Parse(rawContent)["weight"].ToArray();
            foreach (var weightJson in weightJsonArray)
            {
                var weight = new FitbitWeightResult();
                weight.Weight = weightJson["weight"]?.Value<decimal>() ?? 0;
                weight.Date = weightJson["date"].Value<DateTime>();
                WeightResultList.Add(weight);
            }
        }

        private string SendRequest(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {Settings.Default.AccessToken}");
            var response = (RestResponse)client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new HttpException(401, "Unauthorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content;
            }

            throw new HttpException((int)response.StatusCode, response.StatusDescription);
        }
    }
}
