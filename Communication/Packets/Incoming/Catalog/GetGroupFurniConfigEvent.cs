namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    internal class GetGroupFurniConfigEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager()
                .GetGroupsForUser(Session.GetHabbo().Id)));
        }
    }
}