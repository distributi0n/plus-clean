namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using HabboHotel.Catalog.Utilities;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Furni;
    using Outgoing.Rooms.AI.Pets;
    using Outgoing.Rooms.Engine;

    internal class RemoveSaddleFromHorseEvent : IPacketEvent
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

            RoomUser PetUser = null;
            if (!Room.GetRoomUserManager().TryGetPet(Packet.PopInt(), out PetUser))
            {
                return;
            }
            if (PetUser.PetData == null || PetUser.PetData.OwnerId != Session.GetHabbo().Id)
            {
                return;
            }

            //Fetch the furniture Id for the pets current saddle.
            var SaddleId = ItemUtility.GetSaddleId(PetUser.PetData.Saddle);

            //Remove the saddle from the pet.
            PetUser.PetData.Saddle = 0;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '0' WHERE `id` = '" + PetUser.PetData.PetId +
                                  "' LIMIT 1");
            }

            //Give the saddle back to the user.
            ItemData ItemData = null;
            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(SaddleId, out ItemData))
            {
                return;
            }

            var Item = ItemFactory.CreateSingleItemNullable(ItemData, Session.GetHabbo(), "", "", 0, 0, 0);
            if (Item != null)
            {
                Session.GetHabbo().GetInventoryComponent().TryAddItem(Item);
                Session.SendPacket(new FurniListNotificationComposer(Item.Id, 1));
                Session.SendPacket(new PurchaseOKComposer());
                Session.SendPacket(new FurniListAddComposer(Item));
                Session.SendPacket(new FurniListUpdateComposer());
            }

            //Update the Pet and the Pet figure information.
            Room.SendPacket(new UsersComposer(PetUser));
            Room.SendPacket(new PetHorseFigureInformationComposer(PetUser));
        }
    }
}