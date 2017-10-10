﻿namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Permissions;
    using Outgoing.Rooms.Settings;

    internal class RemoveAllRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Instance;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Instance))
            {
                return;
            }
            if (!Instance.CheckRights(Session, true))
            {
                return;
            }

            foreach (var UserId in new List<int>(Instance.UsersWithRights))
            {
                var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null && !User.IsBot)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.UpdateNeeded = true;
                    User.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                    dbClient.AddParameter("uid", UserId);
                    dbClient.AddParameter("rid", Instance.Id);
                    dbClient.RunQuery();
                }
                Session.SendPacket(new FlatControllerRemovedComposer(Instance, UserId));
                Session.SendPacket(new RoomRightsListComposer(Instance));
                Session.SendPacket(new UserUpdateComposer(Instance.GetRoomUserManager().GetUserList().ToList()));
            }

            if (Instance.UsersWithRights.Count > 0)
            {
                Instance.UsersWithRights.Clear();
            }
        }
    }
}