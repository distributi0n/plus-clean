namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Moderation;

    internal sealed class GetModeratorRoomInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var RoomId = Packet.PopInt();
            var Data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Data == null)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
            {
                return;
            }

            Session.SendPacket(new ModeratorRoomInfoComposer(Data,
                Room.GetRoomUserManager().GetRoomUserByHabbo(Data.OwnerName) != null));
        }
    }
}