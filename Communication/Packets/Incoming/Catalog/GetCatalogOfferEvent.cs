namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.Catalog;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal sealed class GetCatalogOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var OfferId = Packet.PopInt();
            if (!PlusEnvironment.GetGame().GetCatalog().ItemOffers.ContainsKey(OfferId))
            {
                return;
            }

            var PageId = PlusEnvironment.GetGame().GetCatalog().ItemOffers[OfferId];
            CatalogPage Page;
            if (!PlusEnvironment.GetGame().GetCatalog().TryGetPage(PageId, out Page))
            {
                return;
            }
            if (!Page.Enabled ||
                !Page.Visible ||
                Page.MinimumRank > Session.GetHabbo().Rank ||
                Page.MinimumVIP > Session.GetHabbo().VIPRank && Session.GetHabbo().Rank == 1)
            {
                return;
            }

            CatalogItem Item = null;
            if (!Page.ItemOffers.ContainsKey(OfferId))
            {
                return;
            }

            Item = Page.ItemOffers[OfferId];
            if (Item != null)
            {
                Session.SendPacket(new CatalogOfferComposer(Item));
            }
        }
    }
}