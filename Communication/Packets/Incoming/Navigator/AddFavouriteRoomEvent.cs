namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    public sealed class AddFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
            {
                return;
            }

            var RoomId = Packet.PopInt();
            var Data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Data == null || Session.GetHabbo().FavoriteRooms.Count >= 30 || Session.GetHabbo().FavoriteRooms.Contains(RoomId))
            {
                // send packet that favourites is full.
                return;
            }

            Session.GetHabbo().FavoriteRooms.Add(RoomId);
            Session.SendPacket(new UpdateFavouriteRoomComposer(RoomId, true));
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO user_favorites (user_id,room_id) VALUES (" + Session.GetHabbo().Id + "," + RoomId +
                                  ")");
            }
        }
    }
}