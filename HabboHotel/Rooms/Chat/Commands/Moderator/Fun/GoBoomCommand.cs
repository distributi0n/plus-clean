namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System.Linq;
    using GameClients;

    internal class GoBoomCommand : IChatCommand
    {
        public string PermissionRequired => "command_goboom";

        public string Parameters => "";

        public string Description => "Make the entire room go boom! (Applys effect 108)";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            var Users = Room.GetRoomUserManager().GetRoomUsers();
            if (Users.Count > 0)
            {
                foreach (var U in Users.ToList())
                {
                    if (U == null)
                    {
                        continue;
                    }

                    U.ApplyEffect(108);
                }
            }
        }
    }
}