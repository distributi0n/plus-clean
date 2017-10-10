namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal class UpdateNavigatorSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var roomID = Packet.PopInt();
            if (roomID == 0)
            {
                return;
            }

            var Data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);
            if (Data == null)
            {
                return;
            }

            Session.GetHabbo().HomeRoom = roomID;
            Session.SendPacket(new NavigatorSettingsComposer(roomID));
        }
    }
}