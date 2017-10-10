namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using Outgoing.Messenger;
    using Outgoing.Rooms.Session;

    internal class FollowFriendEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var BuddyId = Packet.PopInt();
            if (BuddyId == 0 || BuddyId == Session.GetHabbo().Id)
            {
                return;
            }

            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(BuddyId);
            if (Client == null || Client.GetHabbo() == null)
            {
                return;
            }

            if (!Client.GetHabbo().InRoom)
            {
                Session.SendPacket(new FollowFriendFailedComposer(2));
                Session.GetHabbo().GetMessenger().UpdateFriend(Client.GetHabbo().Id, Client, true);
                return;
            }

            if (Session.GetHabbo().CurrentRoom != null && Client.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.RoomId == Client.GetHabbo().CurrentRoom.RoomId)
                {
                    return;
                }
            }

            Session.SendPacket(new RoomForwardComposer(Client.GetHabbo().CurrentRoomId));
        }
    }
}