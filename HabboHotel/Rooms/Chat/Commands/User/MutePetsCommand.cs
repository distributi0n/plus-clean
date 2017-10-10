namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class MutePetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_pets";

        public string Parameters => "";

        public string Description => "Ignore bot chat or enable it again.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().AllowPetSpeech = !Session.GetHabbo().AllowPetSpeech;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `pets_muted` = '" +
                                  (Session.GetHabbo().AllowPetSpeech ? 1 : 0) +
                                  "' WHERE `id` = '" +
                                  Session.GetHabbo().Id +
                                  "' LIMIT 1");
            }
            if (Session.GetHabbo().AllowPetSpeech)
            {
                Session.SendWhisper("Change successful, you can no longer see speech from pets.");
            }
            else
            {
                Session.SendWhisper("Change successful, you can now see speech from pets.");
            }
        }
    }
}