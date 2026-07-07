using HospitalSystem.Helpers;
using HospitalSystem.Services;
using System;
using System.Windows;

namespace HospitalSystem.Views
{
    public partial class ApiWindow : Window
    {
        ApiService api = new ApiService();

        public ApiWindow()
        {
            InitializeComponent();
        }

        private async void GetData_Click(object sender, RoutedEventArgs e)
        {
            GetDataButton.IsEnabled = false;

            try
            {
                string result = await api.GetCurrencyRatesAsync();
                ResultBox.Text = result;

                LogApiCall(200, result);
            }
            catch (Exception ex)
            {
                LogApiCall(0, ex.Message);
                MessageBox.Show("Ошибка API: " + ex.Message);
            }
            finally
            {
                GetDataButton.IsEnabled = true;
            }
        }

        private void LogApiCall(int statusCode, string response)
        {
            string trimmed = response != null && response.Length > 255 ? response.Substring(0, 255) : response;

            using (var db = new HospitalDBEntities())
            {
                db.ApiLogs.Add(new ApiLogs
                {
                    UserID = SessionManager.CurrentUser.UserID,
                    Endpoint = "https://www.cbr-xml-daily.ru/daily_json.js",
                    StatusCode = statusCode,
                    Response = trimmed,
                    RequestDate = DateTime.Now
                });

                db.SaveChanges();
            }
        }
    }
}