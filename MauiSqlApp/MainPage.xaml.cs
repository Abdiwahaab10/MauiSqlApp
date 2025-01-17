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
            string searchText = SearchBar.Text.ToLower();
            _filteredStudents = _allStudents
                .Where(s => s.Name.ToLower().Contains(searchText) || s.Email.ToLower().Contains(searchText) || s.Course.ToLower().Contains(searchText))
                .ToList();
            DisplayStudentsForPage(1);
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
            if (_currentPage < (_filteredStudents.Count + PageSize - 1) / PageSize)
            {
                _currentPage++;
                DisplayStudentsForPage(_currentPage);
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "students.csv");
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Name,Age,Email,Course");
                foreach (var student in _allStudents)
                {
                    writer.WriteLine($"{student.Name},{student.Age},{student.Email},{student.Course}");
                }
            }

            await DisplayAlert("Success", $"Data exported to {filePath}", "OK");
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