namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System.Text;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using GameClients;

    internal class RoomCommand : IChatCommand
    {
        public string PermissionRequired => "command_room";

        public string Parameters => "push/pull/enables/respect";

        public string Description => "Gives you the ability to enable or disable basic room commands.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Oops, you must choose a room option to disable.");
                return;
            }

            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Oops, only the room owner or staff can use this command.");
                return;
            }

            var Option = Params[1];
            switch (Option)
            {
                case "list":
                {
                    var List = new StringBuilder("");
                    List.AppendLine("Room Command List");
                    List.AppendLine("-------------------------");
                    List.AppendLine("Pet Morphs: " + (Room.PetMorphsAllowed ? "enabled" : "disabled"));
                    List.AppendLine("Pull: " + (Room.PullEnabled ? "enabled" : "disabled"));
                    List.AppendLine("Push: " + (Room.PushEnabled ? "enabled" : "disabled"));
                    List.AppendLine("Super Pull: " + (Room.SPullEnabled ? "enabled" : "disabled"));
                    List.AppendLine("Super Push: " + (Room.SPushEnabled ? "enabled" : "disabled"));
                    List.AppendLine("Respect: " + (Room.RespectNotificationsEnabled ? "enabled" : "disabled"));
                    List.AppendLine("Enables: " + (Room.EnablesEnabled ? "enabled" : "disabled"));
                    Session.SendNotification(List.ToString());
                    break;
                }
                case "push":
                {
                    Room.PushEnabled = !Room.PushEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.PushEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Push mode is now " + (Room.PushEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "spush":
                {
                    Room.SPushEnabled = !Room.SPushEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.SPushEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Super Push mode is now " + (Room.SPushEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "spull":
                {
                    Room.SPullEnabled = !Room.SPullEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.SPullEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Super Pull mode is now " + (Room.SPullEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "pull":
                {
                    Room.PullEnabled = !Room.PullEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `pull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.PullEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Pull mode is now " + (Room.PullEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "enable":
                case "enables":
                {
                    Room.EnablesEnabled = !Room.EnablesEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("EnablesEnabled", PlusEnvironment.BoolToEnum(Room.EnablesEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Enables mode set to " + (Room.EnablesEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "respect":
                {
                    Room.RespectNotificationsEnabled = !Room.RespectNotificationsEnabled;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery(
                            "UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" +
                            Room.Id +
                            "' LIMIT 1");
                        dbClient.AddParameter("RespectNotificationsEnabled",
                            PlusEnvironment.BoolToEnum(Room.RespectNotificationsEnabled));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Respect notifications mode set to " +
                                        (Room.RespectNotificationsEnabled ? "enabled!" : "disabled!"));
                    break;
                }
                case "pets":
                case "morphs":
                {
                    Room.PetMorphsAllowed = !Room.PetMorphsAllowed;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + Room.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("PetMorphsAllowed", PlusEnvironment.BoolToEnum(Room.PetMorphsAllowed));
                        dbClient.RunQuery();
                    }
                    Session.SendWhisper("Human pet morphs notifications mode set to " +
                                        (Room.PetMorphsAllowed ? "enabled!" : "disabled!"));
                    if (!Room.PetMorphsAllowed)
                    {
                        foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                        {
                            if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                            {
                                continue;
                            }

                            User.GetClient()
                                .SendWhisper("The room owner has disabled the ability to use a pet morph in this room.");
                            if (User.GetClient().GetHabbo().PetId > 0)
                            {
                                //Tell the user what is going on.
                                User.GetClient()
                                    .SendWhisper("Oops, the room owner has just disabled pet-morphs, un-morphing you.");

                                //Change the users Pet Id.
                                User.GetClient().GetHabbo().PetId = 0;

                                //Quickly remove the old user instance.
                                Room.SendPacket(new UserRemoveComposer(User.VirtualId));

                                //Add the new one, they won't even notice a thing!!11 8-)
                                Room.SendPacket(new UsersComposer(User));
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}