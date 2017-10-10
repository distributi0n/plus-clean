namespace Plus.Database
{
    using System;
    using System.Data;
    using Adapter;
    using Interfaces;
    using MySql.Data.MySqlClient;

    public class DatabaseConnection : IDatabaseClient, IDisposable
    {
        private readonly IQueryAdapter _adapter;
        private readonly MySqlConnection _con;

        public DatabaseConnection(string ConnectionStr)
        {
            _con = new MySqlConnection(ConnectionStr);
            _adapter = new NormalQueryReactor(this);
        }

        public void connect()
        {
            if (_con.State == ConnectionState.Closed)
            {
                try
                {
                    _con.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void disconnect()
        {
            if (_con.State == ConnectionState.Open)
            {
                _con.Close();
            }
        }

        public IQueryAdapter GetQueryReactor() => _adapter;

        public void reportDone()
        {
            Dispose();
        }

        public MySqlCommand createNewCommand() => _con.CreateCommand();

        public void Dispose()
        {
            if (_con.State == ConnectionState.Open)
            {
                _con.Close();
            }
            _con.Dispose();
            GC.SuppressFinalize(this);
        }

        public void prepare()
        {
            // nothing here
        }
    }
}