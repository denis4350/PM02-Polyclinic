using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace HospitalSystem.Views
{
    public partial class AuditLogsWindow : Window
    {
        private class AuditLogRow
        {
            public int AuditID { get; set; }
            public string UserDisplay { get; set; }
            public DateTime? ActionDate { get; set; }
            public string Action { get; set; }
            public string TableName { get; set; }
            public string ActionCategory { get; set; }
        }

        public AuditLogsWindow()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            using (var db = new HospitalDBEntities())
            {
                var rows = db.AuditLogs
                    .Include(a => a.Users)
                    .ToList()
                    .Select(a => new AuditLogRow
                    {
                        AuditID = a.AuditID,
                        UserDisplay = a.Users != null ? $"{a.Users.FullName} ({a.Users.Login})" : "—",
                        ActionDate = a.ActionDate,
                        Action = a.Action,
                        TableName = a.TableName,
                        ActionCategory = GetActionCategory(a.Action)
                    })
                    .OrderByDescending(a => a.ActionDate)
                    .ToList();

                AuditLogsGrid.ItemsSource = rows;

                int total = rows.Count;
                int today = rows.Count(r => r.ActionDate.HasValue && r.ActionDate.Value.Date == DateTime.Today);
                int users = rows.Select(r => r.UserDisplay).Distinct().Count();

                SummaryText.Text = total == 0
                    ? "Записей пока нет — они появляются автоматически при работе с пациентами, врачами, записями и специальностями"
                    : $"Всего действий: {total}   ·   за сегодня: {today}   ·   пользователей: {users}";
            }
        }

        private string GetActionCategory(string action)
        {
            if (string.IsNullOrEmpty(action)) return "Other";
            if (action.StartsWith("Добавление")) return "Add";
            if (action.StartsWith("Изменение")) return "Edit";
            if (action.StartsWith("Удаление")) return "Delete";
            if (action.StartsWith("Неудачная")) return "Warning";
            return "Other";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}