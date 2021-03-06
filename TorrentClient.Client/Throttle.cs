﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TorrentClient.Client
{
    public class Throttle
    {
        public long MaximumSize { get; private set; }
        public TimeSpan MaximumWindow { get; private set; }

        internal struct Item
        {
            public DateTime Time;
            public long Size;
        }

        private readonly object itemLock = new object();
        private readonly List<Item> items = new List<Item>();

        public Throttle(int maxSize, TimeSpan maxWindow)
        {
            MaximumSize = maxSize;
            MaximumWindow = maxWindow;
        }

        public void Add(long size)
        {
            lock (itemLock)
            {
                items.Add(new Item() { Time = DateTime.UtcNow, Size = size });
            }
        }

        public bool IsThrottled
        {
            get
            {
                lock (itemLock)
                {
                    DateTime cutoff = DateTime.UtcNow.Add(-MaximumWindow);
                    items.RemoveAll(x => x.Time < cutoff);
                    return items.Sum(x => x.Size) >= MaximumSize;
                }
            }
        }
    }
}
