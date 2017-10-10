namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;
    using HabboHotel.Navigator;

    internal class UserFlatCatsComposer : ServerPacket
    {
        public UserFlatCatsComposer(ICollection<SearchResultList> Categories, int Rank) : base(ServerPacketHeader
            .UserFlatCatsMessageComposer)
        {
            WriteInteger(Categories.Count);
            foreach (var Cat in Categories)
            {
                WriteInteger(Cat.Id);
                WriteString(Cat.PublicName);
                WriteBoolean(Cat.RequiredRank <= Rank);
                WriteBoolean(false);
                WriteString("");
                WriteString("");
                WriteBoolean(false);
            }
        }
    }
}