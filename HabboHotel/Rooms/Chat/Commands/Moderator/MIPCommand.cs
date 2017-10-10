namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;
    using Moderation;

    internal class MIPCommand : IChatCommand
    {
        public string PermissionRequired => "command_mip";

        public string Parameters => "%username%";

        public string Description => "Machine ban, IP ban and account ban another user.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
                return;
            }

            var Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
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

            var IPAddress = string.Empty;
            var Expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            var Username = Habbo.Username;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
                IPAddress = dbClient.GetString();
            }
            string Reason = null;
            if (Params.Length >= 3)
            {
                Reason = CommandManager.MergeParams(Params, 2);
            }
            else
            {
                Reason = "No reason specified.";
            }
            if (!string.IsNullOrEmpty(IPAddress))
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.IP,
                    IPAddress, Reason, Expire);
            }
            PlusEnvironment.GetGame()
                .GetModerationManager()
                .BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Reason, Expire);
            if (!string.IsNullOrEmpty(Habbo.MachineId))
            {
                PlusEnvironment.GetGame()
                    .GetModerationManager()
                    .BanUser(Session.GetHabbo().Username, ModerationBanType.MACHINE, Habbo.MachineId, Reason, Expire);
            }
            var TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient != null)
            {
                TargetClient.Disconnect();
            }
            Session.SendWhisper("Success, you have machine, IP and account banned the user '" + Username + "' for '" + Reason +
                                "'!");
        }
    }
}