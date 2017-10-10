namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal class GetNavigatorFlatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Categories = PlusEnvironment.GetGame().GetNavigator().GetEventCategories();
            Session.SendPacket(new NavigatorFlatCatsComposer(Categories, Session.GetHabbo().Rank));
        }
    }
}