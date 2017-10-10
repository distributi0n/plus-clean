namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.AI.Bots;

    internal class OpenBotActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var BotId = Packet.PopInt();
            var ActionId = Packet.PopInt();
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            RoomUser BotUser = null;
            if (!Room.GetRoomUserManager().TryGetBot(BotId, out BotUser))
            {
                return;
            }

            var BotSpeech = "";
            foreach (var Speech in BotUser.BotData.RandomSpeech.ToList())
            {
                BotSpeech += Speech.Message + "\n";
            }

            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.AutomaticChat;
            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.SpeakingInterval;
            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.MixSentences;
            if (ActionId == 2 || ActionId == 5)
            {
                Session.SendPacket(new OpenBotActionComposer(BotUser, ActionId, BotSpeech));
            }
        }
    }
}