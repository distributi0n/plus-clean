namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;

    internal sealed class InitializeGameCenterEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
        }
    }
}