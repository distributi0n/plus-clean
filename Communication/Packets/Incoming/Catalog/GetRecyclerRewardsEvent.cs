namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetRecyclerRewardsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new RecyclerRewardsComposer());
        }
    }
}