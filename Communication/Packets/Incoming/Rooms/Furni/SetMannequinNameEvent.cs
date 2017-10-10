namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;

    internal class SetMannequinNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var ItemId = Packet.PopInt();
            var Name = Packet.PopString();
            var Item = Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
            {
                return;
            }

            if (Item.ExtraData.Contains(Convert.ToChar(5)))
            {
                var Flags = Item.ExtraData.Split(Convert.ToChar(5));
                Item.ExtraData = Flags[0] + Convert.ToChar(5) + Flags[1] + Convert.ToChar(5) + Name;
            }
            else
            {
                Item.ExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @Ed WHERE `id` = @itemId LIMIT 1");
                dbClient.AddParameter("itemId", Item.Id);
                dbClient.AddParameter("Ed", Item.ExtraData);
                dbClient.RunQuery();
            }
            Item.UpdateState(true, true);
        }
    }
}