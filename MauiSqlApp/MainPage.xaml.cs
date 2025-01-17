using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MauiSqlApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "people.db3");
            _databaseService = new DatabaseService(dbPath);
            LoadPeople();
        }

        private async void LoadPeople()
        {
            List<Person> people = await _databaseService.GetPeopleAsync();
            PeopleListView.ItemsSource = people;
        }

        private async void OnAddPersonClicked(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameEntry.Text) && !string.IsNullOrWhiteSpace(AgeEntry.Text))
            {
                Person person = new Person
                {
                    Name = NameEntry.Text,
                    Age = int.Parse(AgeEntry.Text)
                };

                await _databaseService.SavePersonAsync(person);
                LoadPeople();

                NameEntry.Text = string.Empty;
                AgeEntry.Text = string.Empty;
            }
        }
    }
}