﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using System;
    using GameClients;

    internal class TradeBanCommand : IChatCommand
    {
        public string PermissionRequired => "command_trade_ban";

        public string Parameters => "%target% %length%";

        public string Description => "Trade ban another user.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a username and a valid length in days (min 1 day, max 365 days).");
                return;
            }

            var Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (Convert.ToDouble(Params[2]) == 0)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery(
                        "UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
                }
                if (Habbo.GetClient() != null)
                {
                    Habbo.TradingLockExpiry = 0;
                    Habbo.GetClient().SendNotification("Your outstanding trade ban has been removed.");
                }
                Session.SendWhisper("You have successfully removed " + Habbo.Username + "'s trade ban.");
                return;
            }

            double Days;
            if (double.TryParse(Params[2], out Days))
            {
                if (Days < 1)
                {
                    Days = 1;
                }
                if (Days > 365)
                {
                    Days = 365;
                }
                var Length = PlusEnvironment.GetUnixTimestamp() + Days * 86400;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '" +
                                      Length +
                                      "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" +
                                      Habbo.Id +
                                      "' LIMIT 1");
                }
                if (Habbo.GetClient() != null)
                {
                    Habbo.TradingLockExpiry = Length;
                    Habbo.GetClient().SendNotification("You have been trade banned for " + Days + " day(s)!");
                }
                Session.SendWhisper("You have successfully trade banned " + Habbo.Username + " for " + Days + " day(s).");
            }
            else
            {
                Session.SendWhisper("Please enter a valid integer.");
            }
        }
    }
}