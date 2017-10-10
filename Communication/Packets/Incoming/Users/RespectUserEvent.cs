namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Avatar;
    using Outgoing.Users;

    internal class RespectUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }
            if (!Session.GetHabbo().InRoom || Session.GetHabbo().GetStats().DailyRespectPoints <= 0)
            {
                return;
            }

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Packet.PopInt());
            if (User == null || User.GetClient() == null || User.GetClient().GetHabbo().Id == Session.GetHabbo().Id || User.IsBot)
            {
                return;
            }

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_RESPECT);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RespectGiven", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(User.GetClient(), "ACH_RespectEarned", 1);
            Session.GetHabbo().GetStats().DailyRespectPoints -= 1;
            Session.GetHabbo().GetStats().RespectGiven += 1;
            User.GetClient().GetHabbo().GetStats().Respect += 1;
            if (Room.RespectNotificationsEnabled)
            {
                Room.SendPacket(new RespectNotificationComposer(User.GetClient().GetHabbo().Id,
                    User.GetClient().GetHabbo().GetStats().Respect));
            }
            Room.SendPacket(new ActionComposer(ThisUser.VirtualId, 7));
        }
    }
}