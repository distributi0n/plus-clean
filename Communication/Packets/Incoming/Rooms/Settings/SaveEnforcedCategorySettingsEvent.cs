namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;

    internal class SaveEnforcedCategorySettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Packet.PopInt(), out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            var CategoryId = Packet.PopInt();
            var TradeSettings = Packet.PopInt();
            if (TradeSettings < 0 || TradeSettings > 2)
            {
                TradeSettings = 0;
            }
            SearchResultList SearchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(CategoryId, out SearchResultList))
            {
                CategoryId = 36;
            }
            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank)
            {
                CategoryId = 36;
            }
        }
    }
}