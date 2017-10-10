namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.Trading;
    using Outgoing.Inventory.Trading;

    internal class TradingOfferItemsEvent : IPacketEvent
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

            var Amount = Packet.PopInt();
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
            var AllItems = Session.GetHabbo().GetInventoryComponent().GetItems.Where(x => x.Data.Id == Item.Data.Id).Take(Amount)
                .ToList();
            foreach (var I in AllItems)
            {
                if (TradeUser.OfferedItems.ContainsKey(I.Id))
                {
                    return;
                }

                Trade.RemoveAccepted();
                TradeUser.OfferedItems.Add(I.Id, I);
            }

            Trade.SendPacket(new TradingUpdateComposer(Trade));
        }
    }
}