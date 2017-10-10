namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.Trading;

    internal sealed class TradingCancelConfirmEvent : IPacketEvent
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

            Trade Trade = null;
            if (!Room.GetTrading().TryGetTrade(RoomUser.TradeId, out Trade))
            {
                return;
            }

            Trade.EndTrade(Session.GetHabbo().Id);
        }
    }
}