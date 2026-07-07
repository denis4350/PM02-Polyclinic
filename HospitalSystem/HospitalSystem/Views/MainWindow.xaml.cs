using HospitalSystem.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalSystem.Views
{
    public partial class MainWindow : Window
    {
        private class UpcomingAppointment
        {
            public int AppointmentID { get; set; }
            public string WhenDisplay { get; set; }
            public string PatientName { get; set; }
            public string DoctorName { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            ApplyRoleAccess();
            ShowCurrentUser();
            LoadDashboard();

            // Обновляем сводку каждый раз, когда окно снова становится активным —
            // например, вернулись сюда, закрыв окно "Пациенты" после добавления записи
            this.Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        private void ShowCurrentUser()
        {
            var user = SessionManager.CurrentUser;
            UserNameText.Text = user?.FullName ?? "Пользователь";
            UserRoleText.Text = SessionManager.Role ?? "";
        }

        private void LoadDashboard()
        {
            using (var db = new HospitalDBEntities())
            {
                PatientsCountText.Text = db.Patients.Count().ToString();
                DoctorsCountText.Text = db.Doctors.Count().ToString();
                TotalAppointmentsCountText.Text = db.Appointments.Count().ToString();

                DateTime today = DateTime.Today;
                TodayCountText.Text = db.Appointments.Count(a => a.AppointmentDate == today).ToString();

                var upcoming = db.Appointments
                    .Include(a => a.Patients)
                    .Include(a => a.Doctors)
                    .Where(a => a.AppointmentDate >= today)
                    .ToList()
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Take(5)
.Select(a => new UpcomingAppointment
{
    AppointmentID = a.AppointmentID,
    WhenDisplay = a.AppointmentDate.ToString("dd.MM") + ", " + a.AppointmentTime.ToString(@"hh\:mm"),
    PatientName = NameHelper.FullName(a.Patients.LastName, a.Patients.FirstName),
    DoctorName = NameHelper.FullName(a.Doctors.LastName, a.Doctors.FirstName)
})
                    .ToList();

                UpcomingList.ItemsSource = upcoming;
                NoUpcomingText.Visibility = upcoming.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpcomingRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int appointmentId)
            {
                new AppointmentsWindow(appointmentId).Show();
            }
        }

        private void Patients_Click(object sender, RoutedEventArgs e)
        {
            new PatientsWindow().Show();
        }

        private void Doctors_Click(object sender, RoutedEventArgs e)
        {
            new DoctorsWindow().Show();
        }

        private void Appointments_Click(object sender, RoutedEventArgs e)
        {
            new AppointmentsWindow().Show();
        }

        private void Specialties_Click(object sender, RoutedEventArgs e)
        {
            new SpecialtiesWindow().Show();
        }

        private void ApiLogs_Click(object sender, RoutedEventArgs e)
        {
            new ApiLogsWindow().Show();
        }
        private void AuditLogs_Click(object sender, RoutedEventArgs e)
        {
            new AuditLogsWindow().Show();
        }

        private void Api_Click(object sender, RoutedEventArgs e)
        {
            new ApiWindow().Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AuditHelper.Log("Выход из системы", "Users");
            SessionManager.CurrentUser = null;
            new LoginWindow().Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ApplyRoleAccess()
        {
            string role = SessionManager.Role;

            if (role == "Врач")
            {
                PatientsBtn.IsEnabled = false;
                DoctorsBtn.IsEnabled = false;
                PatientsTile.IsEnabled = false;
                DoctorsTile.IsEnabled = false;
            }

            if (role == "Регистратор")
            {
                DoctorsBtn.IsEnabled = false;
                DoctorsTile.IsEnabled = false;
            }

            // Журналы — только администратору: там видно, кто и что делал во всей системе
            if (role != "Администратор")
            {
                ApiLogsBtn.IsEnabled = false;
                AuditLogsBtn.IsEnabled = false;
            }
        }
    }
    }
