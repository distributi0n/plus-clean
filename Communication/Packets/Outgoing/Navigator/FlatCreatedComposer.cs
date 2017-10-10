namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class FlatCreatedComposer : ServerPacket
    {
        public FlatCreatedComposer(int roomID, string roomName) : base(ServerPacketHeader.FlatCreatedMessageComposer)
        {
            WriteInteger(roomID);
            WriteString(roomName);
        }
    }
}