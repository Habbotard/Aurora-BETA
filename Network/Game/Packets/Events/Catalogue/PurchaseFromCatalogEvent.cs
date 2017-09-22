﻿using AuroraEmu.Game.Catalog;
using AuroraEmu.Game.Clients;
using AuroraEmu.Network.Game.Packets.Composers.Catalogue;
using AuroraEmu.Network.Game.Packets.Composers.Inventory;
using AuroraEmu.Network.Game.Packets.Composers.Users;

namespace AuroraEmu.Network.Game.Packets.Events.Catalogue
{
    class PurchaseFromCatalogEvent : IPacketEvent
    {
        public void Run(Client client, MessageEvent msgEvent)
        {
            var pageId = msgEvent.ReadVL64();
            var productId = msgEvent.ReadVL64();
            var data = msgEvent.ReadString();
            var unk = msgEvent.ReadVL64(); // TODO: Find this out perhaps..?

            var product = Engine.MainDI.CatalogController.GetProduct(productId);

            if (product == null) 
                return;
            
            var shortOnCoins = product.PriceCoins > client.Player.Coins;
            var shortOnPixels = product.PricePixels > client.Player.Pixels;

            if (shortOnCoins || shortOnPixels)
            {
                client.SendComposer(new NotEnoughBalanceMessageComposer(shortOnCoins, shortOnPixels));
            }
            else
            {
                if (product.PriceCoins > 0)
                {
                    client.DecreaseCredits(product.PriceCoins);
                    client.QueueComposer(new CreditBalanceMessageComposer(client.Player.Coins));
                }

                if (product.PricePixels > 0)
                {
                    client.DecreasePixels(product.PricePixels);
                    client.QueueComposer(new HabboActivityPointNotificationMessageComposer(client.Player.Pixels, 0));
                }

                if (product.IsDeal)
                {
                    var dealItems = Engine.MainDI.CatalogController.GetDeal(product.DealId);

                    foreach (var dealItem in dealItems)
                    {
                        for (var i = 0; i < dealItem.Amount; i++)
                        {
                            Engine.MainDI.ItemController.GiveItem(client, dealItem.Template, string.Empty);
                        }
                    }
                }
                else
                {
                    var extraData = Engine.MainDI.CatalogController.GenerateExtraData(product, data).Replace("{USERNAME}", client.Player.Username);
                    
                    for (var i = 0; i < product.Amount; i++)
                    {
                        Engine.MainDI.ItemController.GiveItem(client, product, extraData);
                    }
                }

                client.QueueComposer(new PurchaseOKMessageComposer(product));

                if (client.Items != null)
                    client.QueueComposer(new FurniListUpdateComposer());

                client.Flush();
            }
        }
    }
}
