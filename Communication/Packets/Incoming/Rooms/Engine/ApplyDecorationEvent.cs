namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class ApplyDecorationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Packet.PopInt());
            if (Item == null)
            {
                return;
            }
            if (Item.GetBaseItem() == null)
            {
                return;
            }

            var DecorationKey = string.Empty;
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.FLOOR:
                    DecorationKey = "floor";
                    break;
                case InteractionType.WALLPAPER:
                    DecorationKey = "wallpaper";
                    break;
                case InteractionType.LANDSCAPE:
                    DecorationKey = "landscape";
                    break;
            }
            switch (DecorationKey)
            {
                case "floor":
                    Room.Floor = Item.ExtraData;
                    Room.RoomData.Floor = Item.ExtraData;
                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_DECORATION_FLOOR);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RoomDecoFloor", 1);
                    break;
                case "wallpaper":
                    Room.Wallpaper = Item.ExtraData;
                    Room.RoomData.Wallpaper = Item.ExtraData;
                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_DECORATION_WALL);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RoomDecoWallpaper", 1);
                    break;
                case "landscape":
                    Room.Landscape = Item.ExtraData;
                    Room.RoomData.Landscape = Item.ExtraData;
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET `" + DecorationKey + "` = @extradata WHERE `id` = '" + Room.RoomId +
                                  "' LIMIT 1");
                dbClient.AddParameter("extradata", Item.ExtraData);
                dbClient.RunQuery();
                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
            }
            Session.GetHabbo().GetInventoryComponent().RemoveItem(Item.Id);
            Room.SendPacket(new RoomPropertyComposer(DecorationKey, Item.ExtraData));
        }
    }
}