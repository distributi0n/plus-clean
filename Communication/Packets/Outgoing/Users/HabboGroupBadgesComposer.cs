namespace Plus.Communication.Packets.Outgoing.Users
{
    using System.Collections.Generic;
    using HabboHotel.Groups;

    internal class HabboGroupBadgesComposer : ServerPacket
    {
        public HabboGroupBadgesComposer(Dictionary<int, string> Badges) : base(ServerPacketHeader.HabboGroupBadgesMessageComposer)
        {
            WriteInteger(Badges.Count);
            foreach (var Badge in Badges)
            {
                WriteInteger(Badge.Key);
                WriteString(Badge.Value);
            }
        }

        public HabboGroupBadgesComposer(Group Group) : base(ServerPacketHeader.HabboGroupBadgesMessageComposer)
        {
            WriteInteger(1); //count
            {
                WriteInteger(Group.Id);
                WriteString(Group.Badge);
            }
        }
    }
}