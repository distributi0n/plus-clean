namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using GameClients;

    internal class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_moonwalk";

        public string Parameters => "";

        public string Description => "Wear the shoes of Michael Jackson.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.moonwalkEnabled = !User.moonwalkEnabled;
            if (User.moonwalkEnabled)
            {
                Session.SendWhisper("Moonwalk enabled!");
            }
            else
            {
                Session.SendWhisper("Moonwalk disabled!");
            }
        }
    }
}