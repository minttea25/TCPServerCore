﻿using System.Diagnostics;

namespace ServerCoreTCP.Core
{
    /// <summary>
    /// This class will be initialized when the service starts.
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// JobTimer and PacketSession uses this stopwatch globally.
        /// </summary>
        public static Stopwatch G_Stopwatch = new Stopwatch();

        static int _refCount = 0;
        static bool _init = false;
        static object _lock = new object();

        /// <summary>
        /// It will be called when the service starts.
        /// </summary>
        internal static void Init()
        {
            lock (_lock)
            {
                _refCount++;
                if (_init == true) return;

                _init = true;
            }
            
            // init
            {
                G_Stopwatch.Start();
                // Add
            }
        }

        internal static void Clear()
        {
            lock (_lock)
            {
                _refCount--;
                if (_refCount != 0) return;
            }

            // clean up
            {
                G_Stopwatch.Stop();
                // Add
            }
        }
    }
}
