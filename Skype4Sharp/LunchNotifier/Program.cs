using System.Collections.Generic;

namespace LunchNotifier
{
    class Program
    {
        public void Run()
        {
            List<ILunchProvider> lunchProviders = new List<ILunchProvider>();
            lunchProviders.Add(new Providers.DummyLunchProvider());

            List<ILunchNotifyTarget> lunchNotifyTargets = new List<ILunchNotifyTarget>();
            lunchNotifyTargets.Add(new Targets.SkypeNotifier("cred.json"));

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
