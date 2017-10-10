namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;

    internal sealed class CreateFlatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
            {
                return;
            }

            if (Session.GetHabbo().UsersRooms.Count >= 500)
            {
                Session.SendPacket(new CanCreateRoomComposer(true, 500));
                return;
            }

            var Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var ModelName = Packet.PopString();
            var Category = Packet.PopInt();
            var MaxVisitors = Packet.PopInt(); //10 = min, 25 = max.
            var TradeSettings = Packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
            if (Name.Length < 3)
            {
                return;
            }
            if (Name.Length > 25)
            {
                return;
            }

            RoomModel RoomModel = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(ModelName, out RoomModel))
            {
                return;
            }

            SearchResultList SearchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(Category, out SearchResultList))
            {
                Category = 36;
            }
            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank)
            {
                Category = 36;
            }
            if (MaxVisitors < 10 || MaxVisitors > 25)
            {
                MaxVisitors = 10;
            }
            if (TradeSettings < 0 || TradeSettings > 2)
            {
                TradeSettings = 0;
            }
            var NewRoom = PlusEnvironment.GetGame()
                .GetRoomManager()
                .CreateRoom(Session, Name, Description, ModelName, Category, MaxVisitors, TradeSettings);
            if (NewRoom != null)
            {
                Session.SendPacket(new FlatCreatedComposer(NewRoom.Id, Name));
            }
        }
    }
}