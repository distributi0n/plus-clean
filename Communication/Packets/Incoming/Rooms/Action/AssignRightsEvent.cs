namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Permissions;
    using Outgoing.Rooms.Settings;

    internal class AssignRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var UserId = Packet.PopInt();
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            if (Room.UsersWithRights.Contains(UserId))
            {
                Session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.rights.user.has_rights"));
                return;
            }

            Room.UsersWithRights.Add(UserId);
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO `room_rights` (`room_id`,`user_id`) VALUES ('" + Room.RoomId + "','" + UserId +
                                  "')");
            }
            var RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (RoomUser != null && !RoomUser.IsBot)
            {
                RoomUser.SetStatus("flatctrl 1", "");
                RoomUser.UpdateNeeded = true;
                if (RoomUser.GetClient() != null)
                {
                    RoomUser.GetClient().SendPacket(new YouAreControllerComposer(1));
                }
                Session.SendPacket(new FlatControllerAddedComposer(Room.RoomId,
                    RoomUser.GetClient().GetHabbo().Id,
                    RoomUser.GetClient().GetHabbo().Username));
            }
            else
            {
                var User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);
                if (User != null)
                {
                    Session.SendPacket(new FlatControllerAddedComposer(Room.RoomId, User.Id, User.Username));
                }
            }
        }
    }
}