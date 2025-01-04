namespace WebApplication2.Data
{
    public interface IDapperRepository<T> : IDisposable
    {
        int? Insert(T item, bool withIdentity);
        int Update(T item);
        int Delete(int item);
        IEnumerable<T> GetAll();
        T GetById(int id);
		IEnumerable<string> GetRoles(int userId);
        void SaveSecretKey(int id, string userKey);
    }
}
