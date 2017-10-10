namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.AI.Pets;

    internal class ModifyWhoCanRideHorseEvent : IPacketEvent
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

            var PetId = Packet.PopInt();
            RoomUser Pet = null;
            if (!Room.GetRoomUserManager().TryGetPet(PetId, out Pet))
            {
                return;
            }

            if (Pet.PetData.AnyoneCanRide == 1)
            {
                Pet.PetData.AnyoneCanRide = 0;
            }
            else
            {
                Pet.PetData.AnyoneCanRide = 1;
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `bots_petdata` SET `anyone_ride` = '" + Pet.PetData.AnyoneCanRide + "' WHERE `id` = '" +
                                  PetId + "' LIMIT 1");
            }
            Room.SendPacket(new PetInformationComposer(Pet.PetData));
        }
    }
}