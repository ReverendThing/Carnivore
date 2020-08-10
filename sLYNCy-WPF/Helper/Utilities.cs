using System;
using System.Windows.Threading;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public static class Utilities
    {
        public static string StripUser(string username)
        {
            if (username.Contains("\\"))
            {
                return username.Split('\\')[1];
            }
            else if (username.Contains("@"))
            {
                return username.Split('@')[0];
            }
            return null;
        }


    }

    public class URLObject
    {
        private string url;
        private URLType type;

        public string Url { get => url; set => url = value; }
        public URLType Type { get => type; set => type = value; }
    }

    public class CounterUpdate
    {
        private DispatcherTimer dispatchTimerUserEnum;
        private DispatcherTimer dispatchTimerPassSpray;

        public void Initialise()
        {
            dispatchTimerUserEnum = new DispatcherTimer();
            dispatchTimerPassSpray = new DispatcherTimer();
        }

        public void startUpdate(SendingWindow sender, MainWindow UI)
        {
            switch (sender)
            {
                case SendingWindow.PasswordSpray:
                    UI.updatePassSprayText("Current Position: 0/0");
                    dispatchTimerPassSpray.Tick += new EventHandler(UI.dispatcherTimerPassSpray_Tick);
                    dispatchTimerPassSpray.Interval = new TimeSpan(0, 0, 0, 0, 10);
                    dispatchTimerPassSpray.Start();
                    break;
                case SendingWindow.UserEnum:
                    UI.updateUserEnumText("Current Position: 0/0");
                    dispatchTimerUserEnum.Tick += new EventHandler(UI.dispatcherTimerUserEnum_Tick);
                    dispatchTimerUserEnum.Interval = new TimeSpan(0, 0, 0, 0, 10);
                    dispatchTimerUserEnum.Start();
                    break;
            }
        }

        public void stopUpdate(SendingWindow sender)
        {
            switch (sender)
            {
                case SendingWindow.PasswordSpray:
                    dispatchTimerPassSpray.Stop();
                    break;
                case SendingWindow.UserEnum:
                    dispatchTimerUserEnum.Stop();
                    break;
            }
        }


    }
}
