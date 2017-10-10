namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;
    using Outgoing.GameCenter;

    internal class GetGameListingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Games = PlusEnvironment.GetGame().GetGameDataManager().GameData;
            Session.SendPacket(new GameListComposer(Games));
        }
    }
}