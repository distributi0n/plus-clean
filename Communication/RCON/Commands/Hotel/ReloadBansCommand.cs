﻿namespace Plus.Communication.RCON.Commands.Hotel
{
    internal class ReloadBansCommand : IRCONCommand
    {
        public string Description => "This command is used to re-cache the bans.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
            return true;
        }
    }
}