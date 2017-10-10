namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Rooms;
    using Outgoing.Groups;
    using Outgoing.Rooms.Permissions;

    internal sealed class GiveAdminRightsEvent : IPacketEvent
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
            if (Session.GetHabbo().Id != Group.CreatorId || !Group.IsMember(UserId))
            {
                return;
            }

            var Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            Group.MakeAdmin(UserId);
            Room Room = null;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
            {
                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null)
                {
                    if (!User.Statusses.ContainsKey("flatctrl 3"))
                    {
                        User.SetStatus("flatctrl 3", "");
                    }
                    User.UpdateNeeded = true;
                    if (User.GetClient() != null)
                    {
                        User.GetClient().SendPacket(new YouAreControllerComposer(3));
                    }
                }
            }
            Session.SendPacket(new GroupMemberUpdatedComposer(GroupId, Habbo, 1));
        }
    }
}