namespace Plus.Communication.Packets.Outgoing.Notifications
{
    internal class MOTDNotificationComposer : ServerPacket
    {
        public MOTDNotificationComposer(string Message) : base(ServerPacketHeader.MOTDNotificationMessageComposer)
        {
            WriteInteger(1);
            WriteString(Message);
        }
    }
}