﻿using HardwareInventoryService.Models.Models;
using HardwareInventoryService.Modules.Cache.Logic.IRepositories;
using HardwareInventoryService.Modules.Cache.Logic.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInventoryService.Modules.Cache.Logic.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly object _itemsSetLock = new object();

        public HashSet<CacheObject<Item>> _itemsSet;

        public HashSet<CacheObject<Item>> Objects
        {
            get
            {
                lock (this._itemsSetLock)
                {
                    return this._itemsSet;
                }
            }
            set
            {
                lock (this._itemsSetLock)
                {
                    this._itemsSet = value;
                }
            }
        }

        public ItemRepository()
        {
            this._itemsSet = new HashSet<CacheObject<Item>>();
        }

        public void AddItem(Item item)
        {
            this._itemsSet.AddExtension(item);
        }

        public List<Item> GetItems()
        {
            List<Item> list = new List<Item>();

            foreach (var item in this._itemsSet)
            {
                list.Add(item.Object);
            }

            return list;
        }
    }
}