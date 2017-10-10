namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Cache.Type;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Groups;

    internal class GetGroupMembersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var Page = Packet.PopInt();
            var SearchVal = Packet.PopString();
            var RequestType = Packet.PopInt();
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }

            var Members = new List<UserCache>();
            switch (RequestType)
            {
                case 0:
                {
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

                    break;
                }
                case 1:
                {
                    var AdminIds = Group.GetAdministrators;
                    foreach (var Id in AdminIds.ToList())
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

                    break;
                }
                case 2:
                {
                    var RequestIds = Group.GetRequests;
                    foreach (var Id in RequestIds.ToList())
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

                    break;
                }
            }

            if (!string.IsNullOrEmpty(SearchVal))
            {
                Members = Members.Where(x => x.Username.StartsWith(SearchVal)).ToList();
            }
            var StartIndex = (Page - 1) * 14 + 14;
            var FinishIndex = Members.Count;
            Session.SendPacket(new GroupMembersComposer(Group,
                Members.Skip(StartIndex).Take(FinishIndex - StartIndex).ToList(),
                Members.Count,
                Page,
                Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id),
                RequestType,
                SearchVal));
        }
    }
}