namespace Plus.Communication
{
    using System;
    using System.IO;
    using ConnectionManager;
    using HabboHotel.GameClients;
    using log4net;
    using Packets.Incoming;
    using Utilities;

    public sealed class GamePacketParser : IDataParser
    {
        public delegate void HandlePacket(ClientPacket message);

        private static readonly ILog log = LogManager.GetLogger("Plus.Messages.Net.GamePacketParser");

        private readonly GameClient currentClient;
        private bool _deciphered;
        private byte[] _halfData;

        private bool _halfDataRecieved;
        private ConnectionInformation con;

        public GamePacketParser(GameClient me) => currentClient = me;

        public void handlePacketData(byte[] Data)
        {
            try
            {
                if (currentClient.RC4Client != null && !_deciphered)
                {
                    currentClient.RC4Client.Decrypt(ref Data);
                    _deciphered = true;
                }
                if (_halfDataRecieved)
                {
                    var FullDataRcv = new byte[_halfData.Length + Data.Length];
                    Buffer.BlockCopy(_halfData, 0, FullDataRcv, 0, _halfData.Length);
                    Buffer.BlockCopy(Data, 0, FullDataRcv, _halfData.Length, Data.Length);
                    _halfDataRecieved = false; // mark done this round
                    handlePacketData(FullDataRcv); // repeat now we have the combined array
                    return;
                }

                using (var Reader = new BinaryReader(new MemoryStream(Data)))
                {
                    if (Data.Length < 4)
                    {
                        return;
                    }

                    var MsgLen = HabboEncoding.DecodeInt32(Reader.ReadBytes(4));
                    if (Reader.BaseStream.Length - 4 < MsgLen)
                    {
                        _halfData = Data;
                        _halfDataRecieved = true;
                        return;
                    }

                    if (MsgLen < 0 || MsgLen > 5120) //TODO: Const somewhere.
                    {
                        return;
                    }

                    var Packet = Reader.ReadBytes(MsgLen);
                    using (var R = new BinaryReader(new MemoryStream(Packet)))
                    {
                        var Header = HabboEncoding.DecodeInt16(R.ReadBytes(2));
                        var Content = new byte[Packet.Length - 2];
                        Buffer.BlockCopy(Packet, 2, Content, 0, Packet.Length - 2);
                        var Message = new ClientPacket(Header, Content);
                        onNewPacket.Invoke(Message);
                        _deciphered = false;
                    }
                    if (Reader.BaseStream.Length - 4 > MsgLen)
                    {
                        var Extra = new byte[Reader.BaseStream.Length - Reader.BaseStream.Position];
                        Buffer.BlockCopy(Data,
                            (int) Reader.BaseStream.Position,
                            Extra,
                            0,
                            (int) (Reader.BaseStream.Length - Reader.BaseStream.Position));
                        _deciphered = true;
                        handlePacketData(Extra);
                    }
                }
            }
            catch (Exception e)
            {
                //log.Error("Packet Error!", e);
            }
        }

        public void Dispose()
        {
            onNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone() => new GamePacketParser(currentClient);

        public event HandlePacket onNewPacket;

        public void SetConnection(ConnectionInformation con)
        {
            this.con = con;
            onNewPacket = null;
        }
    }
}