namespace Plus.Database.Interfaces
{
    using System;
    using MySql.Data.MySqlClient;

    public interface IDatabaseClient : IDisposable
    {
        void connect();
        void disconnect();
        IQueryAdapter GetQueryReactor();
        MySqlCommand createNewCommand();
        void reportDone();
    }
}