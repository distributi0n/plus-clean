namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;
    using Outgoing.Messenger;

    internal class GetBuddyRequestsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            ICollection<MessengerRequest> Requests = Session.GetHabbo().GetMessenger().GetRequests().ToList();
            Session.SendPacket(new BuddyRequestsComposer(Requests));
        }
    }
}