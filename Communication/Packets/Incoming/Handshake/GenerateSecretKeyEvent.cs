namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Encryption;
    using Encryption.Crypto.Prng;
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public sealed class GenerateSecretKeyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var CipherPublickey = Packet.PopString();
            var SharedKey = HabboEncryptionV2.CalculateDiffieHellmanSharedKey(CipherPublickey);
            if (SharedKey != 0)
            {
                Session.RC4Client = new Arc4(SharedKey.getBytes());
                Session.SendPacket(new SecretKeyComposer(HabboEncryptionV2.GetRsaDiffieHellmanPublicKey()));
            }
            else
            {
                Session.SendNotification("There was an error logging you in, please try again!");
            }
        }
    }
}