namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetCatalogRoomPromotionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new GetCatalogRoomPromotionComposer(Session.GetHabbo().UsersRooms));
        }
    }
}