﻿namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.AI.Pets;

    internal class GetPetInformationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var PetId = Packet.PopInt();
            RoomUser Pet = null;
            if (!Session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(PetId, out Pet))
            {
                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(PetId);
                if (User == null)
                {
                    return;
                }

                //Check some values first, please!
                if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                {
                    return;
                }

                //And boom! Let us send the information composer 8-).
                Session.SendPacket(new PetInformationComposer(User.GetClient().GetHabbo()));
                return;
            }

            //Continue as a regular pet..
            if (Pet.RoomId != Session.GetHabbo().CurrentRoomId || Pet.PetData == null)
            {
                return;
            }

            Session.SendPacket(new PetInformationComposer(Pet.PetData));
        }
    }
}