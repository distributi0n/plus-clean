namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.Quests;

    internal class GetCurrentQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var UserQuest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(Session.GetHabbo().QuestLastCompleted);
            var NextQuest = PlusEnvironment.GetGame().GetQuestManager()
                .GetNextQuestInSeries(UserQuest.Category, UserQuest.Number + 1);
            if (NextQuest == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `user_quests`(`user_id`,`quest_id`) VALUES (" + Session.GetHabbo().Id + ", " +
                                  NextQuest.Id + ")");
                dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '" + NextQuest.Id + "' WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }
            Session.GetHabbo().GetStats().QuestID = NextQuest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(Session, null);
            Session.SendPacket(new QuestStartedComposer(Session, NextQuest));
        }
    }
}