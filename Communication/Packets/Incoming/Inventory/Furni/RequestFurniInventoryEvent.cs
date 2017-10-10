namespace Plus.Communication.Packets.Incoming.Inventory.Furni
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using MoreLinq;
    using Outgoing.Inventory.Furni;

    internal class RequestFurniInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Items = Session.GetHabbo().GetInventoryComponent().GetWallAndFloor;
            var page = 0;
            var pages = (Items.Count() - 1) / 700 + 1;
            if (!Items.Any())
            {
                Session.SendPacket(new FurniListComposer(Items.ToList(), 1, 0));
            }
            else
            {
                foreach (ICollection<Item> batch in Items.Batch(700))
                {
                    Session.SendPacket(new FurniListComposer(batch.ToList(), pages, page));
                    page++;
                }
            }
        }
    }
}