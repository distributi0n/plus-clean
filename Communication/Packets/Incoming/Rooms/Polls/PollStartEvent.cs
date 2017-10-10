namespace Plus.Communication.Packets.Incoming.Rooms.Polls
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Polls;

    internal class PollStartEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new PollContentsComposer());
        }
    }
}