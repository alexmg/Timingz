using System.Diagnostics;

namespace Timingz
{
    public static class ActivityExtensions
    {
        private static readonly object CustomValue = new();

        public static Activity AddServerTiming(this Activity activity, string description = null)
        {
            if (activity != null && !string.IsNullOrEmpty(description))
                activity.DisplayName = description;

            activity?.SetCustomProperty(ServerTimingProcessor.CustomPropertyKey, CustomValue);
            return activity;
        }
    }
}