using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ServerCoreTCP.Core
{
    internal static class Global
    {
        internal static Stopwatch g_watch = new Stopwatch();

        internal static void Init()
        {
            g_watch.Start();
        }

        internal static void Clear()
        {
            g_watch.Stop();
        }
    }
}
