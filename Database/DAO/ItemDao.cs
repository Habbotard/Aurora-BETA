﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AuroraEmu.DI.Database.DAO;
using AuroraEmu.Game.Clients;
using AuroraEmu.Game.Catalog.Models;
using AuroraEmu.Game.Items.Models;
using AuroraEmu.Game.Items.Models.Dimmer;
using AuroraEmu.Game.Players.Models;
using MySql.Data.MySqlClient;

namespace AuroraEmu.Database.DAO
{
    public class ItemDao : IItemDao
    {
        public void ReloadTemplates(Dictionary<int, ItemDefinition> items)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM item_definitions;");
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        items.Add(reader.GetInt32("id"), new ItemDefinition(reader));
            }
        }
        
        public void GiveItem(Client client, CatalogProduct product, string extraData)
        {
            int id = -1;

            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("INSERT INTO items (owner_id, definition_id, data) VALUES (@ownerId, @definitionId, @data)");
                dbConnection.AddParameter("@ownerId", client.Player.Id);
                dbConnection.AddParameter("@definitionId", product.TemplateId);
                dbConnection.AddParameter("@data", extraData);
                id = dbConnection.Insert();
            }

            if (id > 0 && client.Items != null)
            {
                client.Items.Add(id, new Item(id, client.Player.Id, product.TemplateId, extraData));
            }
        }
        
        public void GiveItem(Client client, ItemDefinition template, string extraData)
        {
            int id = -1;

            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("INSERT INTO items (owner_id, definition_id, data) VALUES (@ownerId, @definitionId, @data)");
                dbConnection.AddParameter("@ownerId", client.Player.Id);
                dbConnection.AddParameter("@definitionId", template.Id);
                dbConnection.AddParameter("@data", extraData);
                id = dbConnection.Insert();
            }

            if (id > 0 && client.Items != null)
            {
                client.Items.Add(id, new Item(id, client.Player.Id, template.Id, extraData));
            }
        }
        
        public void GiveItem(Player targetUser, CatalogProduct product, string extraData)
        {
            int id = -1;

            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("INSERT INTO items (owner_id, definition_id, data) VALUES (@ownerId, @definitionId, @data)");
                dbConnection.AddParameter("@ownerId", targetUser.Id);
                dbConnection.AddParameter("@definitionId", product.TemplateId);
                dbConnection.AddParameter("@data", extraData);
                id = dbConnection.Insert();
            }

            Client client = Engine.Locator.ClientController.GetClientByHabbo(targetUser.Id);

            if (id > 0 && client != null && client.Items != null)
            {
                client.Items.Add(id, new Item(id, client.Player.Id, product.TemplateId, extraData));
            }
        }
        
        public int GiveItem(Player targetUser, ItemDefinition template, string extraData)
        {
            int id = -1;

            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("INSERT INTO items (owner_id, definition_id, data) VALUES (@ownerId, @definitionId, @data)");
                dbConnection.AddParameter("@ownerId", targetUser.Id);
                dbConnection.AddParameter("@definitionId", template.Id);
                dbConnection.AddParameter("@data", extraData);
                id = dbConnection.Insert();
            }
            
            Client client = Engine.Locator.ClientController.GetClientByHabbo(targetUser.Id);

            if (id > 0 && client != null && client.Items != null)
            {
                client.Items.Add(id, new Item(id, client.Player.Id, template.Id, extraData));
            }

            return id;
        }
        
        public ConcurrentDictionary<int, Item> GetItemsInRoom(int roomId)
        {
            ConcurrentDictionary<int, Item> items = new ConcurrentDictionary<int, Item>();
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM items WHERE room_id = @roomId");
                dbConnection.AddParameter("@roomId", roomId);
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        items.TryAdd(reader.GetInt32("id"), new Item(reader));
            }
            return items;
        }
        
        public Dictionary<int, Item> GetItemsFromOwner(int ownerId)
        {
            Dictionary<int, Item> items = new Dictionary<int, Item>();

            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM items WHERE owner_id = @ownerId AND room_id IS NULL");
                dbConnection.AddParameter("@ownerId", ownerId);
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        items.Add(reader.GetInt32("id"), new Item(reader));
            }

            return items;
        }

        public void UpdateItem(int itemId, int x, int y, int rot, object roomId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("UPDATE items SET room_id = @roomId, x = @x, y = @y, rotation = @rot WHERE id = @itemId LIMIT 1");
                dbConnection.AddParameter("@roomId", roomId);
                dbConnection.AddParameter("@x", x);
                dbConnection.AddParameter("@y", y);
                dbConnection.AddParameter("@rot", rot);
                dbConnection.AddParameter("@itemId", itemId);
                dbConnection.Execute();
            }
        }

        public void DeleteItem(int itemId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("DELETE FROM `items` WHERE `id` = @itemid");
                dbConnection.AddParameter("@itemid", itemId);
                dbConnection.Execute();
            }
        }

        public void AddFloorItem(int itemId, int x, int y, int rot, int roomId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("UPDATE items SET room_id = @roomId, x = @x, y = @y, rotation = @rot WHERE id = @itemId LIMIT 1");
                dbConnection.AddParameter("@roomId", roomId);
                dbConnection.AddParameter("@x", x);
                dbConnection.AddParameter("@y", y);
                dbConnection.AddParameter("@rot", rot);
                dbConnection.AddParameter("@itemId", itemId);
                dbConnection.Execute();
            }
        }

        public void AddWallItem(int itemId, string wallposition, int roomId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("UPDATE items SET room_id = @roomId, wallposition = @wallposition WHERE id = @itemId LIMIT 1");
                dbConnection.AddParameter("@roomId", roomId);
                dbConnection.AddParameter("@wallposition", wallposition);
                dbConnection.AddParameter("@itemId", itemId);
                dbConnection.Execute();
            }
        }

        public void UpdateItemData(int itemId, string data)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("UPDATE items SET data = @extraData WHERE id = @id LIMIT 1");
                dbConnection.AddParameter("@extraData", data);
                dbConnection.AddParameter("@id", itemId);
                dbConnection.Execute();
            }
        }

        public void UpdateDimmerPreset(DimmerData data)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("UPDATE room_dimmer SET enabled = @enabled, current_preset = @cur_preset, preset_one = @pres_one, preset_two = @pres_two, preset_three = @pres_three WHERE item_id = @item_id");
                dbConnection.AddParameter("@enabled", data.Enabled ? 1 : 0);
                dbConnection.AddParameter("@cur_preset", data.CurrentPreset);
                dbConnection.AddParameter("@pres_one", data.Presets[0].PresetData());
                dbConnection.AddParameter("@pres_two", data.Presets[1].PresetData());
                dbConnection.AddParameter("@pres_three", data.Presets[2].PresetData());
                dbConnection.AddParameter("@item_id", data.ItemId);
                dbConnection.Execute();
            }
        }

        public void CreatePresent(int definitionId, int playerId, int giftId, string data)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("INSERT INTO `item_presents` VALUES (@giftid, @playerid, @definitionid, @data)");
                dbConnection.AddParameter("@giftid", giftId);
                dbConnection.AddParameter("@playerid", playerId);
                dbConnection.AddParameter("@definitionid", definitionId);
                dbConnection.AddParameter("@data", data);
                dbConnection.Execute();
            }
        }

        public (int, string) GetPresent(int presentId, int playerId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT `definition_id`, `data` FROM `item_presents` WHERE `furni_id` = @presentid AND `player_id` = @playerid LIMIT 1");
                dbConnection.AddParameter("@presentid", presentId);
                dbConnection.AddParameter("@playerid", playerId);

                using (var reader = dbConnection.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var definitionId = reader.GetInt32(0);
                        var data = reader.GetString(1);
                
                        return (definitionId, data);
                    }
                }
            }
        
            return (-1, string.Empty);
        }

        public void DeletePresent(int presentId)
        {
            using (DatabaseConnection dbConnection = Engine.Locator.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("DELETE FROM `item_presents` WHERE `furni_id` = @presentid");
                dbConnection.AddParameter("@presentid", presentId);
                dbConnection.Execute();
            }
        }
    }
}