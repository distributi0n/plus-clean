namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;

    internal sealed class ClientVariablesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GordanPath = Packet.PopString();
            var ExternalVariables = Packet.PopString();
        }
    }
}