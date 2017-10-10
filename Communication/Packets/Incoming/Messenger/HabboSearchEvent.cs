namespace Plus.Communication.Packets.Incoming.Messenger
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger;
    using Outgoing.Messenger;
    using Utilities;

    internal class HabboSearchEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var Query = StringCharFilter.Escape(Packet.PopString().Replace("%", ""));
            if (Query.Length < 1 || Query.Length > 100)
            {
                return;
            }

            var Friends = new List<SearchResult>();
            var OthersUsers = new List<SearchResult>();
            var Results = SearchResultFactory.GetSearchResult(Query);
            foreach (var Result in Results.ToList())
            {
                if (Session.GetHabbo().GetMessenger().FriendshipExists(Result.UserId))
                {
                    Friends.Add(Result);
                }
                else
                {
                    OthersUsers.Add(Result);
                }
            }

            Session.SendPacket(new HabboSearchResultComposer(Friends, OthersUsers));
        }
    }
}