namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Session;

    internal class LetUserInEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session))
            {
                return;
            }

            var Name = Packet.PopString();
            var Accepted = Packet.PopBoolean();
            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Name);
            if (Client == null)
            {
                return;
            }

            if (Accepted)
            {
                Client.GetHabbo().RoomAuthOk = true;
                Client.SendPacket(new FlatAccessibleComposer(""));
                Room.SendPacket(new FlatAccessibleComposer(Client.GetHabbo().Username), true);
            }
            else
            {
                Client.SendPacket(new FlatAccessDeniedComposer(""));
                Room.SendPacket(new FlatAccessDeniedComposer(Client.GetHabbo().Username), true);
            }
        }
    }
}