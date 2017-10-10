namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.Quests;

    internal class StartQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var QuestId = Packet.PopInt();
            var Quest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(QuestId);
            if (Quest == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `user_quests` (`user_id`,`quest_id`) VALUES ('" + Session.GetHabbo().Id + "', '" +
                                  Quest.Id + "')");
                dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '" + Quest.Id + "' WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }
            Session.GetHabbo().GetStats().QuestID = Quest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(Session, null);
            Session.SendPacket(new QuestStartedComposer(Session, Quest));
        }
    }
}