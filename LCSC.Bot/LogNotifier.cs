using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Discord
{
    internal static class LogNotifier
    {
        public static void Notify(string message)
            => Console.WriteLine(message);
    }
}