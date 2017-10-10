namespace Plus.Communication.Packets.Incoming.Navigator
{
    using HabboHotel.GameClients;
    using Outgoing.Navigator;

    internal sealed class CanCreateRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new CanCreateRoomComposer(false, 150));
        }
    }
}