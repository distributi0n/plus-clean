namespace Plus.Communication.Packets.Incoming.LandingView
{
    using HabboHotel.GameClients;
    using Outgoing.LandingView;

    internal class GetPromoArticlesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var LandingPromotions = PlusEnvironment.GetGame().GetLandingManager().GetPromotionItems();
            Session.SendPacket(new PromoArticlesComposer(LandingPromotions));
        }
    }
}