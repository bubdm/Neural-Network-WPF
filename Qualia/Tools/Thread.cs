﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class Threads
    {
        [DllImport("kernel32")]
        static extern int GetCurrentThreadId();

        public enum Processor
        {
            Proc0 = 1,
            Proc1 = 2,
            Proc2 = 4,
            Proc3 = 8,
            Proc4 = 16,
            Proc5 = 32,
            Proc6 = 64,
            Proc7 = 128,

            All = Proc0 | Proc1 | Proc2 | Proc3 | Proc4 | Proc5 | Proc6 | Proc7
        }

        public static void SetProcessorAffinity(Processor processors)
        {
            foreach (ProcessThread pt in Process.GetCurrentProcess().Threads)
            {
                int utid = GetCurrentThreadId();
                if (utid == pt.Id)
                {
                    pt.ProcessorAffinity = (IntPtr)processors;
                }
            }
        }
    }
}
