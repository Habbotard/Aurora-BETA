﻿using AuroraEmu.DI.Database.DAO;
using AuroraEmu.Game.Catalog;
using AuroraEmu.Game.Catalog.Voucher;
using System.Collections.Generic;

namespace AuroraEmu.Database.DAO
{
    public class CatalogDao : ICatalogDao
    {
        public Dictionary<int, CatalogPage> ReloadCatalogPage(Dictionary<int, CatalogPage> pages)
        {
            pages.Clear();

            using (DatabaseConnection dbConnection = Engine.MainDI.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM catalog_pages;");
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        pages.Add(reader.GetInt32("id"), new CatalogPage(reader));

                dbConnection.SetQuery("SELECT page_id,type,value FROM catalog_pages_data;");
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        pages[reader.GetInt32("page_id")].Data[reader.GetString("type")].Add(reader.GetString("value"));
            }
            return pages;
        }

        public Dictionary<int, CatalogProduct> ReloadProducts(Dictionary<int, CatalogProduct> products)
        {
            using (DatabaseConnection dbConnection = Engine.MainDI.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM catalog_products;");
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        products.Add(reader.GetInt32("id"), new CatalogProduct(reader));
            }

            return products;
        }
        
        public Dictionary<int, List<CatalogDealItem>> ReloadDeals(Dictionary<int, List<CatalogDealItem>> deals)
        {
            using (DatabaseConnection dbConnection = Engine.MainDI.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM catalog_deals;");
                using (var reader = dbConnection.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int dealId = reader.GetInt32("id");

                        if (!deals.TryGetValue(dealId, out List<CatalogDealItem> item))
                            deals.Add(dealId, new List<CatalogDealItem>());

                        deals[dealId].Add(new CatalogDealItem(reader));
                    }
                }
            }

            return deals;
        }

        public Dictionary<string, Voucher> ReloadVouchers(Dictionary<string, Voucher> vouchers)
        {
            using (DatabaseConnection dbConnection = Engine.MainDI.ConnectionPool.PopConnection())
            {
                dbConnection.SetQuery("SELECT * FROM catalog_vouchers;");
                using (var reader = dbConnection.ExecuteReader())
                    while (reader.Read())
                        vouchers.Add(reader.GetString("voucher"), new Voucher(reader));
            }

            return vouchers;
        }
    }
}
