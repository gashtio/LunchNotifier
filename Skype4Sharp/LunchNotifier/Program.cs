using System;
using System.Collections.Generic;
using System.Linq;
using Skype4Sharp;
using Skype4Sharp.Events;
using Skype4Sharp.Auth;
using Skype4Sharp.Helpers;
using Skype4Sharp.Enums;
using System.Threading;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LunchNotifier
{
    class Program
    {
        public void Run()
        {
            List<ILunchProvider> lunchProviders = new List<ILunchProvider>();
            // Add lunch providers

            List<ILunchNotifyTarget> lunchNotifyTargets = new List<ILunchNotifyTarget>();
            // Add lunch notify targets

            foreach (var provider in lunchProviders)
            {
                var info = provider.GetLunchInfo();
                foreach (var target in lunchNotifyTargets)
                {
                    target.BroadcastLunchInfo(info);
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
