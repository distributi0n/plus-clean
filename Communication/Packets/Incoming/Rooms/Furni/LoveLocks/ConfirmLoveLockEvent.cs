﻿namespace Plus.Communication.Packets.Incoming.Rooms.Furni.LoveLocks
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using Outgoing.Rooms.Furni.LoveLocks;

    internal class ConfirmLoveLockEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var pId = Packet.PopInt();
            var isConfirmed = Packet.PopBoolean();
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(pId);
            if (Item == null || Item.GetBaseItem() == null || Item.GetBaseItem().InteractionType != InteractionType.LOVELOCK)
            {
                return;
            }

            var UserOneId = Item.InteractingUser;
            var UserTwoId = Item.InteractingUser2;
            var UserOne = Room.GetRoomUserManager().GetRoomUserByHabbo(UserOneId);
            var UserTwo = Room.GetRoomUserManager().GetRoomUserByHabbo(UserTwoId);
            if (UserOne == null && UserTwo == null)
            {
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                Session.SendNotification("Your partner has left the room or has cancelled the love lock.");
                return;
            }

            if (UserOne.GetClient() == null || UserTwo.GetClient() == null)
            {
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                Session.SendNotification("Your partner has left the room or has cancelled the love lock.");
                return;
            }

            if (UserOne == null)
            {
                UserTwo.CanWalk = true;
                UserTwo.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
                UserTwo.LLPartner = 0;
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                return;
            }

            if (UserTwo == null)
            {
                UserOne.CanWalk = true;
                UserOne.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
                UserOne.LLPartner = 0;
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                return;
            }

            if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
            {
                UserTwo.CanWalk = true;
                UserTwo.GetClient().SendNotification("It appears this love lock has already been locked.");
                UserTwo.LLPartner = 0;
                UserOne.CanWalk = true;
                UserOne.GetClient().SendNotification("It appears this love lock has already been locked.");
                UserOne.LLPartner = 0;
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                return;
            }

            if (!isConfirmed)
            {
                Item.InteractingUser = 0;
                Item.InteractingUser2 = 0;
                UserOne.LLPartner = 0;
                UserTwo.LLPartner = 0;
                UserOne.CanWalk = true;
                UserTwo.CanWalk = true;
                return;
            }

            if (UserOneId == Session.GetHabbo().Id)
            {
                Session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
                UserOne.LLPartner = UserTwoId;
            }
            else if (UserTwoId == Session.GetHabbo().Id)
            {
                Session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
                UserTwo.LLPartner = UserOneId;
            }
            if (UserOne.LLPartner == 0 || UserTwo.LLPartner == 0)
            {
                return;
            }

            Item.ExtraData =
                "1" +
                (char) 5 +
                UserOne.GetUsername() +
                (char) 5 +
                UserTwo.GetUsername() +
                (char) 5 +
                UserOne.GetClient().GetHabbo().Look +
                (char) 5 +
                UserTwo.GetClient().GetHabbo().Look +
                (char) 5 +
                DateTime.Now.ToString("dd/MM/yyyy");
            Item.InteractingUser = 0;
            Item.InteractingUser2 = 0;
            UserOne.LLPartner = 0;
            UserTwo.LLPartner = 0;
            Item.UpdateState(true, true);
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extraData WHERE `id` = @ID LIMIT 1");
                dbClient.AddParameter("extraData", Item.ExtraData);
                dbClient.AddParameter("ID", Item.Id);
                dbClient.RunQuery();
            }
            UserOne.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));
            UserTwo.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));
            UserOne.CanWalk = true;
            UserTwo.CanWalk = true;
            UserOne = null;
            UserTwo = null;
        }
    }
}