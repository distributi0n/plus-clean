namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Items;
    using Outgoing.Groups;
    using Outgoing.Rooms.Engine;

    internal sealed class UpdateGroupColoursEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var Colour1 = Packet.PopInt();
            var Colour2 = Packet.PopInt();
            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }
            if (Group.CreatorId != Session.GetHabbo().Id)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("colour1", Colour1);
                dbClient.AddParameter("colour2", Colour2);
                dbClient.AddParameter("groupId", Group.Id);
                dbClient.RunQuery();
            }
            Group.Colour1 = Colour1;
            Group.Colour2 = Colour2;
            Session.SendPacket(new GroupInfoComposer(Group, Session));
            if (Session.GetHabbo().CurrentRoom != null)
            {
                foreach (var Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
                {
                    if (Item == null || Item.GetBaseItem() == null)
                    {
                        continue;
                    }
                    if (Item.GetBaseItem().InteractionType != InteractionType.GUILD_ITEM &&
                        Item.GetBaseItem().InteractionType != InteractionType.GUILD_GATE ||
                        Item.GetBaseItem().InteractionType != InteractionType.GUILD_FORUM)
                    {
                        continue;
                    }

                    Session.GetHabbo().CurrentRoom.SendPacket(new ObjectUpdateComposer(Item, Convert.ToInt32(Item.UserID)));
                }
            }
        }
    }
}