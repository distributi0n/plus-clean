﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using System.Linq;
    using GameClients;

    internal class RoomKickCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_kick";

        public string Parameters => "%message%";

        public string Description => "Kick the room and provide a message to the users.";

        public void Execute(GameClient session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please provide a reason to the users for this room kick.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 1);
            foreach (var roomUser in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (roomUser == null ||
                    roomUser.IsBot ||
                    roomUser.GetClient() == null ||
                    roomUser.GetClient().GetHabbo() == null ||
                    roomUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") ||
                    roomUser.GetClient().GetHabbo().Id == session.GetHabbo().Id)
                {
                    continue;
                }

                roomUser.GetClient().SendNotification("You have been kicked by a moderator: " + message);
                room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true);
            }

            session.SendWhisper("Successfully kicked all users from the room.");
        }
    }
}