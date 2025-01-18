using WebApplication2.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace WebApplication2.Data
{
    public class DapperRepository<T> : IDapperRepository<T>
    {
        IConfiguration _configuration;
        SqlConnection _connection;
        public DapperRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            _connection = new SqlConnection(connectionString);
        }

        public T GetById(int id)
        {
            string tableName = typeof(T).Name;
            string sql = $"SELECT * FROM [dbo].[{tableName}] WHERE Id = @Id";
            return _connection.QuerySingleOrDefault<T>(sql, new { Id = id });
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public IEnumerable<T> GetAll()
        {
            return _connection.Query<T>($"SELECT * FROM [dbo].[{typeof(T).Name}]");
        }

        public int? Insert(T entity, bool identityFromDB)
        {
            //// Pobranie właściwości klasy T i utworzenie dynamicznego zapytania SQL
            var properties = typeof(T).GetProperties().Select(p => p.Name);
            var output = "";

            if (identityFromDB)
            {
                properties = properties.Where(p => !p.ToLowerInvariant().Contains("id")); // Wykluczenie kolumny Id z listy kolumn do wstawienia
                output = "OUTPUT INSERTED.Id";
            }

            var columnNames = string.Join(", ", properties); // np. "Name, Age, Grade"
            var parameterNames = string.Join(", ", properties.Select(p => "@" + p)); // np. "@Name, @Age, @Grade"

            var tableName = typeof(T).Name; // Zakładamy, że nazwa klasy odpowiada nazwie tabeli

            var sql = $"INSERT INTO [dbo].[{tableName}] ({columnNames}) {output} VALUES ({parameterNames});";
            Console.WriteLine(sql);

            if (identityFromDB)
                return _connection.QuerySingle<int>(sql, entity); // Wykonanie zapytania
            else
                return _connection.Execute(sql, entity); // Wykonanie zapytania

        }
        public int Update(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != "Id")
                .Select(p => p.Name);
            var setClause = string.Join(", ", properties.Select(p => $"{p} = @{p}"));
            var tableName = typeof(T).Name;
            var sql = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";
            return _connection.Execute(sql, entity);
        }
        public int Delete(int id)
        {
            var tableName = typeof(T).Name;
            var sql = $"DELETE FROM {tableName} WHERE Id = @Id";
            return _connection.Execute(sql, new { Id = id });
        }

        public IEnumerable<string> GetRoles(int userId)
        {
            var sql = $"SELECT r.Name FROM [dbo].[ROLEs] r JOIN [dbo].[USERROLES] ur ON ur.Role_Id = r.Id WHERE ur.User_Id = @UserId";
            return _connection.Query<string>(sql, new { UserId = userId });
        }

        public void SaveSecretKey(int userId, string secretKey)
        {
            var sql = "UPDATE [dbo].[USER] SET SecretKey = @SecretKey WHERE Id = @UserId";
            _connection.Execute(sql, new { UserId = userId, SecretKey = secretKey });
        }

        public T GetUserByEmail(string email)
        {
            var sql = "SELECT * FROM [dbo].[USER] WHERE Email = @Email";
            return _connection.QuerySingleOrDefault<T>(sql, new { Email = email });
        }
    }
}
