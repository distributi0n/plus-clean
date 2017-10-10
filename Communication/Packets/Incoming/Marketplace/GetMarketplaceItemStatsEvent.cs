namespace Plus.Communication.Packets.Incoming.Marketplace
{
    using System;
    using System.Data;
    using HabboHotel.GameClients;
    using Outgoing.Marketplace;

    internal sealed class GetMarketplaceItemStatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt();
            var SpriteId = Packet.PopInt();
            DataRow Row = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
                dbClient.AddParameter("SpriteId", SpriteId);
                Row = dbClient.GetRow();
            }
            Session.SendPacket(new MarketplaceItemStatsComposer(ItemId, SpriteId,
                Row != null ? Convert.ToInt32(Row["avgprice"]) : 0));
        }
    }
}