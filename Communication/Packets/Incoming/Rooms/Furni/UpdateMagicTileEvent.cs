namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Furni;

    internal class UpdateMagicTileEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }
            if (!Room.CheckRights(Session, false, true) &&
                !Session.GetHabbo().GetPermissions().HasRight("room_item_use_any_stack_tile"))
            {
                return;
            }

            var ItemId = Packet.PopInt();
            var DecimalHeight = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
            {
                return;
            }

            Item.GetZ = DecimalHeight / 100.0;
            Room.SendPacket(new ObjectUpdateComposer(Item, Convert.ToInt32(Session.GetHabbo().Id)));
            Room.SendPacket(new UpdateMagicTileComposer(ItemId, DecimalHeight));
        }
    }
}