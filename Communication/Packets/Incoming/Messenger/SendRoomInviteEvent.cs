namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using Outgoing.Messenger;
    using Utilities;

    internal sealed class SendRoomInviteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Oops, you're currently muted - you cannot send room invitations.");
                return;
            }

            var Amount = Packet.PopInt();
            if (Amount > 500)
            {
                return; // don't send at all
            }

            var Targets = new List<int>();
            for (var i = 0; i < Amount; i++)
            {
                var uid = Packet.PopInt();
                if (i < 100) // limit to 100 people, keep looping until we fulfil the request though
                {
                    Targets.Add(uid);
                }
            }

            var Message = StringCharFilter.Escape(Packet.PopString());
            if (Message.Length > 121)
            {
                Message = Message.Substring(0, 121);
            }
            foreach (var UserId in Targets)
            {
                if (!Session.GetHabbo().GetMessenger().FriendshipExists(UserId))
                {
                    continue;
                }

                var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                if (Client == null ||
                    Client.GetHabbo() == null ||
                    Client.GetHabbo().AllowMessengerInvites ||
                    Client.GetHabbo().AllowConsoleMessages == false)
                {
                    continue;
                }

                Client.SendPacket(new RoomInviteComposer(Session.GetHabbo().Id, Message));
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `chatlogs_console_invitations` (`user_id`,`message`,`timestamp`) VALUES (@userId, @message, UNIX_TIMESTAMP())");
                dbClient.AddParameter("userId", Session.GetHabbo().Id);
                dbClient.AddParameter("message", Message);
                dbClient.RunQuery();
            }
        }
    }
}