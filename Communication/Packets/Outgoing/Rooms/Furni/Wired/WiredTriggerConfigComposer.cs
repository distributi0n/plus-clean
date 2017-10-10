namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.Items.Wired;

    internal class WiredTriggerConfigComposer : ServerPacket
    {
        public WiredTriggerConfigComposer(IWiredItem Box, List<int> BlockedItems) : base(ServerPacketHeader
            .WiredTriggerConfigMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(5);
            WriteInteger(Box.SetItems.Count);
            foreach (var Item in Box.SetItems.Values.ToList())
            {
                WriteInteger(Item.Id);
            }

            WriteInteger(Box.Item.GetBaseItem().SpriteId);
            WriteInteger(Box.Item.Id);
            WriteString(Box.StringData);
            WriteInteger(Box is IWiredCycle ? 1 : 0);
            if (Box is IWiredCycle)
            {
                var Cycle = (IWiredCycle) Box;
                WriteInteger(Cycle.Delay);
            }
            WriteInteger(0);
            WriteInteger(WiredBoxTypeUtility.GetWiredId(Box.Type));
            WriteInteger(BlockedItems.Count());
            if (BlockedItems.Count() > 0)
            {
                foreach (var Id in BlockedItems.ToList())
                {
                    WriteInteger(Id);
                }
            }
        }
    }
}