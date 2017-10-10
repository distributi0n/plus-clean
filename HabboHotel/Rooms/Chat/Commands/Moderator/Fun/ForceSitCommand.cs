﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using GameClients;

    internal class ForceSitCommand : IChatCommand
    {
        public string PermissionRequired => "command_forcesit";

        public string Parameters => "%username%";

        public string Description => "Force another to user sit.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Oops, you forgot to choose a target user!");
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (User == null)
            {
                return;
            }
            if (User.Statusses.ContainsKey("lie") || User.isLying || User.RidingHorse || User.IsWalking)
            {
                return;
            }

            if (!User.Statusses.ContainsKey("sit"))
            {
                if (User.RotBody % 2 == 0)
                {
                    if (User == null)
                    {
                        return;
                    }

                    try
                    {
                        User.Statusses.Add("sit", "1.0");
                        User.Z -= 0.35;
                        User.isSitting = true;
                        User.UpdateNeeded = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    User.RotBody--;
                    User.Statusses.Add("sit", "1.0");
                    User.Z -= 0.35;
                    User.isSitting = true;
                    User.UpdateNeeded = true;
                }
            }
            else if (User.isSitting)
            {
                User.Z += 0.35;
                User.Statusses.Remove("sit");
                User.Statusses.Remove("1.0");
                User.isSitting = false;
                User.UpdateNeeded = true;
            }
        }
    }
}