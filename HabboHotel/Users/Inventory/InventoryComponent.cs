namespace Plus.HabboHotel.Users.Inventory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Bots;
    using Communication.Packets.Outgoing.Inventory.Furni;
    using GameClients;
    using Items;
    using Pets;
    using Rooms.AI;

    public class InventoryComponent
    {
        private readonly ConcurrentDictionary<int, Bot> _botItems;
        private readonly ConcurrentDictionary<int, Item> _floorItems;
        private readonly ConcurrentDictionary<int, Pet> _petsItems;
        private readonly int _userId;
        private readonly ConcurrentDictionary<int, Item> _wallItems;
        private GameClient _client;

        public InventoryComponent(int UserId, GameClient Client)
        {
            _client = Client;
            _userId = UserId;
            _floorItems = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _petsItems = new ConcurrentDictionary<int, Pet>();
            _botItems = new ConcurrentDictionary<int, Bot>();
            Init();
        }

        public IEnumerable<Item> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        public IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);

        public void Init()
        {
            if (_floorItems.Count > 0)
            {
                _floorItems.Clear();
            }
            if (_wallItems.Count > 0)
            {
                _wallItems.Clear();
            }
            if (_petsItems.Count > 0)
            {
                _petsItems.Clear();
            }
            if (_botItems.Count > 0)
            {
                _botItems.Clear();
            }
            var Items = ItemLoader.GetItemsForUser(_userId);
            foreach (var Item in Items.ToList())
            {
                if (Item.IsFloorItem)
                {
                    if (!_floorItems.TryAdd(Item.Id, Item))
                    {
                    }
                }
                else if (Item.IsWallItem)
                {
                    if (!_wallItems.TryAdd(Item.Id, Item))
                    {
                    }
                }
            }

            var Pets = PetLoader.GetPetsForUser(Convert.ToInt32(_userId));
            foreach (var Pet in Pets)
            {
                if (!_petsItems.TryAdd(Pet.PetId, Pet))
                {
                    Console.WriteLine("Error whilst loading pet x1: " + Pet.PetId);
                }
            }

            var Bots = BotLoader.GetBotsForUser(Convert.ToInt32(_userId));
            foreach (var Bot in Bots)
            {
                if (!_botItems.TryAdd(Bot.Id, Bot))
                {
                    Console.WriteLine("Error whilst loading bot x1: " + Bot.Id);
                }
            }
        }

        public void ClearItems()
        {
            UpdateItems(true);
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM items WHERE room_id='0' AND user_id = " + _userId); //Do join 
            }
            _floorItems.Clear();
            _wallItems.Clear();
            if (_client != null)
            {
                _client.SendPacket(new FurniListUpdateComposer());
            }
        }

        public void SetIdleState()
        {
            if (_botItems != null)
            {
                _botItems.Clear();
            }
            if (_petsItems != null)
            {
                _petsItems.Clear();
            }
            if (_floorItems != null)
            {
                _floorItems.Clear();
            }
            if (_wallItems != null)
            {
                _wallItems.Clear();
            }
            _client = null;
        }

        public void UpdateItems(bool FromDatabase)
        {
            if (FromDatabase)
            {
                Init();
            }
            if (_client != null)
            {
                _client.SendPacket(new FurniListUpdateComposer());
            }
        }

        public Item GetItem(int Id)
        {
            if (_floorItems.ContainsKey(Id))
            {
                return _floorItems[Id];
            }
            if (_wallItems.ContainsKey(Id))
            {
                return _wallItems[Id];
            }

            return null;
        }

        public Item AddNewItem(int Id, int BaseItem, string ExtraData, int Group, bool ToInsert, bool FromRoom, int LimitedNumber,
            int LimitedStack)
        {
            if (ToInsert)
            {
                if (FromRoom)
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `items` SET `room_id` = '0', `user_id` = '" + _userId + "' WHERE `id` = '" +
                                          Id + "' LIMIT 1");
                    }
                }
                else
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        if (Id > 0)
                        {
                            dbClient.RunQuery(
                                "INSERT INTO `items` (`id`,`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" +
                                Id +
                                "', '" +
                                BaseItem +
                                "', '" +
                                _userId +
                                "', '" +
                                LimitedNumber +
                                "', '" +
                                LimitedStack +
                                "')");
                        }
                        else
                        {
                            dbClient.SetQuery(
                                "INSERT INTO `items` (`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" +
                                BaseItem +
                                "', '" +
                                _userId +
                                "', '" +
                                LimitedNumber +
                                "', '" +
                                LimitedStack +
                                "')");
                            Id = Convert.ToInt32(dbClient.InsertQuery());
                        }
                        SendNewItems(Convert.ToInt32(Id));
                        if (Group > 0)
                        {
                            dbClient.RunQuery("INSERT INTO `items_groups` VALUES (" + Id + ", " + Group + ")");
                        }
                        if (!string.IsNullOrEmpty(ExtraData))
                        {
                            dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extradata WHERE `id` = '" + Id + "' LIMIT 1");
                            dbClient.AddParameter("extradata", ExtraData);
                            dbClient.RunQuery();
                        }
                    }
                }
            }
            var ItemToAdd = new Item(Id, 0, BaseItem, ExtraData, 0, 0, 0, 0, _userId, Group, LimitedNumber, LimitedStack,
                string.Empty);
            if (UserHoldsItem(Id))
            {
                RemoveItem(Id);
            }
            if (ItemToAdd.IsWallItem)
            {
                _wallItems.TryAdd(ItemToAdd.Id, ItemToAdd);
            }
            else
            {
                _floorItems.TryAdd(ItemToAdd.Id, ItemToAdd);
            }
            return ItemToAdd;
        }

        private bool UserHoldsItem(int itemID)
        {
            if (_floorItems.ContainsKey(itemID))
            {
                return true;
            }
            if (_wallItems.ContainsKey(itemID))
            {
                return true;
            }

            return false;
        }

        public void RemoveItem(int Id)
        {
            if (GetClient() == null)
            {
                return;
            }

            if (GetClient().GetHabbo() == null || GetClient().GetHabbo().GetInventoryComponent() == null)
            {
                GetClient().Disconnect();
            }
            if (_floorItems.ContainsKey(Id))
            {
                Item ToRemove = null;
                _floorItems.TryRemove(Id, out ToRemove);
            }
            if (_wallItems.ContainsKey(Id))
            {
                Item ToRemove = null;
                _wallItems.TryRemove(Id, out ToRemove);
            }
            GetClient().SendPacket(new FurniListRemoveComposer(Id));
        }

        private GameClient GetClient() => PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(_userId);

        public void SendNewItems(int Id)
        {
            _client.SendPacket(new FurniListNotificationComposer(Id, 1));
        }

        public ICollection<Pet> GetPets() => _petsItems.Values;

        public bool TryAddPet(Pet Pet)
        {
            //TODO: Sort this mess.
            Pet.RoomId = 0;
            Pet.PlacedInRoom = false;
            return _petsItems.TryAdd(Pet.PetId, Pet);
        }

        public bool TryRemovePet(int PetId, out Pet PetItem)
        {
            if (_petsItems.ContainsKey(PetId))
            {
                return _petsItems.TryRemove(PetId, out PetItem);
            }

            PetItem = null;
            return false;
        }

        public bool TryGetPet(int PetId, out Pet Pet)
        {
            if (_petsItems.ContainsKey(PetId))
            {
                return _petsItems.TryGetValue(PetId, out Pet);
            }

            Pet = null;
            return false;
        }

        public ICollection<Bot> GetBots() => _botItems.Values;

        public bool TryAddBot(Bot Bot) => _botItems.TryAdd(Bot.Id, Bot);

        public bool TryRemoveBot(int BotId, out Bot Bot)
        {
            if (_botItems.ContainsKey(BotId))
            {
                return _botItems.TryRemove(BotId, out Bot);
            }

            Bot = null;
            return false;
        }

        public bool TryGetBot(int BotId, out Bot Bot)
        {
            if (_botItems.ContainsKey(BotId))
            {
                return _botItems.TryGetValue(BotId, out Bot);
            }

            Bot = null;
            return false;
        }

        public bool TryAddItem(Item item)
        {
            if (item.Data.Type.ToString().ToLower() == "s") // ItemType.FLOOR)
            {
                return _floorItems.TryAdd(item.Id, item);
            }
            if (item.Data.Type.ToString().ToLower() == "i") //ItemType.WALL)
            {
                return _wallItems.TryAdd(item.Id, item);
            }

            throw new InvalidOperationException("Item did not match neither floor or wall item");
        }

        public bool TryAddFloorItem(int itemId, Item item) => _floorItems.TryAdd(itemId, item);

        public bool TryAddWallItem(int itemId, Item item) => _floorItems.TryAdd(itemId, item);

        public ICollection<Item> GetFloorItems() => _floorItems.Values;

        public ICollection<Item> GetWallItems() => _wallItems.Values;
    }
}