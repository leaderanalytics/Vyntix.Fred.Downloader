﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.Blazor.Shared
{
    public class RowOpResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class RowOpResult<T> : RowOpResult
    { 
        public T Item { get; set; }

        public RowOpResult() { }

        public RowOpResult(T item, bool success = true)
        {
            this.Success = success;
            this.Item = item;
        }
    }
}
