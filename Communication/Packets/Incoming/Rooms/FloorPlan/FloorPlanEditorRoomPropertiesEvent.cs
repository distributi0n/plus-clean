namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.FloorPlan;

    internal class FloorPlanEditorRoomPropertiesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Model = Room.GetGameMap().Model;
            if (Model == null)
            {
                return;
            }

            var FloorItems = Room.GetRoomItemHandler().GetFloor;
            Session.SendPacket(new FloorPlanFloorMapComposer(FloorItems));
            Session.SendPacket(new FloorPlanSendDoorComposer(Model.DoorX, Model.DoorY, Model.DoorOrientation));
            Session.SendPacket(new RoomVisualizationSettingsComposer(Room.WallThickness,
                Room.FloorThickness,
                PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
        }
    }
}