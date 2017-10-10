namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;

    internal sealed class PickTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var Junk = Packet.PopInt(); //??
            var TicketId = Packet.PopInt();
            ModerationTicket Ticket = null;
            if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(TicketId, out Ticket))
            {
                return;
            }

            Ticket.Moderator = Session.GetHabbo();
            PlusEnvironment.GetGame().GetClientManager()
                .SendPacket(new ModeratorSupportTicketComposer(Session.GetHabbo().Id, Ticket), "mod_tool");
        }
    }
}