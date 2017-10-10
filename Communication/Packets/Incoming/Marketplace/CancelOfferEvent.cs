namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Data;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Inventory.Furni;
    using Outgoing.Marketplace;

    internal sealed class CancelOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            DataRow Row = null;
            var OfferId = Packet.PopInt();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `furni_id`, `item_id`, `user_id`, `extra_data`, `offer_id`, `state`, `timestamp`, `limited_number`, `limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
                dbClient.AddParameter("OfferId", OfferId);
                Row = dbClient.GetRow();
            }
            if (Row == null)
            {
                Session.SendPacket(new MarketplaceCancelOfferResultComposer(OfferId, false));
                return;
            }

            if (Convert.ToInt32(Row["user_id"]) != Session.GetHabbo().Id)
            {
                Session.SendPacket(new MarketplaceCancelOfferResultComposer(OfferId, false));
                return;
            }

            ItemData Item = null;
            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Convert.ToInt32(Row["item_id"]), out Item))
            {
                Session.SendPacket(new MarketplaceCancelOfferResultComposer(OfferId, false));
                return;
            }

            //PlusEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, 1, Convert.ToString(Row["extra_data"]), Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]), Convert.ToInt32(Row["furni_id"]));
            var GiveItem = ItemFactory.CreateSingleItem(Item,
                Session.GetHabbo(),
                Convert.ToString(Row["extra_data"]),
                Convert.ToString(Row["extra_data"]),
                Convert.ToInt32(Row["furni_id"]),
                Convert.ToInt32(Row["limited_number"]),
                Convert.ToInt32(Row["limited_stack"]));
            Session.SendPacket(new FurniListNotificationComposer(GiveItem.Id, 1));
            Session.SendPacket(new FurniListUpdateComposer());
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "DELETE FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId AND `user_id` = @UserId LIMIT 1");
                dbClient.AddParameter("OfferId", OfferId);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }
            Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            Session.SendPacket(new MarketplaceCancelOfferResultComposer(OfferId, true));
        }
    }
}