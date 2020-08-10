using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class CredentialsRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string password;
        private string token;
        private string username;
        private string userRaw;
        private string accountDisabled;
        private string sipEnabled;
        private string serverError;
        private string passwordExpired;
        private string mFA;
        private MicrosoftService service;

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public string MFA
        {
            get { return mFA; }
            set
            {
                mFA = value;
                OnPropertyChanged("MFA");
            }
        }
        public string UserRaw
        {
            get { return userRaw; }
            set
            {
                userRaw = value;
                OnPropertyChanged("UserRaw");
            }
        }
        public string Token
        {
            get { return token; }
            set
            {
                token = value;
                OnPropertyChanged("Token");
            }
        }
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged("Username");
            }
        }
        public string AccountDisabled
        {
            get { return accountDisabled; }
            set
            {
                accountDisabled = value;
                OnPropertyChanged("AccountDisabled");
            }
        }
        public string SipEnabled
        {
            get { return sipEnabled; }
            set
            {
                sipEnabled = value;
                OnPropertyChanged("SipEnabled");
            }
        }
        public string ServerError
        {
            get { return serverError; }
            set
            {
                serverError = value;
                OnPropertyChanged("ServerError");
            }
        }
        public string PasswordExpired
        {
            get { return passwordExpired; }
            set
            {
                passwordExpired = value;
                OnPropertyChanged("PasswordExpired");
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

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
