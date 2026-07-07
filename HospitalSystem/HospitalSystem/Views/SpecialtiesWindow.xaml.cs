using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HospitalSystem.Helpers;

namespace HospitalSystem.Views
{
    public partial class SpecialtiesWindow : Window
    {
        private int? _editingSpecialtyId = null;

        private class SpecialtyRow
        {
            public int SpecialtyID { get; set; }
            public string SpecialtyName { get; set; }
            public int DoctorsCount { get; set; }
        }

        public SpecialtiesWindow()
        {
            InitializeComponent();
            LoadSpecialties();
        }

        private void LoadSpecialties()
        {
            using (var db = new HospitalDBEntities())
            {
                var rows = db.Specialties
                    .Include(s => s.Doctors)
                    .ToList()
                    .Select(s => new SpecialtyRow
                    {
                        SpecialtyID = s.SpecialtyID,
                        SpecialtyName = s.SpecialtyName,
                        DoctorsCount = s.Doctors.Count
                    })
                    .OrderBy(s => s.SpecialtyName)
                    .ToList();

                SpecialtiesGrid.ItemsSource = rows;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название специальности");
                return;
            }

            using (var db = new HospitalDBEntities())
            {
                bool duplicate = db.Specialties.Any(s =>
                    s.SpecialtyName == name &&
                    (!_editingSpecialtyId.HasValue || s.SpecialtyID != _editingSpecialtyId.Value));

                if (duplicate)
                {
                    MessageBox.Show("Такая специальность уже есть в списке");
                    return;
                }

                try
                {
                    if (_editingSpecialtyId.HasValue)
                    {
                        var specialty = db.Specialties.Find(_editingSpecialtyId.Value);
                        if (specialty != null)
                        {
                            specialty.SpecialtyName = name;
                        }
                    }
                    else
                    {
                        db.Specialties.Add(new Specialties { SpecialtyName = name });
                    }

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось сохранить специальность: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            AuditHelper.Log(_editingSpecialtyId.HasValue ? "Изменение специальности" : "Добавление специальности", "Specialties");
            MessageBox.Show(_editingSpecialtyId.HasValue ? "Изменения сохранены!" : "Специальность добавлена!");
            ResetForm();
            LoadSpecialties();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = SpecialtiesGrid.SelectedItem as SpecialtyRow;

            if (selected == null)
            {
                MessageBox.Show("Выберите специальность!");
                return;
            }

            if (!DialogHelper.Confirm($"Удалить специальность «{selected.SpecialtyName}»?"))
                return;

            try
            {
                using (var db = new HospitalDBEntities())
                {
                    var specialty = db.Specialties.Find(selected.SpecialtyID);
                    if (specialty != null)
                    {
                        db.Specialties.Remove(specialty);
                        db.SaveChanges();
                    }
                }

                AuditHelper.Log("Удаление специальности", "Specialties");
                MessageBox.Show("Удалено!");
                ResetForm();
                LoadSpecialties();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось удалить — на эту специальность ссылаются врачи. Сначала измените специальность этим врачам или удалите их.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
            LoadSpecialties();
        }

        private void NewSpecialty_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void SpecialtiesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = SpecialtiesGrid.SelectedItem as SpecialtyRow;
            if (selected == null) return;

            _editingSpecialtyId = selected.SpecialtyID;
            NameBox.Text = selected.SpecialtyName;
            SaveButton.Content = "Сохранить изменения";
        }

        private void ResetForm()
        {
            _editingSpecialtyId = null;
            SpecialtiesGrid.SelectedItem = null;
            NameBox.Text = "";
            SaveButton.Content = "Добавить специальность";
        }
    }
}