namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;

    internal class PopularRoomTagsResultComposer : ServerPacket
    {
        public PopularRoomTagsResultComposer(ICollection<KeyValuePair<string, int>> Tags) : base(
            ServerPacketHeader.PopularRoomTagsResultMessageComposer)
        {
            WriteInteger(Tags.Count);
            foreach (var tag in Tags)
            {
                WriteString(tag.Key);
                WriteInteger(tag.Value);
            }
        }
    }
}