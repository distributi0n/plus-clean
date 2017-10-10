namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;

    internal sealed class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            if (Session.GetHabbo().GetMessenger().RequestBuddy(Packet.PopString()))
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_FRIEND);
            }
        }
    }
}