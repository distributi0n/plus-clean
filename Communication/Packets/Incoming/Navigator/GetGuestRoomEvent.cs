namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var roomID = Packet.PopInt();
            var roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);
            if (roomData == null)
            {
                return;
            }

            var isLoading = Packet.PopInt() == 1;
            var checkEntry = Packet.PopInt() == 1;
            Session.SendPacket(new GetGuestRoomResultComposer(Session, roomData, isLoading, checkEntry));
        }
    }
}