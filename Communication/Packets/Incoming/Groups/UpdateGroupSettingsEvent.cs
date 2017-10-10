namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Rooms;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal sealed class UpdateGroupSettingsEvent : IPacketEvent
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

            var Type = Packet.PopInt();
            var FurniOptions = Packet.PopInt();
            switch (Type)
            {
                default:
                case 0:
                    Group.GroupType = GroupType.OPEN;
                    break;
                case 1:
                    Group.GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    Group.GroupType = GroupType.PRIVATE;
                    break;
            }

            if (Group.GroupType != GroupType.LOCKED)
            {
                if (Group.GetRequests.Count > 0)
                {
                    foreach (var UserId in Group.GetRequests.ToList())
                    {
                        Group.HandleRequest(UserId, false);
                    }

                    Group.ClearRequests();
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `groups` SET `state` = @GroupState, `admindeco` = @AdminDeco WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("GroupState",
                    (Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2).ToString());
                dbClient.AddParameter("AdminDeco", (FurniOptions == 1 ? 1 : 0).ToString());
                dbClient.AddParameter("groupId", Group.Id);
                dbClient.RunQuery();
            }
            Group.AdminOnlyDeco = FurniOptions;
            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
            {
                return;
            }

            foreach (var User in Room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (Room.OwnerId == User.UserId || Group.IsAdmin(User.UserId) || !Group.IsMember(User.UserId))
                {
                    continue;
                }

                if (FurniOptions == 1)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.UpdateNeeded = true;
                    User.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                else if (FurniOptions == 0 && !User.Statusses.ContainsKey("flatctrl 1"))
                {
                    User.SetStatus("flatctrl 1", "");
                    User.UpdateNeeded = true;
                    User.GetClient().SendPacket(new YouAreControllerComposer(1));
                }
            }

            Session.SendPacket(new GroupInfoComposer(Group, Session));
        }
    }
}