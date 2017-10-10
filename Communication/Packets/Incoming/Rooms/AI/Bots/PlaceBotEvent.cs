namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.AI;
    using HabboHotel.Rooms.AI.Speech;
    using HabboHotel.Users.Inventory.Bots;
    using Outgoing.Inventory.Bots;

    internal class PlaceBotEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            var BotId = Packet.PopInt();
            var X = Packet.PopInt();
            var Y = Packet.PopInt();
            if (!Room.GetGameMap().CanWalk(X, Y, false) || !Room.GetGameMap().ValidTile(X, Y))
            {
                Session.SendNotification("You cannot place a bot here!");
                return;
            }

            Bot Bot = null;
            if (!Session.GetHabbo().GetInventoryComponent().TryGetBot(BotId, out Bot))
            {
                return;
            }

            var BotCount = 0;
            foreach (var User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.IsPet || !User.IsBot)
                {
                    continue;
                }

                BotCount += 1;
            }

            if (BotCount >= 5 && !Session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                Session.SendNotification("Sorry; 5 bots per room only!");
                return;
            }

            //TODO: Hmm, maybe not????
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `bots` SET `room_id` = @roomId, `x` = @CoordX, `y` = @CoordY WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("roomId", Room.RoomId);
                dbClient.AddParameter("BotId", Bot.Id);
                dbClient.AddParameter("CoordX", X);
                dbClient.AddParameter("CoordY", Y);
                dbClient.RunQuery();
            }
            var BotSpeechList = new List<RandomSpeech>();

            //TODO: Grab data?
            DataRow GetData = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `ai_type`,`rotation`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `id` = @BotId LIMIT 1");
                dbClient.AddParameter("BotId", Bot.Id);
                GetData = dbClient.GetRow();
                dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @BotId");
                dbClient.AddParameter("BotId", Bot.Id);
                var BotSpeech = dbClient.GetTable();
                foreach (DataRow Speech in BotSpeech.Rows)
                {
                    BotSpeechList.Add(new RandomSpeech(Convert.ToString(Speech["text"]), Bot.Id));
                }
            }

            var BotUser = Room.GetRoomUserManager()
                .DeployBot(new RoomBot(Bot.Id,
                        Session.GetHabbo().CurrentRoomId,
                        Convert.ToString(GetData["ai_type"]),
                        Convert.ToString(GetData["walk_mode"]),
                        Bot.Name,
                        "",
                        Bot.Figure,
                        X,
                        Y,
                        0,
                        4,
                        0,
                        0,
                        0,
                        0,
                        ref BotSpeechList,
                        "",
                        0,
                        Bot.OwnerId,
                        PlusEnvironment.EnumToBool(GetData["automatic_chat"].ToString()),
                        Convert.ToInt32(GetData["speaking_interval"]),
                        PlusEnvironment.EnumToBool(GetData["mix_sentences"].ToString()),
                        Convert.ToInt32(GetData["chat_bubble"])),
                    null);
            BotUser.Chat("Hello!", false, 0);
            Room.GetGameMap().UpdateUserMovement(new Point(X, Y), new Point(X, Y), BotUser);
            Bot ToRemove = null;
            if (!Session.GetHabbo().GetInventoryComponent().TryRemoveBot(BotId, out ToRemove))
            {
                Console.WriteLine("Error whilst removing Bot: " + ToRemove.Id);
                return;
            }

            Session.SendPacket(new BotInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetBots()));
        }
    }
}