namespace Plus.Communication.ConnectionManager
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using log4net;
    using Plus.Communication.ConnectionManager.Socket_Exceptions;

    public class SocketManager
    {
        public delegate void ConnectionEvent(ConnectionInformation connection);
        private static readonly ILog log = LogManager.GetLogger("Plus.Communication.ConnectionManager");

        private bool _acceptConnections;

        private int _acceptedConnections;

        //private Dictionary<string, int> ipConnectionCount;
        private ConcurrentDictionary<string, int> _ipConnectionsCount;

        private Socket connectionListener;

        private bool disableNagleAlgorithm;

        private int maximumConnections;

        private int maxIpConnectionCount;

        private IDataParser parser;

        private int portInformation;

        public event ConnectionEvent connectionEvent;

        public void init(int portID, int maxConnections, int connectionsPerIP, IDataParser parser, bool disableNaglesAlgorithm)
        {
            _ipConnectionsCount = new ConcurrentDictionary<string, int>();
            this.parser = parser;
            disableNagleAlgorithm = disableNaglesAlgorithm;
            maximumConnections = maxConnections;
            portInformation = portID;
            maxIpConnectionCount = connectionsPerIP;
            prepareConnectionDetails();
            _acceptedConnections = 0;
            log.Info("Successfully setup GameSocketManager on port (" + portID + ")!");
            log.Info("Maximum connections per IP has been set to [" + connectionsPerIP + "]!");
        }

        private void prepareConnectionDetails()
        {
            connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectionListener.NoDelay = disableNagleAlgorithm;
            try
            {
                connectionListener.Bind(new IPEndPoint(IPAddress.Any, portInformation));
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        public void initializeConnectionRequests()
        {
            //Out.writeLine("Starting to listen to connection requests", Out.logFlags.ImportantLogLevel);
            connectionListener.Listen(100);
            _acceptConnections = true;
            try
            {
                connectionListener.BeginAccept(newConnectionRequest, connectionListener);
            }
            catch
            {
                destroy();
            }
        }

        public void destroy()
        {
            _acceptConnections = false;
            try
            {
                connectionListener.Close();
            }
            catch
            {
            }
            connectionListener = null;
        }

        private void newConnectionRequest(IAsyncResult iAr)
        {
            if (connectionListener != null)
            {
                if (_acceptConnections)
                {
                    try
                    {
                        var replyFromComputer = ((Socket) iAr.AsyncState).EndAccept(iAr);
                        replyFromComputer.NoDelay = disableNagleAlgorithm;
                        var Ip = replyFromComputer.RemoteEndPoint.ToString().Split(':')[0];
                        var ConnectionCount = getAmountOfConnectionFromIp(Ip);
                        if (ConnectionCount < maxIpConnectionCount)
                        {
                            _acceptedConnections++;
                            var c = new ConnectionInformation(_acceptedConnections, replyFromComputer, this, parser.Clone() as IDataParser, Ip);
                            reportUserLogin(Ip);
                            c.connectionChanged += c_connectionChanged;
                            if (connectionEvent != null)
                            {
                                connectionEvent(c);
                            }
                        }
                        else
                        {
                            log.Info("Connection denied from [" +
                                     replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] +
                                     "]. Too many connections (" +
                                     ConnectionCount +
                                     ").");
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        connectionListener.BeginAccept(newConnectionRequest, connectionListener);
                    }
                }
            }
        }

        private void c_connectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.CLOSED)
            {
                reportDisconnect(information);
            }
        }

        public void reportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.connectionChanged -= c_connectionChanged;
            reportUserLogout(gameConnection.getIp());
            //activeConnections.Remove(gameConnection.getConnectionID());
        }

        private void reportUserLogin(string ip)
        {
            alterIpConnectionCount(ip, getAmountOfConnectionFromIp(ip) + 1);
        }

        private void reportUserLogout(string ip)
        {
            alterIpConnectionCount(ip, getAmountOfConnectionFromIp(ip) - 1);
        }

        private void alterIpConnectionCount(string ip, int amount)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                int am;
                _ipConnectionsCount.TryRemove(ip, out am);
            }
            _ipConnectionsCount.TryAdd(ip, amount);
        }

        private int getAmountOfConnectionFromIp(string ip)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                return _ipConnectionsCount[ip];
            }
            return 0;
        }
    }
}