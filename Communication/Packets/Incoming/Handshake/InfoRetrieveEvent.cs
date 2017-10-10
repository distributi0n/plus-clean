namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public class InfoRetrieveEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new UserObjectComposer(Session.GetHabbo()));
            Session.SendPacket(new UserPerksComposer(Session.GetHabbo()));
        }
    }
}