namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal class GetOwnOffersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new MarketPlaceOwnOffersComposer(Session.GetHabbo().Id));
        }
    }
}