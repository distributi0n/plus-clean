namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Encryption;
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public sealed class InitCryptoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new InitCryptoComposer(HabboEncryptionV2.GetRsaDiffieHellmanPrimeKey(),
                HabboEncryptionV2.GetRsaDiffieHellmanGeneratorKey()));
        }
    }
}