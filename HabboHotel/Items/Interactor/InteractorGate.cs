﻿namespace Plus.HabboHotel.Items.Interactor
{
    using GameClients;
    using Wired;

    public class InteractorGate : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            var Modes = Item.GetBaseItem().Modes - 1;
            if (!HasRights)
            {
                return;
            }

            if (Modes <= 0)
            {
                Item.UpdateState(false, true);
            }
            var CurrentMode = 0;
            var NewMode = 0;
            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {
            }
            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }
            if (NewMode == 0)
            {
                if (!Item.GetRoom().GetGameMap().itemCanBePlacedHere(Item.GetX, Item.GetY))
                {
                    return;
                }
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();
            Item.GetRoom().GetGameMap().updateMapForItem(Item);
            Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);

            //Item.GetRoom().GenerateMaps();
        }

        public void OnWiredTrigger(Item Item)
        {
            var Modes = Item.GetBaseItem().Modes - 1;
            if (Modes <= 0)
            {
                Item.UpdateState(false, true);
            }
            var CurrentMode = 0;
            var NewMode = 0;
            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {
            }
            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }
            if (NewMode == 0)
            {
                if (!Item.GetRoom().GetGameMap().itemCanBePlacedHere(Item.GetX, Item.GetY))
                {
                    return;
                }
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();
            Item.GetRoom().GetGameMap().updateMapForItem(Item);

            //Item.GetRoom().GenerateMaps();
        }
    }
}