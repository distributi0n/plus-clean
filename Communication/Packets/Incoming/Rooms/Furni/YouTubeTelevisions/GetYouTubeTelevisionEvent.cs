namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class GetYouTubeTelevisionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var ItemId = Packet.PopInt();
            var Videos = PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList;
            if (Videos.Count == 0)
            {
                Session.SendNotification("Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
                return;
            }

            var dict = PlusEnvironment.GetGame().GetTelevisionManager()._televisions;
            foreach (var value in RandomValues(dict).Take(1))
            {
                Session.SendPacket(new GetYouTubeVideoComposer(ItemId, value.YouTubeId));
            }

            Session.SendPacket(new GetYouTubePlaylistComposer(ItemId, Videos));
        }

        public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var rand = new Random();
            var values = dict.Values.ToList();
            var size = dict.Count;
            while (true)
            {
                yield return values[rand.Next(size)];
            }
        }
    }
}