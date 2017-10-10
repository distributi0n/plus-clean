﻿namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms.Trading;
    using Outgoing.Inventory.Trading;

    internal sealed class TradingAcceptEvent : IPacketEvent
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
                Session.SendPacket(new TradingClosedComposer(Session.GetHabbo().Id));
                return;
            }

            var TradeUser = Trade.Users[0];
            if (TradeUser.RoomUser != RoomUser)
            {
                TradeUser = Trade.Users[1];
            }
            TradeUser.HasAccepted = true;
            Trade.SendPacket(new TradingAcceptComposer(Session.GetHabbo().Id, true));
            if (Trade.AllAccepted)
            {
                Trade.SendPacket(new TradingCompleteComposer());
                Trade.CanChange = false;
                Trade.RemoveAccepted();
            }
        }
    }
}