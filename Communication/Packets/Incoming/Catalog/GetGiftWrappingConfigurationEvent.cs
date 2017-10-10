namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public sealed class GetGiftWrappingConfigurationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new GiftWrappingConfigurationComposer());
        }
    }
}