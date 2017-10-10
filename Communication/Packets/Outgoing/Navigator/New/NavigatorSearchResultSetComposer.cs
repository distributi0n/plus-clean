namespace Plus.Communication.Packets.Outgoing.Navigator.New
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;

    internal class NavigatorSearchResultSetComposer : ServerPacket
    {
        public NavigatorSearchResultSetComposer(string Category,
            string Data,
            ICollection<SearchResultList> SearchResultLists,
            GameClient Session,
            int GoBack = 1,
            int FetchLimit = 12) : base(ServerPacketHeader.NavigatorSearchResultSetMessageComposer)
        {
            WriteString(Category); //Search code.
            WriteString(Data); //Text?
            WriteInteger(SearchResultLists.Count); //Count
            foreach (var SearchResult in SearchResultLists.ToList())
            {
                WriteString(SearchResult.CategoryIdentifier);
                WriteString(SearchResult.PublicName);
                WriteInteger(NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult.SearchAllowance) != 0
                    ? GoBack
                    : NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult
                        .SearchAllowance)); //0 = nothing, 1 = show more, 2 = back Action allowed.
                WriteBoolean(false); //True = minimized, false = open.
                WriteInteger(SearchResult.ViewMode == NavigatorViewMode.REGULAR
                    ? 0
                    : SearchResult.ViewMode == NavigatorViewMode.THUMBNAIL
                        ? 1
                        : 0); //View mode, 0 = tiny/regular, 1 = thumbnail
                NavigatorHandler.Search(this, SearchResult, Data, Session, FetchLimit);
            }
        }
    }
}