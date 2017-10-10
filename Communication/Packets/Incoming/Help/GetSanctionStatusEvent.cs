namespace Plus.Communication.Packets.Incoming.Help
{
    using HabboHotel.GameClients;

    internal sealed class GetSanctionStatusEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //Session.SendMessage(new SanctionStatusComposer());
        }
    }
}