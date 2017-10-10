namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Inventory.Furni;
    using Outgoing.Inventory.Purse;

    internal class CreditFurniRedeemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            if (PlusEnvironment.GetSettingsManager().TryGetValue("room.item.exchangeables.enabled") != "1")
            {
                Session.SendNotification("The hotel managers have temporarilly disabled exchanging!");
                return;
            }

            var Exchange = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Exchange == null)
            {
                return;
            }
            if (Exchange.Data.InteractionType != InteractionType.EXCHANGE)
            {
                return;
            }

            var Value = Exchange.Data.BehaviourData;
            if (Value > 0)
            {
                Session.GetHabbo().Credits += Value;
                Session.SendPacket(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @exchangeId LIMIT 1");
                dbClient.AddParameter("exchangeId", Exchange.Id);
                dbClient.RunQuery();
            }
            Session.SendPacket(new FurniListUpdateComposer());
            Room.GetRoomItemHandler().RemoveFurniture(null, Exchange.Id, false);
            Session.GetHabbo().GetInventoryComponent().RemoveItem(Exchange.Id);
        }
    }
}