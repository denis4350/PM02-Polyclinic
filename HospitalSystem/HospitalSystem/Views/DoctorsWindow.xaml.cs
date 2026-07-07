using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using HospitalSystem.Helpers;

namespace HospitalSystem.Views
{
    public partial class DoctorsWindow : Window
    {
        private int? _editingDoctorId = null;
        private static readonly Random _random = new Random();
        private List<Doctors> _allDoctors = new List<Doctors>();

        public DoctorsWindow()
        {
            InitializeComponent();
            LoadSpecialties();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            using (var db = new HospitalDBEntities())
            {
                _allDoctors = db.Doctors
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToList();
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchBox.Text?.Trim() ?? "";

            List<Doctors> filtered = string.IsNullOrEmpty(query)
                ? _allDoctors
                : _allDoctors.Where(d =>
                    NameHelper.FullName(d.LastName, d.FirstName, d.MiddleName)
                        .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            DoctorsGrid.ItemsSource = filtered;
            NoResultsText.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void LoadSpecialties()
        {
            using (var db = new HospitalDBEntities())
            {
                SpecialtyBox.ItemsSource = db.Specialties.OrderBy(s => s.SpecialtyName).ToList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) || string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Заполните фамилию и имя врача");
                return;
            }

            if (SpecialtyBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите специальность");
                return;
            }

            if (_editingDoctorId.HasValue)
                UpdateDoctor();
            else
                CreateDoctorWithLogin();
        }

        private void UpdateDoctor()
        {
            try
            {
                using (var db = new HospitalDBEntities())
                {
                    var doctor = db.Doctors.Find(_editingDoctorId.Value);
                    if (doctor != null)
                    {
                        doctor.LastName = LastNameBox.Text.Trim();
                        doctor.FirstName = FirstNameBox.Text.Trim();
                        doctor.MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim();
                        doctor.Office = OfficeBox.Text.Trim();
                        doctor.Phone = PhoneBox.Text.Trim();
                        doctor.SpecialtyID = (int)SpecialtyBox.SelectedValue;

                        db.SaveChanges();
                    }
                }

                AuditHelper.Log("Изменение врача", "Doctors");
                MessageBox.Show("Изменения сохранены!");
                ResetForm();
                LoadDoctors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить изменения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDoctorWithLogin()
        {
            using (var db = new HospitalDBEntities())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var doctorRole = db.Roles.FirstOrDefault(r => r.RoleName == "Врач");
                    if (doctorRole == null)
                    {
                        MessageBox.Show("В базе данных не найдена роль «Врач». Добавьте её в таблицу Roles, прежде чем создавать врачей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string login = GenerateDoctorLogin(db);
                    string tempPassword = GenerateTempPassword();

                    var newUser = new Users
                    {
                        Login = login,
                        PasswordHash = PasswordHelper.Hash(tempPassword),
                        FullName = NameHelper.FullName(LastNameBox.Text.Trim(), FirstNameBox.Text.Trim(), MiddleNameBox.Text.Trim()),
                        RoleID = doctorRole.RoleID
                    };
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    var doctor = new Doctors
                    {
                        LastName = LastNameBox.Text.Trim(),
                        FirstName = FirstNameBox.Text.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim(),
                        Office = OfficeBox.Text.Trim(),
                        Phone = PhoneBox.Text.Trim(),
                        SpecialtyID = (int)SpecialtyBox.SelectedValue,
                        UserID = newUser.UserID
                    };
                    db.Doctors.Add(doctor);
                    db.SaveChanges();

                    transaction.Commit();
                    AuditHelper.Log("Добавление врача", "Doctors");

                    MessageBox.Show(
                        $"Врач добавлен.\n\nЛогин: {login}\nВременный пароль: {tempPassword}\n\nСообщите эти данные врачу — под этой учётной записью он будет входить в систему.",
                        "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                    ResetForm();
                    LoadDoctors();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Не удалось добавить врача: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GenerateDoctorLogin(HospitalDBEntities db)
        {
            int n = db.Users.Count() + 1;
            string login = "doctor" + n;

            while (db.Users.Any(u => u.Login == login))
            {
                n++;
                login = "doctor" + n;
            }

            return login;
        }

        private string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var result = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                result.Append(chars[_random.Next(chars.Length)]);
            }
            return result.ToString();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DoctorsGrid.SelectedItem as Doctors;

            if (selected == null)
            {
                MessageBox.Show("Выберите врача!");
                return;
            }

            if (!DialogHelper.Confirm($"Удалить врача {NameHelper.FullName(selected.LastName, selected.FirstName)}?\n\nУчётная запись для входа останется в системе."))
                return;

            try
            {
                using (var db = new HospitalDBEntities())
                {
                    var doc = db.Doctors.Find(selected.DoctorID);
                    if (doc != null)
                    {
                        db.Doctors.Remove(doc);
                        db.SaveChanges();
                    }
                }

                AuditHelper.Log("Удаление врача", "Doctors");
                MessageBox.Show("Удалено!");
                ResetForm();
                LoadDoctors();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось удалить врача — скорее всего, на него уже есть записи на приём. Сначала удалите или переназначьте эти записи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
            LoadDoctors();
        }

        private void NewDoctor_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void DoctorsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DoctorsGrid.SelectedItem as Doctors;
            if (selected == null) return;

            _editingDoctorId = selected.DoctorID;
            LastNameBox.Text = selected.LastName;
            FirstNameBox.Text = selected.FirstName;
            MiddleNameBox.Text = selected.MiddleName;
            OfficeBox.Text = selected.Office;
            PhoneBox.Text = selected.Phone;
            SpecialtyBox.SelectedValue = selected.SpecialtyID;

            SaveButton.Content = "Сохранить изменения";
        }

        private void ResetForm()
        {
            _editingDoctorId = null;
            DoctorsGrid.SelectedItem = null;
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            MiddleNameBox.Text = "";
            OfficeBox.Text = "";
            PhoneBox.Text = "";
            SpecialtyBox.SelectedIndex = -1;
            SaveButton.Content = "Добавить врача";
        }
    }
}