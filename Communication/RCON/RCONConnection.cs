namespace Plus.Communication.RCON
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using log4net;

    public class RCONConnection
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Communication.RCON.RCONConnection");
        private byte[] _buffer = new byte[1024];
        private Socket _socket;

        public RCONConnection(Socket socket)
        {
            _socket = socket;
            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnCallBack, _socket);
            }
            catch
            {
                Dispose();
            }
        }

        public void OnCallBack(IAsyncResult iAr)
        {
            try
            {
                var bytes = 0;
                if (!int.TryParse(_socket.EndReceive(iAr).ToString(), out bytes))
                {
                    Dispose();
                    return;
                }

                var data = Encoding.Default.GetString(_buffer, 0, bytes);
                if (!PlusEnvironment.GetRCONSocket().GetCommands().Parse(data))
                {
                    log.Error("Failed to execute a MUS command. Raw data: " + data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Dispose();
        }

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
            }
            _socket = null;
            _buffer = null;
        }
    }
}