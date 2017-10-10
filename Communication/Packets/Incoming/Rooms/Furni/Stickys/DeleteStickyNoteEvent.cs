namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;

    internal class DeleteStickyNoteEvent : IPacketEvent
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
            if (!Room.CheckRights(Session))
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null)
            {
                return;
            }

            if (Item.GetBaseItem().InteractionType == InteractionType.POSTIT ||
                Item.GetBaseItem().InteractionType == InteractionType.CAMERA_PICTURE)
            {
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }
            }
        }
    }
}