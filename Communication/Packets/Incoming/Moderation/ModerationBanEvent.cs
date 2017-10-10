namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;

    internal class ModerationBanEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_soft_ban"))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            var Length = Packet.PopInt() * 3600 + PlusEnvironment.GetUnixTimestamp();
            var Unknown1 = Packet.PopString();
            var Unknown2 = Packet.PopString();
            var IPBan = Packet.PopBoolean();
            var MachineBan = Packet.PopBoolean();
            if (MachineBan)
            {
                IPBan = false;
            }
            var Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            Message = Message != null ? Message : "No reason specified.";
            var Username = Habbo.Username;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
            }
            if (IPBan == false && MachineBan == false)
            {
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Message, Length);
            }
            else if (IPBan)
            {
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.IP, Habbo.Username, Message, Length);
            }
            else if (MachineBan)
            {
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.IP, Habbo.Username, Message, Length);
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Message, Length);
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.MACHINE, Habbo.Username, Message, Length);
            }
            var TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Habbo.Username);
            if (TargetClient != null)
            {
                TargetClient.Disconnect();
            }
        }
    }
}