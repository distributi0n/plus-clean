namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;

    internal sealed class SendMsgEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var userId = Packet.PopInt();
            if (userId == 0 || userId == Session.GetHabbo().Id)
            {
                return;
            }

            var message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Oops, you're currently muted - you cannot send messages.");
                return;
            }

            Session.GetHabbo().GetMessenger().SendInstantMessage(userId, message);
        }
    }
}