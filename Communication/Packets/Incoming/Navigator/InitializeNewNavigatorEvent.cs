namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator.New;

    internal class InitializeNewNavigatorEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var TopLevelItems = PlusEnvironment.GetGame().GetNavigator().GetTopLevelItems();
            Session.SendPacket(new NavigatorMetaDataParserComposer(TopLevelItems));
            Session.SendPacket(new NavigatorLiftedRoomsComposer());
            Session.SendPacket(new NavigatorCollapsedCategoriesComposer());
            Session.SendPacket(new NavigatorPreferencesComposer());
        }
    }
}