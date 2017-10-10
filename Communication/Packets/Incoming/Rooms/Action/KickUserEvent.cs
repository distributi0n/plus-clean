namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;

    internal class KickUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }
            if (!Room.CheckRights(Session) && Room.WhoCanKick != 2 && Room.Group == null)
            {
                return;
            }
            if (Room.Group != null && !Room.CheckRights(Session, false, true))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (User == null || User.IsBot)
            {
                return;
            }

            //Cannot kick owner or moderators.
            if (Room.CheckRights(User.GetClient(), true) || User.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            Room.GetRoomUserManager().RemoveUserFromRoom(User.GetClient(), true, true);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModKickSeen", 1);
        }
    }
}