namespace Plus.Database.Adapter
{
    using System;
    using Interfaces;

    public class NormalQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
    {
        public NormalQueryReactor(IDatabaseClient Client) : base(Client) => command = Client.createNewCommand();

        public void Dispose()
        {
            command.Dispose();
            client.reportDone();
            GC.SuppressFinalize(this);
        }
    }
}