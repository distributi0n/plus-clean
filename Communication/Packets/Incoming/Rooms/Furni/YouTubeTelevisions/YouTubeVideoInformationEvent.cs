namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class YouTubeVideoInformationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt();
            var VideoId = Packet.PopString();
            foreach (var Tele in PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList.ToList())
            {
                if (Tele.YouTubeId != VideoId)
                {
                    continue;
                }

                Session.SendPacket(new GetYouTubeVideoComposer(ItemId, Tele.YouTubeId));
            }
        }
    }
}