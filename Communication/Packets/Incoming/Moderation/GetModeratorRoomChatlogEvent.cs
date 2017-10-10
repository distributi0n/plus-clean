namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Rooms.Chat.Logs;
    using Outgoing.Moderation;

    internal sealed class GetModeratorRoomChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var Junk = Packet.PopInt();
            var RoomId = Packet.PopInt();
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
            {
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().FlushAndSave();
            var Chats = new List<ChatlogEntry>();
            DataTable Data = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `chatlogs` WHERE `room_id` = @id ORDER BY `id` DESC LIMIT 100");
                dbClient.AddParameter("id", RoomId);
                Data = dbClient.GetTable();
                if (Data != null)
                {
                    foreach (DataRow Row in Data.Rows)
                    {
                        var Habbo = PlusEnvironment.GetHabboById(Convert.ToInt32(Row["user_id"]));
                        if (Habbo != null)
                        {
                            Chats.Add(new ChatlogEntry(Convert.ToInt32(Row["user_id"]),
                                RoomId,
                                Convert.ToString(Row["message"]),
                                Convert.ToDouble(Row["timestamp"]),
                                Habbo));
                        }
                    }
                }
            }

            Session.SendPacket(new ModeratorRoomChatlogComposer(Room, Chats));
        }
    }
}