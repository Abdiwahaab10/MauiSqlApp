using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MauiSqlApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;
        private List<Student> _allStudents;
        private List<Student> _filteredStudents;
        private int _currentPage = 1;
        private const int PageSize = 5;

        public MainPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "students.db3");
            _databaseService = new DatabaseService(dbPath);
            LoadStudents();
        }

        private async void LoadStudents()
        {
            _allStudents = await _databaseService.GetStudentsAsync();
            _filteredStudents = _allStudents;
            DisplayStudentsForPage(_currentPage);
        }

        private void DisplayStudentsForPage(int page)
        {
            if (_filteredStudents == null)
            {
                StudentsCollectionView.ItemsSource = null;
                PageNumberLabel.Text = "Page 1";
                return;
            }

            var studentsToDisplay = _filteredStudents
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            StudentsCollectionView.ItemsSource = studentsToDisplay;
            PageNumberLabel.Text = $"Page {page}";
        }

        private async void OnAddStudentClicked(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                Student student = new Student
                {
                    Name = NameEntry.Text,
                    Age = int.Parse(AgeEntry.Text),
                    Email = EmailEntry.Text,
                    Course = CourseEntry.Text
                };

                await _databaseService.SaveStudentAsync(student);
                LoadStudents();

                ClearInputs();
            }
            else
            {
                await DisplayAlert("Error", "Please fill all fields correctly.", "OK");
            }
        }

        private async void OnEditStudentClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var student = button.CommandParameter as Student;

            if (student != null)
            {
                NameEntry.Text = student.Name;
                AgeEntry.Text = student.Age.ToString();
                EmailEntry.Text = student.Email;
                CourseEntry.Text = student.Course;

                await _databaseService.DeleteStudentAsync(student);
                LoadStudents();
            }
        }

        private async void OnDeleteStudentClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var student = button.CommandParameter as Student;

            if (student != null)
            {
                bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this student?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteStudentAsync(student);
                    LoadStudents();
                }
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allStudents == null)
                return;

            string searchText = SearchBar.Text?.ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(searchText))
            {
                _filteredStudents = _allStudents;
            }
            else
            {
                _filteredStudents = _allStudents
                    .Where(s => (s.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (s.Email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (s.Course?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }
            _currentPage = 1;
            DisplayStudentsForPage(_currentPage);
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayStudentsForPage(_currentPage);
            }
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            if (_filteredStudents == null)
                return;

            if (_currentPage < (_filteredStudents.Count + PageSize - 1) / PageSize)
            {
                _currentPage++;
                DisplayStudentsForPage(_currentPage);
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            if (_allStudents == null || _allStudents.Count == 0)
            {
                await DisplayAlert("Info", "No students to export.", "OK");
                return;
            }

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "students.csv");
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteLineAsync("Name,Age,Email,Course");
                foreach (var student in _allStudents)
                {
                    string name = EscapeCsvField(student.Name);
                    string email = EscapeCsvField(student.Email);
                    string course = EscapeCsvField(student.Course);
                    await writer.WriteLineAsync($"{name},{student.Age},{email},{course}");
                }
            }

            await DisplayAlert("Success", $"Data exported to {filePath}", "OK");
        }

        private static string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;
            
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            if (Application.Current.UserAppTheme == AppTheme.Light)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        private bool ValidateInputs()
        {
            return !string.IsNullOrWhiteSpace(NameEntry.Text) &&
                   int.TryParse(AgeEntry.Text, out int age) &&
                   !string.IsNullOrWhiteSpace(EmailEntry.Text) &&
                   !string.IsNullOrWhiteSpace(CourseEntry.Text);
        }

        private void ClearInputs()
        {
            NameEntry.Text = string.Empty;
            AgeEntry.Text = string.Empty;
            EmailEntry.Text = string.Empty;
            CourseEntry.Text = string.Empty;
        }
    }
}