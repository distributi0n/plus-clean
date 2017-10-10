namespace Plus.Communication.RCON.Commands.User
{
    internal class ReloadUserVIPRankCommand : IRCONCommand
    {
        public string Description => "This command is used to reload a users VIP rank and permissions.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            var userId = 0;
            if (!int.TryParse(parameters[0], out userId))
            {
                return false;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `rank_vip` FROM `users` WHERE `id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", userId);
                client.GetHabbo().VIPRank = dbClient.GetInteger();
            }
            client.GetHabbo().GetPermissions().Init(client.GetHabbo());
            return true;
        }
    }
}