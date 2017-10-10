namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using Outgoing.Catalog;
    using Outgoing.Groups;
    using Outgoing.Inventory.Purse;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Session;

    internal sealed class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var Description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var RoomId = packet.PopInt();
            var Colour1 = packet.PopInt();
            var Colour2 = packet.PopInt();
            var Unknown = packet.PopInt();
            var groupCost = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("catalog.group.purchase.cost"));
            if (session.GetHabbo().Credits < groupCost)
            {
                session.SendPacket(
                    new BroadcastMessageAlertComposer("A group costs " + groupCost + " credits! You only have " +
                                                      session.GetHabbo().Credits + "!"));
                return;
            }

            session.GetHabbo().Credits -= groupCost;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            var Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Room == null || Room.OwnerId != session.GetHabbo().Id || Room.Group != null)
            {
                return;
            }

            var Badge = string.Empty;
            for (var i = 0; i < 5; i++)
            {
                Badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.PopInt().ToString(), packet.PopInt().ToString(),
                    packet.PopInt().ToString());
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame()
                .GetGroupManager()
                .TryCreateGroup(session.GetHabbo(), Name, Description, RoomId, Badge, Colour1, Colour2, out Group))
            {
                session.SendNotification(
                    "An error occured whilst trying to create this group.\n\nTry again. If you get this message more than once, report it at the link below.\r\rhttp://boonboards.com");
                return;
            }

            session.SendPacket(new PurchaseOKComposer());
            Room.Group = Group;
            if (session.GetHabbo().CurrentRoomId != Room.Id)
            {
                session.SendPacket(new RoomForwardComposer(Room.Id));
            }
            session.SendPacket(new NewGroupInfoComposer(RoomId, Group.Id));
        }
    }
}