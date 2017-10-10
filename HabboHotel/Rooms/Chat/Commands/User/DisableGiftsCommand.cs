namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_gifts";

        public string Parameters => "";

        public string Description =>
            "Allows you to disable the ability to receive gifts or to enable the ability to receive gifts.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowGifts = !Session.GetHabbo().AllowGifts;
            Session.SendWhisper("You're " + (Session.GetHabbo().AllowGifts ? "now" : "no longer") + " accepting gifts.");
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_gifts` = @AllowGifts WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowGifts", PlusEnvironment.BoolToEnum(Session.GetHabbo().AllowGifts));
                dbClient.RunQuery();
            }
        }
    }
}