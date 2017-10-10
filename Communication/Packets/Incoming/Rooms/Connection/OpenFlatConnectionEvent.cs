namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    using HabboHotel.GameClients;

    public class OpenFlatConnectionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var RoomId = Packet.PopInt();
            var Password = Packet.PopString();
            Session.GetHabbo().PrepareRoom(RoomId, Password);
        }
    }
}