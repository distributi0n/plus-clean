namespace Plus.Communication.Packets.Incoming.Avatar
{
    using HabboHotel.GameClients;
    using Outgoing.Avatar;

    internal sealed class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new WardrobeComposer(Session));
        }
    }
}