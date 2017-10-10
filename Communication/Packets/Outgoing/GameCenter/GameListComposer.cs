namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    using System.Collections.Generic;
    using HabboHotel.Games;

    internal class GameListComposer : ServerPacket
    {
        public GameListComposer(ICollection<GameData> Games) : base(ServerPacketHeader.GameListMessageComposer)
        {
            WriteInteger(PlusEnvironment.GetGame().GetGameDataManager().GetCount()); //Game count
            foreach (var Game in Games)
            {
                WriteInteger(Game.GameId);
                WriteString(Game.GameName);
                WriteString(Game.ColourOne);
                WriteString(Game.ColourTwo);
                WriteString(Game.ResourcePath);
                WriteString(Game.StringThree);
            }
        }
    }
}