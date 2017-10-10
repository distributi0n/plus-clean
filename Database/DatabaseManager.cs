namespace Plus.Database
{
    using System;
    using Core;
    using Interfaces;
    using MySql.Data.MySqlClient;

    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string ConnectionStr) => _connectionStr = ConnectionStr;

        public bool IsConnected()
        {
            try
            {
                var Con = new MySqlConnection(_connectionStr);
                Con.Open();
                var CMD = Con.CreateCommand();
                CMD.CommandText = "SELECT 1+1";
                CMD.ExecuteNonQuery();
                CMD.Dispose();
                Con.Close();
            }
            catch (MySqlException)
            {
                return false;
            }

            return true;
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient DbConnection = new DatabaseConnection(_connectionStr);
                DbConnection.connect();
                return DbConnection.GetQueryReactor();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                return null;
            }
        }
    }
}