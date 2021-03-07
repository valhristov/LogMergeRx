using System.Windows.Threading;

namespace LogMergeRxTests
{
    public static class DispatcherUtil
    {
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame),
                frame);
            Dispatcher.PushFrame(frame);

            static object ExitFrame(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }
    }
}
