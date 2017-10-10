namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal sealed class OpenHelpToolEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new OpenHelpToolComposer());
        }
    }
}