namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;

    internal class ThrowDiceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null)
            {
                return;
            }

            var hasRights = false;
            if (Room.CheckRights(Session, false, true))
            {
                hasRights = true;
            }
            var request = Packet.PopInt();
            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
        }
    }
}