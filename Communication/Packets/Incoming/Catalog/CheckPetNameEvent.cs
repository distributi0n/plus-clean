namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public sealed class CheckPetNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PetName = Packet.PopString();
            if (PetName.Length < 2)
            {
                Session.SendPacket(new CheckPetNameComposer(2, "2"));
                return;
            }

            if (PetName.Length > 15)
            {
                Session.SendPacket(new CheckPetNameComposer(1, "15"));
                return;
            }

            if (!PlusEnvironment.IsValidAlphaNumeric(PetName))
            {
                Session.SendPacket(new CheckPetNameComposer(3, ""));
                return;
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(PetName))
            {
                Session.SendPacket(new CheckPetNameComposer(4, ""));
                return;
            }

            Session.SendPacket(new CheckPetNameComposer(0, ""));
        }
    }
}