namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.BuildersClub;
    using Outgoing.Catalog;

    public sealed class GetCatalogIndexEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new CatalogIndexComposer(Session, PlusEnvironment.GetGame().GetCatalog().GetPages())); //, Sub));
            Session.SendPacket(new CatalogItemDiscountComposer());
            Session.SendPacket(new BCBorrowedItemsComposer());
        }
    }
}