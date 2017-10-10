namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.Trading;
    using Outgoing.Inventory.Trading;

    internal class TradingRemoveItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (RoomUser == null)
            {
                return;
            }

            var ItemId = Packet.PopInt();
            Trade Trade = null;
            if (!Room.GetTrading().TryGetTrade(RoomUser.TradeId, out Trade))
            {
                Session.SendPacket(new TradingClosedComposer(Session.GetHabbo().Id));
                return;
            }

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
            if (Item == null)
            {
                return;
            }
            if (!Trade.CanChange)
            {
                return;
            }

            var User = Trade.Users[0];
            if (User.RoomUser != RoomUser)
            {
                User = Trade.Users[1];
            }
            if (!User.OfferedItems.ContainsKey(Item.Id))
            {
                return;
            }

            Trade.RemoveAccepted();
            User.OfferedItems.Remove(Item.Id);
            Trade.SendPacket(new TradingUpdateComposer(Trade));
        }
    }
}