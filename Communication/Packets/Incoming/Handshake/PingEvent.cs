namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    internal sealed class PingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.PingCount = 0;
        }
    }
}