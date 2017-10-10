namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;

    internal sealed class GetModeratorTicketChatlogsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
            {
                return;
            }

            var TicketId = Packet.PopInt();
            ModerationTicket Ticket = null;
            if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(TicketId, out Ticket) || Ticket.Room == null)
            {
                return;
            }

            var Data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Ticket.Room.Id);
            if (Data == null)
            {
                return;
            }

            Session.SendPacket(new ModeratorTicketChatlogComposer(Ticket, Data, Ticket.Timestamp));
        }
    }
}