using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MauiSqlApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;
        private List<Student> _allStudents;
        private int _currentPage = 1;
        private const int PageSize = 5; // Number of students per page

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
            DisplayStudentsForPage(_currentPage);
        }

        private void DisplayStudentsForPage(int page)
        {
            var studentsToDisplay = _allStudents
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

        private async void OnDeleteStudentClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var student = button.CommandParameter as Student;

            if (student != null)
            {
                await _databaseService.DeleteStudentAsync(student);
                LoadStudents();
            }
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
            if (_currentPage < (_allStudents.Count + PageSize - 1) / PageSize)
            {
                _currentPage++;
                DisplayStudentsForPage(_currentPage);
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