﻿namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using System;
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Bots;
    using GameClients;
    using Users.Inventory.Bots;

    internal class KickBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickbots";

        public string Parameters => "";

        public string Description => "Kick all of the bots from the room.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Oops, only the room owner can run this command!");
                return;
            }

            foreach (var User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.IsPet || !User.IsBot)
                {
                    continue;
                }

                RoomUser BotUser = null;
                if (!Room.GetRoomUserManager().TryGetBot(User.BotData.Id, out BotUser))
                {
                    return;
                }

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", User.BotData.Id);
                    dbClient.RunQuery();
                }
                Session.GetHabbo()
                    .GetInventoryComponent()
                    .TryAddBot(new Bot(Convert.ToInt32(BotUser.BotData.Id),
                        Convert.ToInt32(BotUser.BotData.ownerID),
                        BotUser.BotData.Name,
                        BotUser.BotData.Motto,
                        BotUser.BotData.Look,
                        BotUser.BotData.Gender));
                Session.SendPacket(new BotInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetBots()));
                Room.GetRoomUserManager().RemoveBot(BotUser.VirtualId, false);
            }

            Session.SendWhisper("Success, removed all bots.");
        }
    }
}