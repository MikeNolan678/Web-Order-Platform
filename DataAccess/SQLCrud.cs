﻿using DataAccess.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DataAccess
{
    public class SQLCrud
    {
        private SQLDataAccess db = new SQLDataAccess();
        private readonly string _connectionString;

        public SQLCrud(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InventoryModel> GetAllInventory()
        {
            string sql = "select * from dbo.OnHandInventory";

            return db.LoadData<InventoryModel, dynamic>(sql, new { }, _connectionString);
        }

        public List<InventoryModel> GetInventoryByItem(string upcCode)
        {
            string sql = "select * from dbo.OnHandInventory where UPC = @upcCode";

            return db.LoadData<InventoryModel, dynamic>(sql, new { upcCode = upcCode }, _connectionString);
        }

        public (List<InventoryModel> inventoryExists, List<InventoryModel> inventoryNotExists) SplitIncomingInventory(List<InventoryModel> incomingInventory)
        {
            List<InventoryModel> inventoryExists = new List<InventoryModel>();
            List<InventoryModel> inventoryNotExists = new List<InventoryModel>();

            string sql = "select UPC from dbo.OnHandInventory";

            var existingItems = new HashSet<string>(db.LoadData<string, dynamic>(sql, new { }, _connectionString));

            foreach (var item in incomingInventory)
            {
                if (existingItems.Contains(item.UPC))
                {
                    inventoryExists.Add(item);
                }
                else
                {
                    inventoryNotExists.Add(item);
                }
            }
            
            return (inventoryExists, inventoryNotExists);
        }

        public void BulkUpdateInventory(List<InventoryModel> inventoryExists, List<InventoryModel> inventoryNotExists)
        {
            string sql = "UPDATE dbo.OnHandInventory SET Quantity = @Quantity WHERE UPC = @UPC";
            db.BulkSaveData(sql, inventoryExists, _connectionString);

            sql = "INSERT INTO dbo.OnHandInventory (UPC, Quantity, Warehouse) VALUES(@UPC, @Quantity, @Warehouse)";
            db.BulkSaveData(sql, inventoryNotExists, _connectionString);
        }

    }
}