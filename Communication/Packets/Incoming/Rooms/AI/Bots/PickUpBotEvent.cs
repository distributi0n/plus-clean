namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System;
    using System.Drawing;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Users.Inventory.Bots;
    using Outgoing.Inventory.Bots;

    internal class PickUpBotEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var BotId = Packet.PopInt();
            if (BotId == 0)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            RoomUser BotUser = null;
            if (!Room.GetRoomUserManager().TryGetBot(BotId, out BotUser))
            {
                return;
            }

            if (Session.GetHabbo().Id != BotUser.BotData.ownerID &&
                !Session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                Session.SendWhisper("You can only pick up your own bots!");
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", BotId);
                dbClient.RunQuery();
            }
            Room.GetGameMap().RemoveUserFromMap(BotUser, new Point(BotUser.X, BotUser.Y));
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
    }
}