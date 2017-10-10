﻿namespace Plus.HabboHotel.Items.Wired
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    public interface IWiredItem
    {
        Room Instance { get; set; }
        Item Item { get; set; }
        WiredBoxType Type { get; }
        ConcurrentDictionary<int, Item> SetItems { get; set; }
        string StringData { get; set; }
        bool BoolData { get; set; }
        string ItemsData { get; set; }
        void HandleSave(ClientPacket Packet);
        bool Execute(params object[] Params);
    }
}