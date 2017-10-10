namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;
    using Outgoing.LandingView;

    internal class GetDailyQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UsersOnline = PlusEnvironment.GetGame().GetClientManager().Count;
            Session.SendPacket(new ConcurrentUsersGoalProgressComposer(UsersOnline));
        }
    }
}