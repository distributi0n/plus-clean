namespace Plus.Communication
{
    using System;
    using System.IO;
    using ConnectionManager;
    using HabboHotel.GameClients;
    using Packets.Incoming;
    using Utilities;

    public sealed class GamePacketParser : IDataParser
    {
        public delegate void HandlePacket(ClientPacket message);
        private readonly GameClient _currentClient;
        private bool _deciphered;
        private byte[] _halfData;

        private bool _halfDataRecieved;
        private ConnectionInformation con;

        public GamePacketParser(GameClient me) => _currentClient = me;

        public void handlePacketData(byte[] Data)
        {
            try
            {
                if (_currentClient.RC4Client != null && !_deciphered)
                {
                    _currentClient.RC4Client.Decrypt(ref Data);
                    _deciphered = true;
                }
                if (_halfDataRecieved)
                {
                    var fullDataRcv = new byte[_halfData.Length + Data.Length];
                    Buffer.BlockCopy(_halfData, 0, fullDataRcv, 0, _halfData.Length);
                    Buffer.BlockCopy(Data, 0, fullDataRcv, _halfData.Length, Data.Length);
                    _halfDataRecieved = false; // mark done this round
                    handlePacketData(fullDataRcv); // repeat now we have the combined array
                    return;
                }

                using (var Reader = new BinaryReader(new MemoryStream(Data)))
                {
                    if (Data.Length < 4)
                    {
                        return;
                    }

                    var msgLen = HabboEncoding.DecodeInt32(Reader.ReadBytes(4));
                    if (Reader.BaseStream.Length - 4 < msgLen)
                    {
                        _halfData = Data;
                        _halfDataRecieved = true;
                        return;
                    }

                    if (msgLen < 0 || msgLen > 5120) //TODO: Const somewhere.
                    {
                        return;
                    }

                    var packet = Reader.ReadBytes(msgLen);
                    using (var r = new BinaryReader(new MemoryStream(packet)))
                    {
                        var header = HabboEncoding.DecodeInt16(r.ReadBytes(2));
                        var content = new byte[packet.Length - 2];
                        Buffer.BlockCopy(packet, 2, content, 0, packet.Length - 2);
                        var message = new ClientPacket(header, content);

                        OnNewPacket?.Invoke(message);

                        _deciphered = false;
                    }
                    
                    if (Reader.BaseStream.Length - 4 <= msgLen)
                    {
                        return;
                    }

                    var extra = new byte[Reader.BaseStream.Length - Reader.BaseStream.Position];
                    
                    Buffer.BlockCopy(Data,
                        (int) Reader.BaseStream.Position,
                        extra,
                        0,
                        (int) (Reader.BaseStream.Length - Reader.BaseStream.Position));
                    
                    _deciphered = true;
                    handlePacketData(extra);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void Dispose()
        {
            OnNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone() => new GamePacketParser(_currentClient);

        public event HandlePacket OnNewPacket;

        internal void SetConnection(ConnectionInformation con)
        {
            this.con = con;
            OnNewPacket = null;
        }
    }
}