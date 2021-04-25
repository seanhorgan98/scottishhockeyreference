using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Dapper;

namespace DataLibrary
{
    public class DataAccess : IDataAccess
    {
        public async Task<List<T>> LoadData<T, TU>(string sql, TU parameters, string connectionString)
        {
            using IDbConnection connection = new MySqlConnection(connectionString);

            var rows = await connection.QueryAsync<T>(sql, parameters);
            return rows.ToList();
        }

        public async Task<T> LoadDataSingle<T, TU>(string sql, TU parameters, string connectionString)
        {
            using IDbConnection connection = new MySqlConnection(connectionString);

            var row = await connection.QueryAsync<T>(sql, parameters);
            return row.Single<T>();
        }

        public Task SaveData<T>(string sql, T parameters, string connectionString)
        {
            using IDbConnection connection = new MySqlConnection(connectionString);
            return connection.ExecuteAsync(sql, parameters);
        }
    }
}