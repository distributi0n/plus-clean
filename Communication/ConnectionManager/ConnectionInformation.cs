namespace Plus.Communication.ConnectionManager
{
    using System;
    using System.Net.Sockets;

    public sealed class ConnectionInformation : IDisposable
    {
        public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);

        private static readonly bool disableSend = false;
        private static readonly bool disableReceive = false;

        private readonly byte[] buffer;

        private readonly int connectionID;

        private readonly Socket dataSocket;

        private readonly string ip;

        private readonly AsyncCallback sendCallback;

        private bool isConnected;

        private SocketManager _manager;

        public ConnectionInformation(int connectionID, Socket dataStream, SocketManager manager, IDataParser parser, string ip)
        {
            this.parser = parser;
            buffer = new byte[GameSocketManagerStatics.BUFFER_SIZE];
            _manager = manager;
            dataSocket = dataStream;
            dataSocket.SendBufferSize = GameSocketManagerStatics.BUFFER_SIZE;
            this.ip = ip;
            sendCallback = SentData;
            this.connectionID = connectionID;

            if (connectionChanged != null)
            {
                connectionChanged.Invoke(this, ConnectionState.OPEN);
            }
        }

        public IDataParser parser { get; set; }

        public void Dispose()
        {
            if (isConnected)
            {
                disconnect();
            }
            GC.SuppressFinalize(this);
        }

        public event ConnectionChange connectionChanged;

        public void startPacketProcessing()
        {
            if (!isConnected)
            {
                isConnected = true;
                try
                {
                    dataSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, IncomingDataPacket, dataSocket);
                }
                catch
                {
                    disconnect();
                }
            }
        }

        public string GetIp() => ip;

        public int getConnectionID() => connectionID;

        public void disconnect()
        {
            try
            {
                if (isConnected)
                {
                    isConnected = false;
                    try
                    {
                        if (dataSocket != null && dataSocket.Connected)
                        {
                            dataSocket.Shutdown(SocketShutdown.Both);
                            dataSocket.Close();
                        }
                    }
                    catch
                    {
                    }
                    dataSocket.Dispose();
                    parser.Dispose();
                    try
                    {
                        if (connectionChanged != null)
                        {
                            connectionChanged.Invoke(this, ConnectionState.CLOSED);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    connectionChanged = null;
                }
            }
            catch
            {
            }
        }

        private void IncomingDataPacket(IAsyncResult iAr)
        {
            int bytesReceived;
            try
            {
                //The amount of bytes received in the packet
                bytesReceived = dataSocket.EndReceive(iAr);
            }
            catch //(Exception e)
            {
                disconnect();
                return;
            }

            if (bytesReceived == 0)
            {
                disconnect();
                return;
            }

            try
            {
                if (!disableReceive)
                {
                    var packet = new byte[bytesReceived];
                    Array.Copy(buffer, packet, bytesReceived);
                    HandlePacketData(packet);
                }
            }
            catch //(Exception e)
            {
                disconnect();
            }
            finally
            {
                try
                {
                    //and we keep looking for the next packet
                    dataSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, IncomingDataPacket, dataSocket);
                }
                catch //(Exception e)
                {
                    disconnect();
                }
            }
        }

        private void HandlePacketData(byte[] packet)
        {
            parser?.handlePacketData(packet);
        }

        internal void SendData(byte[] packet)
        {
            try
            {
                if (!isConnected || disableSend)
                {
                    return;
                }

                dataSocket.BeginSend(packet, 0, packet.Length, 0, sendCallback, null);
            }
            catch
            {
                disconnect();
            }
        }

        private void SentData(IAsyncResult iAr)
        {
            try
            {
                dataSocket.EndSend(iAr);
            }
            catch
            {
                disconnect();
            }
        }
    }
}