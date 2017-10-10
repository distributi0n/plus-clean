namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;

    internal class ModifyRoomFilterListEvent : IPacketEvent
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

            var RoomId = Packet.PopInt();
            var Added = Packet.PopBoolean();
            var Word = Packet.PopString();
            if (Added)
            {
                Instance.GetFilter().AddFilter(Word);
            }
            else
            {
                Instance.GetFilter().RemoveFilter(Word);
            }
        }
    }
}