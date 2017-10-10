namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;

    internal sealed class AcceptBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var Amount = Packet.PopInt();
            if (Amount > 50)
            {
                Amount = 50;
            }
            else if (Amount < 0)
            {
                return;
            }

            for (var i = 0; i < Amount; i++)
            {
                var RequestId = Packet.PopInt();
                MessengerRequest Request = null;
                if (!Session.GetHabbo().GetMessenger().TryGetRequest(RequestId, out Request))
                {
                    continue;
                }

                if (Request.To != Session.GetHabbo().Id)
                {
                    return;
                }

                if (!Session.GetHabbo().GetMessenger().FriendshipExists(Request.To))
                {
                    Session.GetHabbo().GetMessenger().CreateFriendship(Request.From);
                }
                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
        }
    }
}