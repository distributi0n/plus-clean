﻿namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Rooms.Games.Teams;
    using Users;

    internal class ActorIsInTeamBox : IWiredItem
    {
        public ActorIsInTeamBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionActorIsInTeamBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopInt();
            StringData = Unknown2.ToString();
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0 || Instance == null || string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var Player = (Habbo) Params[0];
            if (Player == null)
            {
                return false;
            }

            var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (User == null)
            {
                return false;
            }
            if (int.Parse(StringData) == 1 && User.Team == TEAM.RED)
            {
                return true;
            }
            if (int.Parse(StringData) == 2 && User.Team == TEAM.GREEN)
            {
                return true;
            }
            if (int.Parse(StringData) == 3 && User.Team == TEAM.BLUE)
            {
                return true;
            }
            if (int.Parse(StringData) == 4 && User.Team == TEAM.YELLOW)
            {
                return true;
            }

            return false;
        }
    }
}