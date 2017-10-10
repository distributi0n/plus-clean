namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Items.Wired;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class UseFurnitureEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var itemID = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(itemID);
            if (Item == null)
            {
                return;
            }

            var hasRights = false;
            if (Room.CheckRights(Session, false, true))
            {
                hasRights = true;
            }
            if (Item.GetBaseItem().InteractionType == InteractionType.banzaitele)
            {
                return;
            }

            if (Item.GetBaseItem().InteractionType == InteractionType.TONER)
            {
                if (!Room.CheckRights(Session, true))
                {
                    return;
                }

                if (Room.TonerData.Enabled == 0)
                {
                    Room.TonerData.Enabled = 1;
                }
                else
                {
                    Room.TonerData.Enabled = 0;
                }
                Room.SendPacket(new ObjectUpdateComposer(Item, Room.OwnerId));
                Item.UpdateState();
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `room_items_toner` SET `enabled` = '" + Room.TonerData.Enabled + "' LIMIT 1");
                }
                return;
            }

            if (Item.Data.InteractionType == InteractionType.GNOME_BOX && Item.UserID == Session.GetHabbo().Id)
            {
                Session.SendPacket(new GnomeBoxComposer(Item.Id));
            }
            var Toggle = true;
            if (Item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_1 ||
                Item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_2)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User == null)
                {
                    return;
                }

                if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    Toggle = false;
                }
            }

            var oldData = Item.ExtraData;
            var request = Packet.PopInt();
            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
            if (Toggle)
            {
                Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);
            }
            PlusEnvironment.GetGame().GetQuestManager()
                .ProgressUserQuest(Session, QuestType.EXPLORE_FIND_ITEM, Item.GetBaseItem().Id);
        }
    }
}