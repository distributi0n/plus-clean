namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Catalog;
    using Outgoing.Groups;
    using Outgoing.Moderation;

    internal class JoinGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Packet.PopInt(), out Group))
            {
                return;
            }
            if (Group.IsMember(Session.GetHabbo().Id) ||
                Group.IsAdmin(Session.GetHabbo().Id) ||
                Group.HasRequest(Session.GetHabbo().Id) && Group.GroupType == GroupType.PRIVATE)
            {
                return;
            }

            var Groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id);
            if (Groups.Count >= 1500)
            {
                Session.SendPacket(
                    new BroadcastMessageAlertComposer(
                        "Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
                return;
            }

            Group.AddMember(Session.GetHabbo().Id);
            if (Group.GroupType == GroupType.LOCKED)
            {
                var GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList()
                    where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id)
                    select Client).ToList();
                foreach (var Client in GroupAdmins)
                {
                    Client.SendPacket(new GroupMembershipRequestedComposer(Group.Id, Session.GetHabbo(), 3));
                }

                Session.SendPacket(new GroupInfoComposer(Group, Session));
            }
            else
            {
                Session.SendPacket(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager()
                    .GetGroupsForUser(Session.GetHabbo().Id)));
                Session.SendPacket(new GroupInfoComposer(Group, Session));
                if (Session.GetHabbo().CurrentRoom != null)
                {
                    Session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                }
                else
                {
                    Session.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                }
            }
        }
    }
}