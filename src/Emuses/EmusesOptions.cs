﻿using System.Collections.Generic;

namespace Emuses
{
    public class EmusesOptions
    {
        public string OpenSessionPage { get; set; }

        public string SessionExpiredPage { get; set; }

        public List<string> NoSessionAccessPages { get; set; }

        public ISessionStorage Storage { get; set; }
    }
}