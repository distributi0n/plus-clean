namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Navigator;

    internal class NavigatorFlatCatsComposer : ServerPacket
    {
        public NavigatorFlatCatsComposer(ICollection<SearchResultList> Categories, int Rank) : base(ServerPacketHeader
            .NavigatorFlatCatsMessageComposer)
        {
            WriteInteger(Categories.Count);
            foreach (var Category in Categories.ToList())
            {
                WriteInteger(Category.Id);
                WriteString(Category.PublicName);
                WriteBoolean(true); //TODO
            }
        }
    }
}