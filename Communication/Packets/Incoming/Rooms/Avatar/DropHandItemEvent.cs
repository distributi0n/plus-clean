namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class DropHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            if (User.CarryItemID > 0 && User.CarryTimer > 0)
            {
                User.CarryItem(0);
            }
        }
    }
}