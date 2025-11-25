using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiSqlApp
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;
        private bool _isInitialized;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public DatabaseService(string dbPath)
        {
            _dbPath = dbPath;
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                _database = new SQLiteAsyncConnection(_dbPath);
                await _database.CreateTableAsync<Student>();
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            await InitializeAsync();
            return await _database.Table<Student>().ToListAsync();
        }

        public async Task<int> SaveStudentAsync(Student student)
        {
            await InitializeAsync();
            return await _database.InsertAsync(student);
        }

        public async Task<int> UpdateStudentAsync(Student student)
        {
            await InitializeAsync();
            return await _database.UpdateAsync(student);
        }

        public async Task<int> DeleteStudentAsync(Student student)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(student);
        }
    }
}