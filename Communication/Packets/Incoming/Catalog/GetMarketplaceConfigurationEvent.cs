namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetMarketplaceConfigurationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new MarketplaceConfigurationComposer());
        }
    }
}