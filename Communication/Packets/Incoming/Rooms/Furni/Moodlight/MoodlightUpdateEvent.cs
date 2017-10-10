namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Rooms;

    internal class MoodlightUpdateEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }
            if (!Room.CheckRights(Session, true) || Room.MoodlightData == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Room.MoodlightData.ItemId);
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.MOODLIGHT)
            {
                return;
            }

            var Preset = Packet.PopInt();
            var BackgroundMode = Packet.PopInt();
            var ColorCode = Packet.PopString();
            var Intensity = Packet.PopInt();
            var BackgroundOnly = false;
            if (BackgroundMode >= 2)
            {
                BackgroundOnly = true;
            }
            Room.MoodlightData.Enabled = true;
            Room.MoodlightData.CurrentPreset = Preset;
            Room.MoodlightData.UpdatePreset(Preset, ColorCode, Intensity, BackgroundOnly);
            Item.ExtraData = Room.MoodlightData.GenerateExtraData();
            Item.UpdateState();
        }
    }
}