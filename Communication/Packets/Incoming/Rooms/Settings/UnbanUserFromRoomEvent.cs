namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class UnbanUserFromRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var Instance = session.GetHabbo().CurrentRoom;
            if (Instance == null || !Instance.CheckRights(session, true))
            {
                return;
            }

            var UserId = packet.PopInt();
            var RoomId = packet.PopInt();
            if (Instance.GetBans().IsBanned(UserId))
            {
                Instance.GetBans().Unban(UserId);
                session.SendPacket(new UnbanUserFromRoomComposer(RoomId, UserId));
            }
        }
    }
}