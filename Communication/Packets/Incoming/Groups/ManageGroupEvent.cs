namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal sealed class ManageGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }
            if (Group.CreatorId != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("group_management_override"))
            {
                return;
            }

            Session.SendPacket(new ManageGroupComposer(Group, Group.Badge.Replace("b", "").Split('s')));
        }
    }
}