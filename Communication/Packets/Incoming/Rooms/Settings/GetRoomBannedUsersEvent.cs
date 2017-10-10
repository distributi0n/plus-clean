namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomBannedUsersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null || !Instance.CheckRights(Session, true))
            {
                return;
            }

            if (Instance.GetBans().BannedUsers().Count > 0)
            {
                Session.SendPacket(new GetRoomBannedUsersComposer(Instance));
            }
        }
    }
}