namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetSellablePetBreedsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Type = Packet.PopString();
            var Item = PlusEnvironment.GetGame().GetItemManager().GetItemByName(Type);
            if (Item == null)
            {
                return;
            }

            var PetId = Item.BehaviourData;
            Session.SendPacket(new SellablePetBreedsComposer(Type,
                PetId,
                PlusEnvironment.GetGame().GetCatalog().GetPetRaceManager().GetRacesForRaceId(PetId)));
        }
    }
}