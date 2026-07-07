using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace HospitalSystem.Views
{
    public partial class ApiLogsWindow : Window
    {
        private class ApiLogRow
        {
            public int LogID { get; set; }
            public string UserDisplay { get; set; }
            public DateTime? RequestDate { get; set; }
            public string Endpoint { get; set; }
            public string StatusDisplay { get; set; }
            public bool IsSuccess { get; set; }
            public string Response { get; set; }
        }

        public ApiLogsWindow()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            using (var db = new HospitalDBEntities())
            {
                var rows = db.ApiLogs
                    .Include(l => l.Users)
                    .ToList()
                    .Select(l => new ApiLogRow
                    {
                        LogID = l.LogID,
                        UserDisplay = l.Users != null ? $"{l.Users.FullName} ({l.Users.Login})" : "—",
                        RequestDate = l.RequestDate,
                        Endpoint = l.Endpoint,
                        StatusDisplay = l.StatusCode == 200 ? "200 OK" : (l.StatusCode.HasValue ? $"Ошибка ({l.StatusCode})" : "—"),
                        IsSuccess = l.StatusCode == 200,
                        Response = l.Response
                    })
                    .OrderByDescending(l => l.RequestDate)
                    .ToList();

                ApiLogsGrid.ItemsSource = rows;

                int total = rows.Count;
                int success = rows.Count(r => r.IsSuccess);
                int failed = total - success;

                SummaryText.Text = total == 0
                    ? "Записей пока нет — сделайте запрос в окне «API»"
                    : $"Всего запросов: {total}   ·   успешных: {success}   ·   с ошибкой: {failed}";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}