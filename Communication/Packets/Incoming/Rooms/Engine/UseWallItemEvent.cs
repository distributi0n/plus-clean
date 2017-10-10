namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;

    internal class UseWallItemEvent : IPacketEvent
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
            var oldData = Item.ExtraData;
            var request = Packet.PopInt();
            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
            Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);
            PlusEnvironment.GetGame().GetQuestManager()
                .ProgressUserQuest(Session, QuestType.EXPLORE_FIND_ITEM, Item.GetBaseItem().Id);
        }
    }
}