namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetPromotableRoomsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Rooms = Session.GetHabbo().UsersRooms;
            Rooms = Rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < PlusEnvironment.GetUnixTimestamp())
                .ToList();
            Session.SendPacket(new PromotableRoomsComposer(Rooms));
        }
    }
}