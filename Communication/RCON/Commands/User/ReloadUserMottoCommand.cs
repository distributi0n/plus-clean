namespace Plus.Communication.RCON.Commands.User
{
    using Packets.Outgoing.Rooms.Engine;

    internal class ReloadUserMottoCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the users motto from the database.";

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
                dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
                dbClient.AddParameter("userID", userId);
                client.GetHabbo().Motto = dbClient.GetString();
            }

            // If we're in a room, we cannot really send the packets, so flag this as completed successfully, since we already updated it.
            if (!client.GetHabbo().InRoom)
            {
                return true;
            }

            //We are in a room, let's try to run the packets.
            var Room = client.GetHabbo().CurrentRoom;
            if (Room != null)
            {
                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
                if (User != null)
                {
                    Room.SendPacket(new UserChangeComposer(User, false));
                    return true;
                }
            }

            return false;
        }
    }
}