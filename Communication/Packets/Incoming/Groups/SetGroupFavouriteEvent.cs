namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;
    using Outgoing.Users;

    internal sealed class SetGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
            {
                return;
            }

            var GroupId = Packet.PopInt();
            if (GroupId == 0)
            {
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }

            Session.GetHabbo().GetStats().FavouriteGroupId = Group.Id;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_stats` SET `groupid` = @groupId WHERE `id` = @userId LIMIT 1");
                dbClient.AddParameter("groupId", Session.GetHabbo().GetStats().FavouriteGroupId);
                dbClient.AddParameter("userId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }
            if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
            {
                Session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                if (Group != null)
                {
                    Session.GetHabbo().CurrentRoom.SendPacket(new HabboGroupBadgesComposer(Group));
                    var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                    {
                        Session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(Group, User.VirtualId));
                    }
                }
            }
            else
            {
                Session.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
        }
    }
}