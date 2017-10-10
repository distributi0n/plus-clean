namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.Trading;
    using Outgoing.Inventory.Trading;

    internal class TradingOfferItemEvent : IPacketEvent
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
            if (!RoomUser.IsTrading)
            {
                Session.SendPacket(new TradingClosedComposer(Session.GetHabbo().Id));
                return;
            }

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

            var TradeUser = Trade.Users[0];
            if (TradeUser.RoomUser != RoomUser)
            {
                TradeUser = Trade.Users[1];
            }
            if (TradeUser.OfferedItems.ContainsKey(Item.Id))
            {
                return;
            }

            Trade.RemoveAccepted();
            if (TradeUser.OfferedItems.Count <= 499)
            {
                var TotalLTDs = TradeUser.OfferedItems.Where(x => x.Value.LimitedNo > 0).Count();
                if (TotalLTDs < 9)
                {
                    TradeUser.OfferedItems.Add(Item.Id, Item);
                }
            }
            Trade.SendPacket(new TradingUpdateComposer(Trade));
        }
    }
}