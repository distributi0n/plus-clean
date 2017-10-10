namespace Plus.HabboHotel.Items
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class ItemDataManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Items.ItemDataManager");
        public Dictionary<int, ItemData> _gifts; //<SpriteId, Item>

        public Dictionary<int, ItemData> _items;

        public ItemDataManager()
        {
            _items = new Dictionary<int, ItemData>();
            _gifts = new Dictionary<int, ItemData>();
        }

        public void Init()
        {
            if (_items.Count > 0)
            {
                _items.Clear();
            }
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `furniture`");
                var ItemData = dbClient.GetTable();
                if (ItemData != null)
                {
                    foreach (DataRow Row in ItemData.Rows)
                    {
                        try
                        {
                            var id = Convert.ToInt32(Row["id"]);
                            var spriteID = Convert.ToInt32(Row["sprite_id"]);
                            var itemName = Convert.ToString(Row["item_name"]);
                            var PublicName = Convert.ToString(Row["public_name"]);
                            var type = Row["type"].ToString();
                            var width = Convert.ToInt32(Row["width"]);
                            var length = Convert.ToInt32(Row["length"]);
                            var height = Convert.ToDouble(Row["stack_height"]);
                            var allowStack = PlusEnvironment.EnumToBool(Row["can_stack"].ToString());
                            var allowWalk = PlusEnvironment.EnumToBool(Row["is_walkable"].ToString());
                            var allowSit = PlusEnvironment.EnumToBool(Row["can_sit"].ToString());
                            var allowRecycle = PlusEnvironment.EnumToBool(Row["allow_recycle"].ToString());
                            var allowTrade = PlusEnvironment.EnumToBool(Row["allow_trade"].ToString());
                            var allowMarketplace = Convert.ToInt32(Row["allow_marketplace_sell"]) == 1;
                            var allowGift = Convert.ToInt32(Row["allow_gift"]) == 1;
                            var allowInventoryStack = PlusEnvironment.EnumToBool(Row["allow_inventory_stack"].ToString());
                            var interactionType = InteractionTypes.GetTypeFromString(Convert.ToString(Row["interaction_type"]));
                            var behaviourData = Convert.ToInt32(Row["behaviour_data"]);
                            var cycleCount = Convert.ToInt32(Row["interaction_modes_count"]);
                            var vendingIDS = Convert.ToString(Row["vending_ids"]);
                            var heightAdjustable = Convert.ToString(Row["height_adjustable"]);
                            var EffectId = Convert.ToInt32(Row["effect_id"]);
                            var IsRare = PlusEnvironment.EnumToBool(Row["is_rare"].ToString());
                            var ExtraRot = PlusEnvironment.EnumToBool(Row["extra_rot"].ToString());
                            if (!_gifts.ContainsKey(spriteID))
                            {
                                _gifts.Add(spriteID,
                                    new ItemData(id,
                                        spriteID,
                                        itemName,
                                        PublicName,
                                        type,
                                        width,
                                        length,
                                        height,
                                        allowStack,
                                        allowWalk,
                                        allowSit,
                                        allowRecycle,
                                        allowTrade,
                                        allowMarketplace,
                                        allowGift,
                                        allowInventoryStack,
                                        interactionType,
                                        behaviourData,
                                        cycleCount,
                                        vendingIDS,
                                        heightAdjustable,
                                        EffectId,
                                        IsRare,
                                        ExtraRot));
                            }
                            if (!_items.ContainsKey(id))
                            {
                                _items.Add(id,
                                    new ItemData(id,
                                        spriteID,
                                        itemName,
                                        PublicName,
                                        type,
                                        width,
                                        length,
                                        height,
                                        allowStack,
                                        allowWalk,
                                        allowSit,
                                        allowRecycle,
                                        allowTrade,
                                        allowMarketplace,
                                        allowGift,
                                        allowInventoryStack,
                                        interactionType,
                                        behaviourData,
                                        cycleCount,
                                        vendingIDS,
                                        heightAdjustable,
                                        EffectId,
                                        IsRare,
                                        ExtraRot));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.ReadKey();

                            //Logging.WriteLine("Could not load item #" + Convert.ToInt32(Row[0]) + ", please verify the data is okay.");
                        }
                    }
                }
            }

            log.Info("Item Manager -> LOADED");
        }

        public bool GetItem(int Id, out ItemData Item)
        {
            if (_items.TryGetValue(Id, out Item))
            {
                return true;
            }

            return false;
        }

        public ItemData GetItemByName(string name)
        {
            foreach (var entry in _items)
            {
                var item = entry.Value;
                if (item.ItemName == name)
                {
                    return item;
                }
            }

            return null;
        }

        public bool GetGift(int SpriteId, out ItemData Item)
        {
            if (_gifts.TryGetValue(SpriteId, out Item))
            {
                return true;
            }

            return false;
        }
    }
}