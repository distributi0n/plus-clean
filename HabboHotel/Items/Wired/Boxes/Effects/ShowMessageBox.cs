﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;

    internal class ShowMessageBox : IWiredItem
    {
        public ShowMessageBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectShowMessage;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Message = Packet.PopString();
            StringData = Message;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                return false;
            }

            var Player = (Habbo) Params[0];
            if (Player == null || Player.GetClient() == null || string.IsNullOrWhiteSpace(StringData))
            {
                return false;
            }

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
            {
                return false;
            }

            var Message = StringData;
            if (StringData.Contains("%USERNAME%"))
            {
                Message = Message.Replace("%USERNAME%", Player.Username);
            }
            if (StringData.Contains("%ROOMNAME%"))
            {
                Message = Message.Replace("%ROOMNAME%", Player.CurrentRoom.Name);
            }
            if (StringData.Contains("%USERCOUNT%"))
            {
                Message = Message.Replace("%USERCOUNT%", Player.CurrentRoom.UserCount.ToString());
            }
            if (StringData.Contains("%USERSONLINE%"))
            {
                Message = Message.Replace("%USERSONLINE%", PlusEnvironment.GetGame().GetClientManager().Count.ToString());
            }
            Player.GetClient().SendPacket(new WhisperComposer(User.VirtualId, Message, 0, 34));
            return true;
        }
    }
}