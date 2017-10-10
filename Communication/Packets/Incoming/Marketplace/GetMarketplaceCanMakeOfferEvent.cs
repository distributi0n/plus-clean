namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal sealed class GetMarketplaceCanMakeOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ErrorCode = Session.GetHabbo().TradingLockExpiry > 0 ? 6 : 1;
            Session.SendPacket(new MarketplaceCanMakeOfferResultComposer(ErrorCode));
        }
    }
}