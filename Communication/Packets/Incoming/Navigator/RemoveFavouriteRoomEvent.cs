namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    public class RemoveFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Id = Packet.PopInt();
            Session.GetHabbo().FavoriteRooms.Remove(Id);
            Session.SendPacket(new UpdateFavouriteRoomComposer(Id, false));
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM user_favorites WHERE user_id = " + Session.GetHabbo().Id + " AND room_id = " + Id +
                                  " LIMIT 1");
            }
        }
    }
}