namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class ToggleYouTubeVideoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt(); //Item Id
            var VideoId = Packet.PopString(); //Video ID
            Session.SendPacket(new GetYouTubeVideoComposer(ItemId, VideoId));
        }
    }
}