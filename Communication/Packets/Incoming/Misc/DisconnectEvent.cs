namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal sealed class DisconnectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.Disconnect();
        }
    }
}