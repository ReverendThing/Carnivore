using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class ServiceInterface
    {
        //These are objects for each specific service - that also contain "EnumerateUsers" and "PasswordSpray" that are then
            //passed off to the UsernameEnumeration or PasswordSpray classes
        public MainWindow UI;
        public Hostnames host;
        public UsernameEnumeration enumerator;
        public PasswordSpray sprayer;
        public PostCompromise postCompromise;
        public MicrosoftService service;
        private string MSISSamlRequest = "";
        public int totalUsernames;
        public int remainingUsernames;

        public void Pause(SendingWindow sender)
        {
            switch (sender)
            {
                case SendingWindow.UserEnum:
                    if (enumerator != null)
                    {
                        UI.unlockUI();
                        enumerator.Pause(UI);
                    }
                    break;
                case SendingWindow.PasswordSpray:
                    if (sprayer != null)
                    {
                        UI.unlockUI();
                        sprayer.Pause(UI);
                    }
                    break;
            }
 
        }

        public void Resume(SendingWindow sender)
        {
            switch (sender)
            {
                case SendingWindow.UserEnum:
                    if (enumerator != null)
                    {
                        UI.lockUI(SendingWindow.UserEnum);
                        enumerator.Resume(UI);
                    }
                    break;
                case SendingWindow.PasswordSpray:
                    if (sprayer != null)
                    {
                        UI.lockUI(SendingWindow.PasswordSpray);
                        sprayer.Resume(UI);
                    }
                    break;
            }
            
           
        }

        public void Stop(SendingWindow sender)
        {
            switch (sender)
            {
                case SendingWindow.UserEnum:
                    if (enumerator != null)
                    {
                        enumerator.Stop(UI);
                    }
                    break;
                case SendingWindow.PasswordSpray:
                    if (sprayer != null)
                    {
                        sprayer.Stop(UI);
                    }
                    break;
            }
        }

        public void EnumerateUsers(UsernameEnumerationType method, string filepath, bool isChecked, string formatChoice, string startingPointName, string password)
        {
            if (service == MicrosoftService.ADFS && MSISSamlRequest == "")
            {
                UI.ThreadSafeAppendLog("[3]Awaiting SAML data...");
                GetMSISSamlRequest();
            }
            else if (service == MicrosoftService.ADFS)
            {
                UI.ThreadSafeAppendLog("[3]Using existing SAML data...");
            }
            enumerator.EnumerateUsers(method, filepath, isChecked, formatChoice, startingPointName, service, MSISSamlRequest, UI, host, password);
        }

        public void SprayUsers(PasswordSprayType method, string filepath, string formatChoice, string password, UsernameFormat discoveredFormat)
        {
            if (service == MicrosoftService.ADFS && MSISSamlRequest == "")
            {
                UI.ThreadSafeAppendLog("[3]Awaiting SAML data...");
                GetMSISSamlRequest();
            } else if (service == MicrosoftService.ADFS)
            {
                UI.ThreadSafeAppendLog("[3]Using existing SAML data...");
            }
            sprayer.SprayUsers(method, filepath, formatChoice, password, UI, discoveredFormat, host, service, MSISSamlRequest);
        }

        private void GetMSISSamlRequest()
        {

            WebRequests ADFSSaml = CreateWebRequest(host.EnumURL.Url);
            ADFSSaml.request.ContentType = "application/x-www-form-urlencoded";
            ADFSSaml.postData = "SignInIdpSite=SignInIdpSite&SignInSubmit=Sign+in&SingleSignOut=SingleSignOut";
            ADFSSaml.request.CookieContainer = new CookieContainer();
            string saml = ADFSSaml.MakePOSTRequest().Cookies[0].Value;

            if (saml == null || saml == "")
            {
                UI.ThreadSafeAppendLog("[1]Problem getting ADFS Saml Information...");
            }
            else
            {
                UI.ThreadSafeAppendLog("[3]Saml information stored...");
                MSISSamlRequest = saml;
            }
  
        }

        private WebRequests CreateWebRequest(string url)
        {
            WebRequests r = new WebRequests();
            r.url = url;
            r.InitialiseRequest();
            r.UI = UI;
            r.request.Method = "POST";
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            return r;
        }


    }
}
