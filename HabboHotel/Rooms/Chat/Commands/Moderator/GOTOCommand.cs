namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using GameClients;

    internal class GOTOCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";

        public string Parameters => "%room_id%";

        public string Description => "";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("You must specify a room id!");
                return;
            }

            int RoomID;
            if (!int.TryParse(Params[1], out RoomID))
            {
                Session.SendWhisper("You must enter a valid room ID");
            }
            else
            {
                var _room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RoomID);
                if (_room == null)
                {
                    Session.SendWhisper("This room does not exist!");
                }
                else
                {
                    Session.GetHabbo().PrepareRoom(_room.Id, "");
                }
            }
        }
    }
}