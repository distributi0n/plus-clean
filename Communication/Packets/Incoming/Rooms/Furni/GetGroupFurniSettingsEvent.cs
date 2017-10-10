namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Groups;
    using HabboHotel.Items;
    using Outgoing.Groups;

    internal class GetGroupFurniSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

            var ItemId = Packet.PopInt();
            var GroupId = Packet.PopInt();
            var Item = Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
            {
                return;
            }
            if (Item.Data.InteractionType != InteractionType.GUILD_GATE)
            {
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
            {
                return;
            }

            Session.SendPacket(new GroupFurniSettingsComposer(Group, ItemId, Session.GetHabbo().Id));
            Session.SendPacket(new GroupInfoComposer(Group, Session, false));
        }
    }
}