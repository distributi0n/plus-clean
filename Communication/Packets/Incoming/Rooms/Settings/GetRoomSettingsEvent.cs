namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Settings;

    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Packet.PopInt());
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            Session.SendPacket(new RoomSettingsDataComposer(Room));
        }
    }
}