namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal sealed class AcceptGroupMembershipEvent : IPacketEvent
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
            if (Session.GetHabbo().Id != Group.CreatorId &&
                !Group.IsAdmin(Session.GetHabbo().Id) &&
                !Session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
            {
                return;
            }
            if (!Group.HasRequest(UserId))
            {
                return;
            }

            var Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            Group.HandleRequest(UserId, true);
            Session.SendPacket(new GroupMemberUpdatedComposer(GroupId, Habbo, 4));
        }
    }
}