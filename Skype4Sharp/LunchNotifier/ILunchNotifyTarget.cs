namespace LunchNotifier
{
    interface ILunchNotifyTarget
    {
        void BroadcastLunchInfo(LunchInfo info);
    }
}
