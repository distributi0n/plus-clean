namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Users;

    internal class GetHabboGroupBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Badges = new Dictionary<int, string>();
            foreach (var User in Room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (User.IsBot || User.IsPet || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                {
                    continue;
                }
                if (User.GetClient().GetHabbo().GetStats().FavouriteGroupId == 0 ||
                    Badges.ContainsKey(User.GetClient().GetHabbo().GetStats().FavouriteGroupId))
                {
                    continue;
                }

                Group Group = null;
                if (!PlusEnvironment.GetGame().GetGroupManager()
                    .TryGetGroup(User.GetClient().GetHabbo().GetStats().FavouriteGroupId, out Group))
                {
                    continue;
                }

                if (!Badges.ContainsKey(Group.Id))
                {
                    Badges.Add(Group.Id, Group.Badge);
                }
            }

            if (Session.GetHabbo().GetStats().FavouriteGroupId > 0)
            {
                Group Group = null;
                if (PlusEnvironment.GetGame().GetGroupManager()
                    .TryGetGroup(Session.GetHabbo().GetStats().FavouriteGroupId, out Group))
                {
                    if (!Badges.ContainsKey(Group.Id))
                    {
                        Badges.Add(Group.Id, Group.Badge);
                    }
                }
            }
            Room.SendPacket(new HabboGroupBadgesComposer(Badges));
            Session.SendPacket(new HabboGroupBadgesComposer(Badges));
        }
    }
}