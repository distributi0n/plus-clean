namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;

    internal class MuteUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var UserId = Packet.PopInt();
            var RoomId = Packet.PopInt();
            var Time = Packet.PopInt();
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }
            if (Room.WhoCanMute == 0 && !Room.CheckRights(Session, true) && Room.Group == null ||
                Room.WhoCanMute == 1 && !Room.CheckRights(Session) && Room.Group == null ||
                Room.Group != null && !Room.CheckRights(Session, false, true))
            {
                return;
            }

            var Target = Room.GetRoomUserManager().GetRoomUserByHabbo(PlusEnvironment.GetUsernameById(UserId));
            if (Target == null)
            {
                return;
            }
            if (Target.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            if (Room.MutedUsers.ContainsKey(UserId))
            {
                if (Room.MutedUsers[UserId] < PlusEnvironment.GetUnixTimestamp())
                {
                    Room.MutedUsers.Remove(UserId);
                }
                else
                {
                    return;
                }
            }

            Room.MutedUsers.Add(UserId, PlusEnvironment.GetUnixTimestamp() + Time * 60);
            Target.GetClient().SendWhisper("The room owner has muted you for " + Time + " minutes!");
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModMuteSeen", 1);
        }
    }
}