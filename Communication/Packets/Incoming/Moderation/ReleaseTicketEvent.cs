namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;

    internal sealed class ReleaseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var Amount = Packet.PopInt();
            for (var i = 0; i < Amount; i++)
            {
                ModerationTicket Ticket = null;
                if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(Packet.PopInt(), out Ticket))
                {
                    continue;
                }

                Ticket.Moderator = null;
                PlusEnvironment.GetGame().GetClientManager()
                    .SendPacket(new ModeratorSupportTicketComposer(Session.GetHabbo().Id, Ticket), "mod_tool");
            }
        }
    }
}