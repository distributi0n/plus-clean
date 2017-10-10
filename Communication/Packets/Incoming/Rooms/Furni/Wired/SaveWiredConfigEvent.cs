namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Wired
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using Outgoing.Rooms.Furni.Wired;

    internal class SaveWiredConfigEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, false, true))
            {
                return;
            }

            var ItemId = Packet.PopInt();
            Session.SendPacket(new HideWiredConfigComposer());
            var SelectedItem = Room.GetRoomItemHandler().GetItem(ItemId);
            if (SelectedItem == null)
            {
                return;
            }

            IWiredItem Box = null;
            if (!Session.GetHabbo().CurrentRoom.GetWired().TryGet(ItemId, out Box))
            {
                return;
            }

            if (Box.Type == WiredBoxType.EffectGiveUserBadge &&
                !Session.GetHabbo().GetPermissions().HasRight("room_item_wired_rewards"))
            {
                Session.SendNotification("You don't have the correct permissions to do this.");
                return;
            }

            Box.HandleSave(Packet);
            Session.GetHabbo().CurrentRoom.GetWired().SaveBox(Box);
        }
    }
}