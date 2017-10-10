namespace Plus.Communication.Packets.Incoming.Moderation
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Moderation;

    internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Target == null)
            {
                return;
            }

            DataTable Table = null;
            var Visits = new Dictionary<double, RoomData>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `room_id`, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` = @id ORDER BY `entry_timestamp` DESC LIMIT 50");
                dbClient.AddParameter("id", UserId);
                Table = dbClient.GetTable();
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        var RData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(Row["room_id"]));
                        if (RData == null)
                        {
                            return;
                        }

                        if (!Visits.ContainsKey(Convert.ToDouble(Row["entry_timestamp"])))
                        {
                            Visits.Add(Convert.ToDouble(Row["entry_timestamp"]), RData);
                        }
                    }
                }
            }

            Session.SendPacket(new ModeratorUserRoomVisitsComposer(Target.GetHabbo(), Visits));
        }
    }
}