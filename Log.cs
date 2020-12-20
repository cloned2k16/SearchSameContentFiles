using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSameFiles {
    class                               Log {
        internal static void            dbg                                 (String fmt, params object[] args)                              {
            Console.WriteLine(fmt, args);
        }
        internal static void            err                                 (String fmt, params object[] args)                              {
            Console.WriteLine("ERROR: " + fmt, args);
        }
    }
}
