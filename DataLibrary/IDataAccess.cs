using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLibrary
{
    public interface IDataAccess
    {
        Task<List<T>> LoadData<T, TU>(string sql, TU parameters, string connectionString);
        Task<T> LoadDataSingle<T, TU>(string sql, TU parameters, string connectionString);
        Task SaveData<T>(string sql, T parameters, string connectionString);
    }
}