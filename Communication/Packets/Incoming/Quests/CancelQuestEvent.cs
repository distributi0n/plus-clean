namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.Quests;

    internal class CancelQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Quest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(Session.GetHabbo().GetStats().QuestID);
            if (Quest == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `user_quests` WHERE `user_id` = '" +
                                  Session.GetHabbo().Id +
                                  "' AND `quest_id` = '" +
                                  Quest.Id +
                                  "';" +
                                  "UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" +
                                  Session.GetHabbo().Id +
                                  "' LIMIT 1");
            }
            Session.GetHabbo().GetStats().QuestID = 0;
            Session.SendPacket(new QuestAbortedComposer());
            PlusEnvironment.GetGame().GetQuestManager().GetList(Session, null);
        }
    }
}