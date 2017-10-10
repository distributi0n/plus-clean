﻿namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;
    using HabboHotel.Games;

    internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();
            GameData GameData = null;
            if (PlusEnvironment.GetGame().GetGameDataManager().TryGetGame(GameId, out GameData))
            {
                //Code
            }
        }
    }
}