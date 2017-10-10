namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using Outgoing.Messenger;
    using Outgoing.Rooms.Session;

    internal class FindNewFriendsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Instance = PlusEnvironment.GetGame().GetRoomManager().TryGetRandomLoadedRoom();
            if (Instance != null)
            {
                Session.SendPacket(new FindFriendsProcessResultComposer(true));
                Session.SendPacket(new RoomForwardComposer(Instance.Id));
            }
            else
            {
                Session.SendPacket(new FindFriendsProcessResultComposer(false));
            }
        }
    }
}