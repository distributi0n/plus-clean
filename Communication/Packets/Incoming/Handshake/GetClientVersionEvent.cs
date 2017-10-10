namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    public sealed class GetClientVersionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Build = Packet.PopString();
            if (PlusEnvironment.SWFRevision != Build)
            {
                PlusEnvironment.SWFRevision = Build;
            }
        }
    }
}