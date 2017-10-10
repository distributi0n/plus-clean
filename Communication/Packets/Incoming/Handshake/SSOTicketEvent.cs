namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;

    public sealed class SSOTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.RC4Client == null || Session.GetHabbo() != null)
            {
                return;
            }

            var SSO = Packet.PopString();
            if (string.IsNullOrEmpty(SSO) || SSO.Length < 15)
            {
                return;
            }

            Session.TryAuthenticate(SSO);
        }
    }
}