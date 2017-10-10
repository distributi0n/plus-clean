namespace Plus.Communication.Packets.Incoming.Messenger
{
    using HabboHotel.GameClients;

    internal sealed class DeclineBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var DeclineAll = Packet.PopBoolean();
            var Amount = Packet.PopInt();
            if (!DeclineAll)
            {
                var RequestId = Packet.PopInt();
                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
            else
            {
                Session.GetHabbo().GetMessenger().HandleAllRequests();
            }
        }
    }
}