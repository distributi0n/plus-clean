namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;

    internal class GetCatalogModeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageMode = Packet.PopString();
        }
    }
}