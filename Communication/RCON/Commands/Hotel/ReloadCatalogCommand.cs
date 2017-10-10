﻿namespace Plus.Communication.RCON.Commands.Hotel
{
    using Packets.Outgoing.Catalog;

    internal class ReloadCatalogCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the catalog.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new CatalogUpdatedComposer());
            return true;
        }
    }
}