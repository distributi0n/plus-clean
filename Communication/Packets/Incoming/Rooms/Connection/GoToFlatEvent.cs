namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Session;

    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Session.GetHabbo().EnterRoom(Session.GetHabbo().CurrentRoom))
            {
                Session.SendPacket(new CloseConnectionComposer());
            }
        }
    }
}