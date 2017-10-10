namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Session;

    internal sealed class FindRandomFriendingRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Instance = PlusEnvironment.GetGame().GetRoomManager().TryGetRandomLoadedRoom();
            if (Instance != null)
            {
                Session.SendPacket(new RoomForwardComposer(Instance.Id));
            }
        }
    }
}