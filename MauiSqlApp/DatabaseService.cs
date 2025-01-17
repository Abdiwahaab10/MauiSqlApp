using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiSqlApp
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Student>().Wait();
        }

        public Task<List<Student>> GetStudentsAsync()
        {
            return _database.Table<Student>().ToListAsync();
        }

        public Task<int> SaveStudentAsync(Student student)
        {
            return _database.InsertAsync(student);
        }

        public Task<int> UpdateStudentAsync(Student student)
        {
            return _database.UpdateAsync(student);
        }

        public Task<int> DeleteStudentAsync(Student student)
        {
            return _database.DeleteAsync(student);
        }
    }
}