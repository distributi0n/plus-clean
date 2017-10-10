namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal sealed class LatencyTestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //Session.SendMessage(new LatencyTestComposer(Packet.PopInt()));
        }
    }
}