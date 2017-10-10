namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.Catalog;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetCatalogPageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageId = Packet.PopInt();
            var Something = Packet.PopInt();
            var CataMode = Packet.PopString();
            CatalogPage Page = null;
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

            Session.SendPacket(new CatalogPageComposer(Page, CataMode));
        }
    }
}