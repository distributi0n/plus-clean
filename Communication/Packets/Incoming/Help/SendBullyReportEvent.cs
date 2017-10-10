namespace Plus.Communication.Packets.Incoming.Help
{
    using HabboHotel.GameClients;
    using Outgoing.Help;

    internal class SendBullyReportEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new SendBullyReportComposer());
        }
    }
}