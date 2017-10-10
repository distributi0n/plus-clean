namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public sealed class UniqueIDEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Junk = Packet.PopString();
            var MachineId = Packet.PopString();
            Session.MachineId = MachineId;
            Session.SendPacket(new SetUniqueIdComposer(MachineId));
        }
    }
}