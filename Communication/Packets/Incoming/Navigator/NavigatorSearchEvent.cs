namespace Plus.Communication.Packets.Incoming.Navigator
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using Outgoing.Navigator.New;

    internal class NavigatorSearchEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var Category = packet.PopString();
            var Search = packet.PopString();
            ICollection<SearchResultList> Categories = new List<SearchResultList>();
            if (!string.IsNullOrEmpty(Search))
            {
                SearchResultList QueryResult = null;
                if (PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(0, out QueryResult))
                {
                    Categories.Add(QueryResult);
                }
            }
            else
            {
                Categories = PlusEnvironment.GetGame().GetNavigator().GetCategorysForSearch(Category);
                if (Categories.Count == 0)
                {
                    //Are we going in deep?!
                    Categories = PlusEnvironment.GetGame().GetNavigator().GetResultByIdentifier(Category);
                    if (Categories.Count > 0)
                    {
                        session.SendPacket(new NavigatorSearchResultSetComposer(Category, Search, Categories, session, 2, 100));
                        return;
                    }
                }
            }

            session.SendPacket(new NavigatorSearchResultSetComposer(Category, Search, Categories, session));
        }
    }
}