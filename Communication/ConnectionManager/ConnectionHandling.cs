namespace Plus.Communication.ConnectionManager
{
    using System;
    using Core;

    public class ConnectionHandling
    {
        private readonly SocketManager manager;

        public ConnectionHandling(int port, int maxConnections, int connectionsPerIP, bool enabeNagles)
        {
            manager = new SocketManager();
            manager.init(port, maxConnections, connectionsPerIP, new InitialPacketParser(), !enabeNagles);
        }

        public void init()
        {
            manager.connectionEvent += manager_connectionEvent;
            manager.initializeConnectionRequests();
        }

        private void manager_connectionEvent(ConnectionInformation connection)
        {
            connection.connectionChanged += ConnectionChanged;
            PlusEnvironment.GetGame().GetClientManager()
                .CreateAndStartClient(Convert.ToInt32(connection.getConnectionID()), connection);
        }

        private void ConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.CLOSED)
            {
                CloseConnection(information);
            }
        }

        private void CloseConnection(ConnectionInformation connection)
        {
            try
            {
                connection.Dispose();
                PlusEnvironment.GetGame().GetClientManager().DisposeConnection(Convert.ToInt32(connection.getConnectionID()));
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        internal void Destroy()
        {
            manager.destroy();
        }
    }
}