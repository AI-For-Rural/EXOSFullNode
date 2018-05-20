﻿using System;
using System.Collections.Generic;

namespace Stratis.Patricia
{
    /// <summary>
    /// Acts as a very basic in-memory database. Used for testing.
    /// </summary>
    internal class MemoryDictionarySource : ISource<byte[], byte[]>
    {
        public Dictionary<byte[], byte[]> Db { get; private set; }

        public MemoryDictionarySource()
        {
            this.Db = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
        }

        public void Delete(byte[] key)
        {
            this.Db.Remove(key);
        }

        public bool Flush()
        {
            throw new NotImplementedException();
        }

        public byte[] Get(byte[] key)
        {
            if (this.Db.ContainsKey(key))
                return this.Db[key];
            return null;
        }

        public void Put(byte[] key, byte[] val)
        {
            this.Db[key] = val;
        }
    }
}
