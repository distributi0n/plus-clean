namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;

    internal class DiceOffEvent : IPacketEvent
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
            if (Room.CheckRights(Session))
            {
                hasRights = true;
            }
            Item.Interactor.OnTrigger(Session, Item, -1, hasRights);
        }
    }
}