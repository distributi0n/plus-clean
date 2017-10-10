﻿namespace Plus.Communication.RCON.Commands.Hotel
{
    internal class ReloadQuestsCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the quests manager.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetQuestManager().Init();
            return true;
        }
    }
}