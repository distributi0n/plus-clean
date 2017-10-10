namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using Communication.Packets.Outgoing.Moderation;
    using GameClients;

    internal class HotelAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_hotel_alert";

        public string Parameters => "%message%";

        public string Description => "Send a message to the entire hotel.";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a message to send.");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 1);
            PlusEnvironment.GetGame()
                .GetClientManager()
                .SendPacket(new BroadcastMessageAlertComposer(Message + "\r\n" + "- " + Session.GetHabbo().Username));
        }
    }
}