namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    public class GetUserFlatCatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
            {
                return;
            }

            var Categories = PlusEnvironment.GetGame().GetNavigator().GetFlatCategories();
            Session.SendPacket(new UserFlatCatsComposer(Categories, Session.GetHabbo().Rank));
        }
    }
}