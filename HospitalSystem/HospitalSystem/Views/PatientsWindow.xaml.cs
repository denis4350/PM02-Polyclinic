using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HospitalSystem.Helpers;

namespace HospitalSystem.Views
{
    public partial class PatientsWindow : Window
    {
        private int? _editingPatientId = null;
        private List<Patients> _allPatients = new List<Patients>();

        public PatientsWindow()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void LoadPatients()
        {
            using (var db = new HospitalDBEntities())
            {
                _allPatients = db.Patients
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToList();
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchBox.Text?.Trim() ?? "";

            List<Patients> filtered = string.IsNullOrEmpty(query)
                ? _allPatients
                : _allPatients.Where(p =>
                    NameHelper.FullName(p.LastName, p.FirstName, p.MiddleName)
                        .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            PatientsGrid.ItemsSource = filtered;
            NoResultsText.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) || string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Заполните фамилию и имя пациента");
                return;
            }

            if (BirthDateBox.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату рождения");
                return;
            }

            string gender = (GenderBox.SelectedItem as ComboBoxItem)?.Content as string;

            try
            {
                using (var db = new HospitalDBEntities())
                {
                    if (_editingPatientId.HasValue)
                    {
                        var patient = db.Patients.Find(_editingPatientId.Value);
                        if (patient != null)
                        {
                            patient.LastName = LastNameBox.Text.Trim();
                            patient.FirstName = FirstNameBox.Text.Trim();
                            patient.MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim();
                            patient.BirthDate = BirthDateBox.SelectedDate.Value;
                            patient.Gender = gender;
                            patient.Phone = PhoneBox.Text.Trim();
                            patient.Address = AddressBox.Text.Trim();
                            patient.PolicyNumber = PolicyBox.Text.Trim();
                        }
                    }
                    else
                    {
                        db.Patients.Add(new Patients
                        {
                            LastName = LastNameBox.Text.Trim(),
                            FirstName = FirstNameBox.Text.Trim(),
                            MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim(),
                            BirthDate = BirthDateBox.SelectedDate.Value,
                            Gender = gender,
                            Phone = PhoneBox.Text.Trim(),
                            Address = AddressBox.Text.Trim(),
                            PolicyNumber = PolicyBox.Text.Trim()
                        });
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить пациента: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AuditHelper.Log(_editingPatientId.HasValue ? "Изменение пациента" : "Добавление пациента", "Patients");
            MessageBox.Show(_editingPatientId.HasValue ? "Изменения сохранены!" : "Пациент добавлен!");
            ResetForm();
            LoadPatients();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = PatientsGrid.SelectedItem as Patients;

            if (selected == null)
            {
                MessageBox.Show("Выберите пациента!");
                return;
            }

            if (!DialogHelper.Confirm($"Удалить пациента {NameHelper.FullName(selected.LastName, selected.FirstName)}?"))
                return;

            try
            {
                using (var db = new HospitalDBEntities())
                {
                    var patient = db.Patients.Find(selected.PatientID);
                    if (patient != null)
                    {
                        db.Patients.Remove(patient);
                        db.SaveChanges();
                    }
                }

                AuditHelper.Log("Удаление пациента", "Patients");
                MessageBox.Show("Удалено!");
                ResetForm();
                LoadPatients();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось удалить пациента — скорее всего, у него уже есть записи на приём. Сначала удалите эти записи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
            LoadPatients();
        }

        private void NewPatient_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void PatientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = PatientsGrid.SelectedItem as Patients;
            if (selected == null) return;

            _editingPatientId = selected.PatientID;
            LastNameBox.Text = selected.LastName;
            FirstNameBox.Text = selected.FirstName;
            MiddleNameBox.Text = selected.MiddleName;
            BirthDateBox.SelectedDate = selected.BirthDate;
            PhoneBox.Text = selected.Phone;
            AddressBox.Text = selected.Address;
            PolicyBox.Text = selected.PolicyNumber;

            GenderBox.SelectedIndex = -1;
            foreach (ComboBoxItem item in GenderBox.Items)
            {
                string optionText = (item.Content as string)?.Trim();
                string storedGender = selected.Gender?.Trim();

                if (string.Equals(optionText, storedGender, StringComparison.OrdinalIgnoreCase))
                {
                    GenderBox.SelectedItem = item;
                    break;
                }
            }

            SaveButton.Content = "Сохранить изменения";
        }

        private void ResetForm()
        {
            _editingPatientId = null;
            PatientsGrid.SelectedItem = null;
            LastNameBox.Text = "";
            FirstNameBox.Text = "";
            MiddleNameBox.Text = "";
            BirthDateBox.SelectedDate = null;
            GenderBox.SelectedIndex = -1;
            PhoneBox.Text = "";
            AddressBox.Text = "";
            PolicyBox.Text = "";
            SaveButton.Content = "Добавить пациента";
        }
    }
}