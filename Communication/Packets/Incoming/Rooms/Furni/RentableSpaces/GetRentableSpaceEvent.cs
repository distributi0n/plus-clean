namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.RentableSpaces;

    internal class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Something = Packet.PopInt();
            Session.SendPacket(new RentableSpaceComposer());
        }
    }
}