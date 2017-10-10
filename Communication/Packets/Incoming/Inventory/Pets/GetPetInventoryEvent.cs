namespace Plus.Communication.Packets.Incoming.Inventory.Pets
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Pets;

    internal class GetPetInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            var Pets = Session.GetHabbo().GetInventoryComponent().GetPets();
            Session.SendPacket(new PetInventoryComposer(Pets));
        }
    }
}