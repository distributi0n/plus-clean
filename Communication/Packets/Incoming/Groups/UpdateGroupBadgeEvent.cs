namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal class UpdateGroupBadgeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }
            if (Group.CreatorId != Session.GetHabbo().Id)
            {
                return;
            }

            var Count = Packet.PopInt();
            var Badge = "";
            for (var i = 0; i < Count; i++)
            {
                Badge += BadgePartUtility.WorkBadgeParts(i == 0, Packet.PopInt().ToString(), Packet.PopInt().ToString(),
                    Packet.PopInt().ToString());
            }

            Group.Badge = string.IsNullOrWhiteSpace(Badge) ? "b05114s06114" : Badge;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `badge` = @badge WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("badge", Group.Badge);
                dbClient.AddParameter("groupId", Group.Id);
                dbClient.RunQuery();
            }
            Session.SendPacket(new GroupInfoComposer(Group, Session));
        }
    }
}