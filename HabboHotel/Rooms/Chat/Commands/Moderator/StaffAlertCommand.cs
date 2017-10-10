﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using Communication.Packets.Outgoing.Moderation;
    using GameClients;

    internal class StaffAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_staff_alert";

        public string Parameters => "%message%";

        public string Description => "Sends a message typed by you to the current online staff members.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a message to send.");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 1);
            PlusEnvironment.GetGame()
                .GetClientManager()
                .StaffAlert(new BroadcastMessageAlertComposer("Staff Alert:\r\r" + Message + "\r\n" + "- " +
                                                              Session.GetHabbo().Username));
        }
    }
}