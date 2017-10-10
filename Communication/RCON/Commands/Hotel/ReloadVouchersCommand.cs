﻿namespace Plus.Communication.RCON.Commands.Hotel
{
    internal class ReloadVouchersCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the voucher manager.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
            return true;
        }
    }
}