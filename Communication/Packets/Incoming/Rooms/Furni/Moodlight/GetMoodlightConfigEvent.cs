namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Items.Data.Moodlight;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Furni.Moodlight;

    internal class GetMoodlightConfigEvent : IPacketEvent
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
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            if (Room.MoodlightData == null)
            {
                foreach (var item in Room.GetRoomItemHandler().GetWall.ToList())
                {
                    if (item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                    {
                        Room.MoodlightData = new MoodlightData(item.Id);
                    }
                }
            }

            if (Room.MoodlightData == null)
            {
                return;
            }

            Session.SendPacket(new MoodlightConfigComposer(Room.MoodlightData));
        }
    }
}