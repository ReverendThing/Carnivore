using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class Hostnames : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string name = "";
        private MicrosoftService service = MicrosoftService.nullService;
        private string o365 = "";
        private IPAddress address;
        private URLObject enumURL = new URLObject() { Url = "" };
        private URLObject sprayURL = new URLObject() { Url = "" };
        private string ntlmDomain = "";
        private string oauthDomain = "";
        private string federated = "";
        private bool realLync;


        public string Federated
        {
            get { return federated; }
            set
            {
                federated = value;
                OnPropertyChanged("Federated");
            }
        }
        public bool RealLync
        {
            get { return realLync; }
            set
            {
                realLync = value;
                OnPropertyChanged("RealLync");
            }
        }
        public string NTLMDomain
        {
            get { return ntlmDomain; }
            set
            {
                ntlmDomain = value;
                OnPropertyChanged("NTLMDomain");
            }
        }
        public string OAuthDomain
        {
            get { return oauthDomain; }
            set
            {
                oauthDomain = value;
                OnPropertyChanged("OAuthDomain");
            }
        }
        public URLObject EnumURL
        {
            get { return enumURL; }
            set
            {
                enumURL = value;
                OnPropertyChanged("EnumURL");
            }
        }
        public URLObject SprayURL
        {
            get { return sprayURL; }
            set
            {
                sprayURL = value;
                OnPropertyChanged("SprayURL");
            }
        }
        public string O365
        {
            get { return o365; }
            set
            {
                o365 = value;
                OnPropertyChanged("O365");
            }
        }
        public MicrosoftService Service
        {
            get { return service; }
            set
            {
                service = value;
                OnPropertyChanged("Service");
            }
        }
        public string Hostname
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Hostname");
            }
        }
        public IPAddress ipAddress
        {
            get { return address; }
            set
            {
                address = value;
                OnPropertyChanged("ipAddress");
            }
        }

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static List<Hostnames> HostnameList
        {
            get
            {
                return new List<Hostnames>();
            }
        }
    }

}
