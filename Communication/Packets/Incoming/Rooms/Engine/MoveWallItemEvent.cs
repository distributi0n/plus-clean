namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;

    internal class MoveWallItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session))
            {
                return;
            }

            var itemID = Packet.PopInt();
            var wallPositionData = Packet.PopString();
            var Item = Room.GetRoomItemHandler().GetItem(itemID);
            if (Item == null)
            {
                return;
            }

            try
            {
                var WallPos = Room.GetRoomItemHandler().WallPositionCheck(":" + wallPositionData.Split(':')[1]);
                Item.wallCoord = WallPos;
            }
            catch
            {
                return;
            }

            Room.GetRoomItemHandler().UpdateItem(Item);
            Room.SendPacket(new ItemUpdateComposer(Item, Room.OwnerId));
        }
    }
}