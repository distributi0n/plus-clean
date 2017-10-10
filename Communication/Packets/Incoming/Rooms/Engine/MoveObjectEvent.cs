namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class MoveObjectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var ItemId = Packet.PopInt();
            if (ItemId == 0)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            Item Item;
            if (Room.Group != null)
            {
                if (!Room.CheckRights(Session, false, true))
                {
                    Item = Room.GetRoomItemHandler().GetItem(ItemId);
                    if (Item == null)
                    {
                        return;
                    }

                    Session.SendPacket(new ObjectUpdateComposer(Item, Room.OwnerId));
                    return;
                }
            }
            else
            {
                if (!Room.CheckRights(Session))
                {
                    return;
                }
            }

            Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
            {
                return;
            }

            var x = Packet.PopInt();
            var y = Packet.PopInt();
            var Rotation = Packet.PopInt();
            if (x != Item.GetX || y != Item.GetY)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_MOVE);
            }
            if (Rotation != Item.Rotation)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_ROTATE);
            }
            if (!Room.GetRoomItemHandler().SetFloorItem(Session, Item, x, y, Rotation, false, false, true))
            {
                Room.SendPacket(new ObjectUpdateComposer(Item, Room.OwnerId));
                return;
            }

            if (Item.GetZ >= 0.1)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_STACK);
            }
        }
    }
}