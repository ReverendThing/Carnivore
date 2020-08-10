using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace sLYNCy_WPF
{
    public static class CollectionUpdates
    {
        public static void AddHostnameRecord(Hostnames host, ObservableCollection<Hostnames> enumeratedHostnames, MainWindow UI)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (enumeratedHostnames.Any(p => p.Hostname == host.Hostname && p.ipAddress.ToString() == host.ipAddress.ToString()))
                {
                    //Could add some sexier code here to update/add/overwrite if not all values match
                }
                else
                {
                    enumeratedHostnames.Add(host);
                    UI.saveEnumeratedHostnames(null, Enums.SaveType.autoLog);
                }
            });
        }

    }
}
