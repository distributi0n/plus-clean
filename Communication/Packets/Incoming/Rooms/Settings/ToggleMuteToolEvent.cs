namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class ToggleMuteToolEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            Room.RoomMuted = !Room.RoomMuted;
            var roomUsers = Room.GetRoomUserManager().GetRoomUsers();
            foreach (var roomUser in roomUsers.ToList())
            {
                if (roomUser == null || roomUser.GetClient() == null)
                {
                    continue;
                }

                if (Room.RoomMuted)
                {
                    roomUser.GetClient().SendWhisper("This room has been muted");
                }
                else
                {
                    roomUser.GetClient().SendWhisper("This room has been unmuted");
                }
            }

            Room.SendPacket(new RoomMuteSettingsComposer(Room.RoomMuted));
        }
    }
}