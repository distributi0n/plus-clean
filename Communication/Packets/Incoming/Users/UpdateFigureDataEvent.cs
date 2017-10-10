namespace Plus.Communication.Packets.Incoming.Users
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Avatar;
    using Outgoing.Rooms.Engine;

    internal class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var Gender = Packet.PopString().ToUpper();
            var Look = PlusEnvironment.GetFigureManager()
                .ProcessFigure(Packet.PopString(), Gender, Session.GetHabbo().GetClothing().GetClothingParts, true);
            if (Look == Session.GetHabbo().Look)
            {
                return;
            }

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                {
                    Session.GetHabbo().SessionClothingBlocked = true;
                }
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
            {
                return;
            }

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;
            string[] AllowedGenders = {"M", "F"};
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you chose an invalid gender."));
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);
            Session.GetHabbo().Look = PlusEnvironment.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `look` = @look, `gender` = @gender WHERE `id` = '" + Session.GetHabbo().Id +
                                  "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_AvatarLooks", 1);
            Session.SendPacket(new AvatarAspectUpdateComposer(Look, Gender));
            if (Session.GetHabbo().Look.Contains("ha-1006"))
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WEAR_HAT);
            }
            if (Session.GetHabbo().InRoom)
            {
                var RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    Session.SendPacket(new UserChangeComposer(RoomUser, true));
                    Session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(RoomUser, false));
                }
            }
        }
    }
}