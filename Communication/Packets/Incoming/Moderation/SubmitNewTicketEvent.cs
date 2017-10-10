namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using Outgoing.Moderation;
    using Utilities;

    internal sealed class SubmitNewTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            // Run a quick check to see if we have any existing tickets.
            if (PlusEnvironment.GetGame().GetModerationManager().UserHasTickets(Session.GetHabbo().Id))
            {
                var PendingTicket = PlusEnvironment.GetGame().GetModerationManager().GetTicketBySenderId(Session.GetHabbo().Id);
                if (PendingTicket != null)
                {
                    Session.SendPacket(new CallForHelpPendingCallsComposer(PendingTicket));
                    return;
                }
            }

            var Chats = new List<string>();
            var Message = StringCharFilter.Escape(Packet.PopString().Trim());
            var Category = Packet.PopInt();
            var ReportedUserId = Packet.PopInt();
            var Type = Packet.PopInt(); // Unsure on what this actually is.
            var ReportedUser = PlusEnvironment.GetHabboById(ReportedUserId);
            if (ReportedUser == null)
            {
                // User doesn't exist.
                return;
            }

            var Messagecount = Packet.PopInt();
            for (var i = 0; i < Messagecount; i++)
            {
                Packet.PopInt();
                Chats.Add(Packet.PopString());
            }

            var Ticket = new ModerationTicket(1,
                Type,
                Category,
                UnixTimestamp.GetNow(),
                1,
                Session.GetHabbo(),
                ReportedUser,
                Message,
                Session.GetHabbo().CurrentRoom,
                Chats);
            if (!PlusEnvironment.GetGame().GetModerationManager().TryAddTicket(Ticket))
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // TODO: Come back to this.
                /*dbClient.SetQuery("INSERT INTO `moderation_tickets` (`score`,`type`,`status`,`sender_id`,`reported_id`,`moderator_id`,`message`,`room_id`,`room_name`,`timestamp`) VALUES (1, '" + Category + "', 'open', '" + Session.GetHabbo().Id + "', '" + ReportedUserId + "', '0', @message, '0', '', '" + PlusEnvironment.GetUnixTimestamp() + "')");
                dbClient.AddParameter("message", Message);
                dbClient.RunQuery();*/
                dbClient.RunQuery("UPDATE `user_info` SET `cfhs` = `cfhs` + '1' WHERE `user_id` = '" + Session.GetHabbo().Id +
                                  "' LIMIT 1");
            }
            PlusEnvironment.GetGame().GetClientManager().ModAlert("A new support ticket has been submitted!");
            PlusEnvironment.GetGame().GetClientManager()
                .SendPacket(new ModeratorSupportTicketComposer(Session.GetHabbo().Id, Ticket), "mod_tool");
        }
    }
}