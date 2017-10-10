namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal sealed class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }
            if (Session.GetHabbo().Id != Group.CreatorId && !Group.IsAdmin(Session.GetHabbo().Id))
            {
                return;
            }
            if (!Group.HasRequest(UserId))
            {
                return;
            }

            Group.HandleRequest(UserId, false);
            Session.SendPacket(new UnknownGroupComposer(Group.Id, UserId));
        }
    }
}