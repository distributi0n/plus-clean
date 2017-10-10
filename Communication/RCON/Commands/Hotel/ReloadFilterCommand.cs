﻿namespace Plus.Communication.RCON.Commands.Hotel
{
    internal class ReloadFilterCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the chatting filter manager.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
            return true;
        }
    }
}