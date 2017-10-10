namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Cache.Type;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Rooms;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal class RemoveGroupMemberEvent : IPacketEvent
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

            if (UserId == Session.GetHabbo().Id)
            {
                if (Group.IsMember(UserId))
                {
                    Group.DeleteMember(UserId);
                }
                if (Group.IsAdmin(UserId))
                {
                    if (Group.IsAdmin(UserId))
                    {
                        Group.TakeAdmin(UserId);
                    }
                    Room Room;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                    {
                        return;
                    }

                    var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                    {
                        User.RemoveStatus("flatctrl 1");
                        User.UpdateNeeded = true;
                        if (User.GetClient() != null)
                        {
                            User.GetClient().SendPacket(new YouAreControllerComposer(0));
                        }
                    }
                }

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                    dbClient.AddParameter("GroupId", GroupId);
                    dbClient.AddParameter("UserId", UserId);
                    dbClient.RunQuery();
                }
                Session.SendPacket(new GroupInfoComposer(Group, Session));
                if (Session.GetHabbo().GetStats().FavouriteGroupId == GroupId)
                {
                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = @userId LIMIT 1");
                        dbClient.AddParameter("userId", UserId);
                        dbClient.RunQuery();
                    }
                    if (Group.AdminOnlyDeco == 0)
                    {
                        Room Room;
                        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                        {
                            return;
                        }

                        var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (User != null)
                        {
                            User.RemoveStatus("flatctrl 1");
                            User.UpdateNeeded = true;
                            if (User.GetClient() != null)
                            {
                                User.GetClient().SendPacket(new YouAreControllerComposer(0));
                            }
                        }
                    }

                    if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
                    {
                        var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (User != null)
                        {
                            Session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(Group, User.VirtualId));
                        }
                        Session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                    }
                    else
                    {
                        Session.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                    }
                }

                return;
            }

            if (Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id))
            {
                if (!Group.IsMember(UserId))
                {
                    return;
                }

                if (Group.IsAdmin(UserId) && Group.CreatorId != Session.GetHabbo().Id)
                {
                    Session.SendNotification("Sorry, only group creators can remove other administrators from the group.");
                    return;
                }

                if (Group.IsAdmin(UserId))
                {
                    Group.TakeAdmin(UserId);
                }
                if (Group.IsMember(UserId))
                {
                    Group.DeleteMember(UserId);
                }
                var Members = new List<UserCache>();
                var MemberIds = Group.GetAllMembers;
                foreach (var Id in MemberIds.ToList())
                {
                    var GroupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id);
                    if (GroupMember == null)
                    {
                        continue;
                    }

                    if (!Members.Contains(GroupMember))
                    {
                        Members.Add(GroupMember);
                    }
                }

                var FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                var MembersCount = Members.Count;
                Session.SendPacket(new GroupMembersComposer(Group,
                    Members.Take(FinishIndex).ToList(),
                    MembersCount,
                    1,
                    Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id),
                    0,
                    ""));
            }
        }
    }
}