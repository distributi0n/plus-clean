namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomFilterListEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null)
            {
                return;
            }
            if (!Instance.CheckRights(Session))
            {
                return;
            }

            Session.SendPacket(new GetRoomFilterListComposer(Instance));
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModRoomFilterSeen", 1);
        }
    }
}