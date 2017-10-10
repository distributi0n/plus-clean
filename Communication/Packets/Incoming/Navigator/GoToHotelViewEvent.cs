namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                {
                    return;
                }

                if (OldRoom.GetRoomUserManager() != null)
                {
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);
                }
            }
        }
    }
}