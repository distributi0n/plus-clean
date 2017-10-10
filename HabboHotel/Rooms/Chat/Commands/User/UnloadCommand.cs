namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using GameClients;

    internal class UnloadCommand : IChatCommand
    {
        public string PermissionRequired => "command_unload";

        public string Parameters => "%id%";

        public string Description => "Unload the current room.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().GetPermissions().HasRight("room_unload_any"))
            {
                Room R = null;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Room.Id, out R))
                {
                    return;
                }

                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(R, true);
            }
            else
            {
                if (Room.CheckRights(Session, true))
                {
                    PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room);
                }
            }
        }
    }
}