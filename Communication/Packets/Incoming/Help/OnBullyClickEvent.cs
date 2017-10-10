namespace Plus.Communication.Packets.Incoming.Help
{
    using HabboHotel.GameClients;

    internal class OnBullyClickEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //I am a very boring packet.
        }
    }
}