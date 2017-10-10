namespace Plus.Communication.Packets.Outgoing.Users
{
    using System.Linq;
    using HabboHotel.Users;

    internal class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo Habbo) : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            WriteInteger(Habbo.Id);
            WriteInteger(Habbo.GetBadgeComponent().EquippedCount);
            foreach (var Badge in Habbo.GetBadgeComponent().GetBadges().ToList())
            {
                if (Badge.Slot <= 0)
                {
                    continue;
                }

                WriteInteger(Badge.Slot);
                WriteString(Badge.Code);
            }
        }
    }
}