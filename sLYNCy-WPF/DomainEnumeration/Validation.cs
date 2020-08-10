using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class Validation
    {
        public ObservableCollection<Hostnames> toValidate;
        public MainWindow UI;
        public string targetDomain;

        public void Validate()
        {
            foreach (Hostnames host in toValidate)
            {
                bool breaker = false;
                switch (host.Service)
                {
                    case MicrosoftService.ADFS:
                        //ENUM/SPRAY URLS ARE THE SAME - SO ONCE VALIDATED - ADD TO EACH
                        WebRequests ADFSRequest = CreateWebRequest("https://" + host.Hostname + "/adfs/ls/idpinitiatedsignon");
                        HttpWebResponse adfsResponse = ADFSRequest.MakeGETRequest();
                        string responseADFS = WebRequests.GetResponseString(adfsResponse);
                        WebHeaderCollection headersADFS = WebRequests.GetResponseHeaders(adfsResponse);
                        if (areYouNginxOrNull(headersADFS))
                        {
                            breaker = true;
                            break;
                        }
                        if (headersADFS != null)
                        {
                            if (headersADFS.ToString().Contains("No authentication method configured for the URL in the request"))
                            {
                                UI.ThreadSafeAppendLog("[2]Server appears to have authentication disabled for: " + ADFSRequest.url);
                            }
                            else if (responseADFS.Contains("/adfs/"))
                            {
                                //NOT GOT ANYTHING FOR O365 DISCOVERY HERE
                                host.O365 = "N";
                                host.EnumURL = new URLObject() { Url = "https://" + host.Hostname + "/adfs/ls/idpinitiatedsignon", Type = URLType.UserEnum };
                                host.SprayURL = new URLObject() { Url = "https://" + host.Hostname + "/adfs/ls/idpinitiatedsignon", Type = URLType.Oauth };
                                CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                AddService.AddServiceToOptions(UI, host.Service, true, true);
                            }
                        }
                        break;
                    case MicrosoftService.Exchange:
                        //Validate USER ENUM
                        WebRequests exchangeRequest = CreateWebRequest("https://" + host.Hostname + "/owa/auth.owa");
                        HttpWebResponse exchangeResponse = exchangeRequest.MakeGETRequest();
                        string responseExchange = WebRequests.GetResponseString(exchangeResponse);
                        WebHeaderCollection headersExchange = WebRequests.GetResponseHeaders(exchangeResponse);
                        if (areYouNginxOrNull(headersExchange))
                        {
                            breaker = true;
                            break;
                        }
                        if (headersExchange != null) {
                            if (headersExchange.ToString().Contains("No authentication method configured for the URL in the request"))
                            {
                                UI.ThreadSafeAppendLog("[2]Server appears to have authentication disabled for: " + exchangeRequest.url);
                            }
                            else if (responseExchange.Contains("owa") || responseExchange.Contains("OutlookSession"))
                            {
                                if (responseExchange.Contains("outlook.office.com"))
                                {
                                    //Check federated status and handle
                                    if (CheckFederated())
                                    {
                                        host.Federated = "Y";
                                    }
                                    else
                                    {
                                        host.Federated = "N";
                                    }
                                    //Can refactor this...
                                    //DEAL WITH O365 HERE
                                    host.O365 = "Y";
                                    //Change service to O365(?)
                                    host.Service = MicrosoftService.Office365;
                                    host.OAuthDomain = UI.targetDomain;
                                    host.EnumURL = new URLObject() { Url = "https://outlook.office365.com/autodiscover/autodiscover.json/v1.0/", Type = URLType.UserEnum };

                                    if (host.Federated == "Y")
                                    {
                                        //host.OAuthURL = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        AddService.AddServiceToOptions(UI, host.Service, true, false);
                                    }
                                    else
                                    {
                                        host.SprayURL = new URLObject() { Url = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token", Type = URLType.Oauth };
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        AddService.AddServiceToOptions(UI, host.Service, true, true);
                                    }
                                }
                                else
                                {
                                    //HOST should already have subdomain - IP address - can now add O365 N
                                    host.O365 = "N";
                                    host.EnumURL = new URLObject() { Url = "https://" + host.Hostname + "/owa/auth.owa", Type = URLType.UserEnum };
                                }
                            }
                        }
                        //VALIDATE PASS SPRAY SEPERATELY - MAY NOT HAVE ENUM - BUT DOES HAVE PASS SPRAYABLE POINT
                        WebRequests exchangeRequestSpray = CreateWebRequest("https://" + host.Hostname + "/Autodiscover");

                        HttpWebResponse exchangeResponseSpray = exchangeRequestSpray.MakeGETRequest();
                        string responseExchangeSpray = WebRequests.GetResponseString(exchangeResponseSpray);
                        WebHeaderCollection headersExchangeSpray = WebRequests.GetResponseHeaders(exchangeResponseSpray);

                        if (areYouNginxOrNull(headersExchangeSpray))
                        {
                            breaker = true;
                            break;
                        }
                        if (headersExchangeSpray != null)
                        {

                            if (headersExchangeSpray.ToString().Contains("NTLM") || headersExchangeSpray.ToString().Contains("Negotiate"))
                            {

                                if (headersExchangeSpray.GetValues("request-id") != null)
                                {
                                    if (host.O365 == "Y")
                                    {
                                        //We need to create a second host with JUST spray URL and add that too
                                        Hostnames host2 = new Hostnames() { SprayURL = new URLObject() { Url = "https://" + host.Hostname + "/Autodiscover", Type = URLType.Oauth }, Service = MicrosoftService.Exchange, O365 = "N", Hostname = host.Hostname, ipAddress = host.ipAddress };
                                        CollectionUpdates.AddHostnameRecord(host2, UI.enumeratedHostnames, UI);
                                        //NEED TO MAKE ADD SERVICE INCLUDING WHICH TABS TAKE INTO ACCOUNT IF HOST HAS ENUM/SPRAY ENABLED - ACTUALLY NEED SEPERATE HOST IF
                                        //ONE WAS O365 AND ONE WAS ON-PREM
                                        AddService.AddServiceToOptions(UI, host2.Service, false, true);
                                    }
                                    else
                                    {
                                        //Just add oauth to existing on-prem record which will be added at the end
                                        host.SprayURL = new URLObject() { Url = "https://" + host.Hostname + "/Autodiscover", Type = URLType.Oauth };
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        if (host.EnumURL != null)
                                        {
                                            AddService.AddServiceToOptions(UI, host.Service, true, true);
                                        }
                                        else
                                        {
                                            AddService.AddServiceToOptions(UI, host.Service, false, true);
                                        }

                                    }

                                }
                                else if (host.EnumURL != null)
                                {
                                    CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                    AddService.AddServiceToOptions(UI, host.Service, true, true);
                                }
                            } else
                            {
                                UI.ThreadSafeAppendLog("[2]Server appears to have authentication disabled for: " + exchangeRequestSpray.url);
                            }
                        }
                        break;
                    case MicrosoftService.Skype:
                        if (UI.enumeratedHostnames.Any(p => p.RealLync == true))
                        {
                            Hostnames tempHost = UI.enumeratedHostnames.Where(x => x.RealLync == true).First();
                            if (tempHost.SprayURL != null)
                            {
                                break;
                            }
                            }
                            //So - for any subdomains (will only be ones not already found) - validate
                            WebRequests lyncRequest = CreateWebRequest("https://" + host.Hostname);
                            lyncRequest.request.Accept = "text/xml";
                            HttpWebResponse lyncResponse = lyncRequest.MakeGETRequest();
                            string responseLync = WebRequests.GetResponseString(lyncResponse);
                            WebHeaderCollection lyncHeaders = WebRequests.GetResponseHeaders(lyncResponse);
                        if (areYouNginxOrNull(lyncHeaders))
                        {
                            breaker = true;
                            break;
                        }
                        if (responseLync.Contains("Autodiscover"))
                            {

                                if (responseLync.Contains("online.lync.com"))

                                {
                                    //Check federated status and handle
                                    if (CheckFederated())
                                    {
                                        host.Federated = "Y";
                                    }
                                    else
                                    {
                                        host.Federated = "N";
                                    }
                                    //DEAL WITH O365 HERE
                                    host.O365 = "Y";
                                    //Change service to O365(?)
                                    host.Service = MicrosoftService.Office365;
                                    host.OAuthDomain = UI.targetDomain;
                                    host.EnumURL = new URLObject() { Url = "https://outlook.office365.com/autodiscover/autodiscover.json/v1.0/", Type = URLType.UserEnum };


                                    if (host.Federated == "Y")
                                    {
                                        //host.OAuthURL = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        //So for O365 federated - can enum at Office but not spray
                                        AddService.AddServiceToOptions(UI, host.Service, true, false);
                                    }
                                    else
                                    {
                                        host.SprayURL = new URLObject() { Url = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token", Type = URLType.Oauth };
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        AddService.AddServiceToOptions(UI, host.Service, true, true);
                                    }
                                }
                                else
                                {
                                    //HOST should already have subdomain - IP address - can now add O365 N
                                    //Add that subdomain as not real (won't be as only lyncdiscover etc)
                                    host.O365 = "N";
                                    host.RealLync = false;
                                    CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                    //So we now have lyncdiscover/internal and have added - so now pull real address and fqdn and add
                                    //We also already have the response here
                                    //Then - if we don't already have "real"...
                                    if (UI.enumeratedHostnames.Any(p => p.RealLync == true))
                                    {

                                    }
                                    else
                                    {
                                        //Match the response we've already got here
                                        Match match = RegexClass.ReturnMatch(Regexs.SkypeRealAddress, responseLync);
                                        if (match.Success)
                                        {
                                            string realAddress = match.Value;
                                            UI.ThreadSafeAppendLog("[1][*] Skype Server Hostname: " + realAddress);
                                            //IP Lookup
                                            IPHostEntry ipEntry2 = Dns.GetHostEntry(realAddress);
                                            IPAddress[] ipAddr2 = ipEntry2.AddressList;
                                            foreach (IPAddress addr in ipAddr2)
                                            {
                                                Hostnames hostReal = new Hostnames() { Hostname = realAddress, ipAddress = addr, Service = MicrosoftService.Skype, O365 = "N", RealLync = true };
                                                //VALIDATE USER ENUM
                                                WebRequests lyncRequestReal = CreateWebRequest("https://" + realAddress + "/WebTicket/WebTicketService.svc/Auth");
                                                lyncRequestReal.request.Accept = "text/xml";
                                                HttpWebResponse lyncResponseReal = lyncRequestReal.MakeGETRequest();
                                                WebHeaderCollection Headers = WebRequests.GetResponseHeaders(lyncResponseReal);

                                            if (areYouNginxOrNull(Headers))
                                            {
                                                breaker = true;
                                                break;
                                            }
                                            if (Headers != null)
                                                {

                                                if (Headers.ToString().Contains("No authentication method configured for the URL in the request"))
                                                    {
                                                        UI.ThreadSafeAppendLog("[2]The server appears to have disabled authentication for the user enumeration URL: " + lyncRequestReal.url);
                                                    }
                                                    else if (Headers.GetValues("X-MS-Server-Fqdn") != null)
                                                    {
                                                        hostReal.EnumURL = new URLObject() { Url = "https://" + realAddress + "/WebTicket/WebTicketService.svc/Auth", Type = URLType.UserEnum };
                                                    }
                                                }
                                                //Seperately VALIDATE PASS SPRAY 
                                                //So this isn't the actual pass spray URL - but will tell us the grant types - other option is to check ACTIVE pass spray WITH
                                                //Creds - gibberish and see if it says unsupported grant type - would solve if they just blocked /OAuth for some reason
                                                //But more active - requires adding and removing during validation active/not active
                                                WebRequests lyncRequestSpray = CreateWebRequest("https://" + realAddress + "/WebTicket/WebTicketService.svc/OAuth");
                                                HttpWebResponse lyncRequestSprayResponse = lyncRequestSpray.MakeGETRequest();
                                                WebHeaderCollection HeadersSpray = WebRequests.GetResponseHeaders(lyncRequestSprayResponse);

                                            if (areYouNginxOrNull(HeadersSpray))
                                            {
                                                breaker = true;
                                                break;
                                            }
                                            if (HeadersSpray != null)
                                                {

                                                if (HeadersSpray.ToString().Contains("password"))
                                                    {
                                                        if (HeadersSpray.GetValues("client-request-id") != null)
                                                        {
                                                            UI.ThreadSafeAppendLog("[1][*] " + realAddress + ": " + addr.ToString());
                                                            hostReal.SprayURL = new URLObject() { Url = "https://" + realAddress + "/WebTicket/oauthtoken", Type = URLType.Oauth };
                                                            CollectionUpdates.AddHostnameRecord(hostReal, UI.enumeratedHostnames, UI);
                                                            if (hostReal.EnumURL != null)
                                                            {
                                                                AddService.AddServiceToOptions(UI, hostReal.Service, true, true);
                                                            }
                                                            else
                                                            {
                                                                AddService.AddServiceToOptions(UI, hostReal.Service, false, true);
                                                            }
                                                        }
                                                        else if (hostReal.EnumURL != null)
                                                        {
                                                            UI.ThreadSafeAppendLog("[1][*] " + realAddress + ": " + addr.ToString());
                                                            CollectionUpdates.AddHostnameRecord(hostReal, UI.enumeratedHostnames, UI);
                                                            AddService.AddServiceToOptions(UI, hostReal.Service, true, true);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        UI.ThreadSafeAppendLog("[2]It looks like the oauth endpoint has disabled password authentication: " + lyncRequestSpray.url);
                                                    }
                                                }

                                            if (hostReal.EnumURL != null)
                                            {
                                                AddService.AddServiceToOptions(UI, hostReal.Service, true, true);
                                            } else if (hostReal.EnumURL == null && hostReal.SprayURL == null)
                                            {
                                                //Not sure we could hit here with Y - but just to check
                                                if (hostReal.O365 != "Y")
                                                {
                                                    UI.ThreadSafeAppendLog("[2]No user enum or pass spray URL discovered for " + host.Hostname + " - attempting to find NTLM endpoints...");
                                                    NTLMDiscovery domainInformation = new NTLMDiscovery();
                                                    ObservableCollection<Hostnames> mySoloHostname = new ObservableCollection<Hostnames>();
                                                    mySoloHostname.Add(hostReal);
                                                    domainInformation.hostnamesForDomainInformationEnumeration = mySoloHostname;
                                                    domainInformation.UI = UI;
                                                    URLObject returnedSprayURL = domainInformation.FindNTLMEndpoints(hostReal);
                                                    if (returnedSprayURL != null)
                                                    {
                                                        hostReal.SprayURL = returnedSprayURL;
                                                        hostReal.O365 = "N";
                                                        CollectionUpdates.AddHostnameRecord(hostReal, UI.enumeratedHostnames, UI);
                                                        AddService.AddServiceToOptions(UI, hostReal.Service, false, true);
                                                    }
                                                }
                                            }


                                            }
                                        }

                                    }
                                }
                        }
                        break;
                    case MicrosoftService.Office365:
                        WebRequests O365Request = CreateWebRequest("https://" + host.Hostname + "/autodiscover/autodiscover.json/v1.0/test.test@" + targetDomain + "?Protocol=Autodiscoverv1");
                        O365Request.request.AllowAutoRedirect = false;
                        HttpWebResponse O365WebResponse = O365Request.MakeGETRequest();
                        string responseO365 = WebRequests.GetResponseString(O365WebResponse);

                        UI.ThreadSafeAppendLog("[4]O365 Response: " + responseO365);
                        if (responseO365.Contains("outlook.office365.com"))
                        {
                            //Check federated status and handle
                            if (CheckFederated())
                            {
                                host.Federated = "Y";
                            }
                            else
                            {
                                host.Federated = "N";
                            }

                            host.O365 = "Y";
                            //Hard code - we manually set up this record - but as this is also needed for lyncdiscover cloud hosted etc...
                            host.EnumURL = new URLObject() { Url = "https://outlook.office365.com/autodiscover/autodiscover.json/v1.0/", Type = URLType.UserEnum };
                            //FOR NOW - NOT SETTING NTLMDOMAIN - O365 WILL ALWAYS BE HITTING THE @DOMAIN.COM ANYWAY?
                            host.OAuthDomain = UI.targetDomain;


                            if (host.Federated == "Y")
                            {
                                //host.OAuthURL = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
                                CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                AddService.AddServiceToOptions(UI, host.Service, true, false);
                            }
                            else
                            {
                                host.SprayURL = new URLObject() { Url = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token", Type = URLType.Oauth };
                                CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                AddService.AddServiceToOptions(UI, host.Service, true, true);
                            }
                        } 
                        break;
                    case MicrosoftService.RDWeb:
                        WebRequests rdWebRequest = CreateWebRequest("https://" + host.Hostname + "/RDWeb/Pages/en-US/login.aspx");
                        HttpWebResponse rdWebResponse = rdWebRequest.MakeGETRequest();
                        string responseRDWeb = WebRequests.GetResponseString(rdWebResponse);
                        WebHeaderCollection responseRDHeaders = WebRequests.GetResponseHeaders(rdWebResponse);
                        if (areYouNginxOrNull(responseRDHeaders))
                        {
                            breaker = true;
                            break;
                        }
                        if (responseRDHeaders != null)
                        {

                            if (responseRDHeaders.ToString().Contains("No authentication method configured for the URL in the request"))
                            {
                                UI.ThreadSafeAppendLog("[2]The server appears to have disabled authentication for the URL: " + rdWebRequest.url);
                            } else
                            {
                                if (responseRDWeb.Contains("RD Web Access"))
                                {
                                    //NOT GOT ANYTHING FOR RDWEB O365
                                    host.O365 = "N";
                                    host.EnumURL = new URLObject() { Url = "https://" + host.Hostname + "/RDWeb/Pages/en-US/login.aspx", Type = URLType.UserEnum };
                                    host.SprayURL = new URLObject() { Url = "https://" + host.Hostname + "/RDWeb/Pages/en-US/login.aspx", Type = URLType.Oauth };
                                    CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                    AddService.AddServiceToOptions(UI, host.Service, true, true);
                                }
                            }
                        }

                        
                        break;

                }
                if (breaker)
                {

                }
                else
                {
                    //At the end of doing that host - if we don't have either - ONLY IF NOT EITHER - do NTLM
                    Hostnames tempHost2 = null;
                    if (UI.enumeratedHostnames.Any(p => p.RealLync == true))
                    {
                        tempHost2 = UI.enumeratedHostnames.Where(x => x.RealLync == true).First();
                    }
                    if (tempHost2 != null)
                    {
                        if (tempHost2.SprayURL != null)
                        {

                        }
                        else
                        {
                            if (host.EnumURL == null && host.SprayURL == null)
                            {
                                //Not sure we could hit here with Y - but just to check
                                if (host.O365 != "Y")
                                {
                                    UI.ThreadSafeAppendLog("[2]No user enum or pass spray URL discovered - attempting to find NTLM endpoints...");
                                    NTLMDiscovery domainInformation = new NTLMDiscovery();
                                    ObservableCollection<Hostnames> mySoloHostname = new ObservableCollection<Hostnames>();
                                    mySoloHostname.Add(host);
                                    domainInformation.hostnamesForDomainInformationEnumeration = mySoloHostname;
                                    domainInformation.UI = UI;
                                    URLObject returnedSprayURL = domainInformation.FindNTLMEndpoints(host);
                                    if (returnedSprayURL != null)
                                    {
                                        host.SprayURL = returnedSprayURL;
                                        host.O365 = "N";
                                        CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                        AddService.AddServiceToOptions(UI, host.Service, false, true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (host.EnumURL == null && host.SprayURL == null)
                        {
                            //Not sure we could hit here with Y - but just to check
                            if (host.O365 != "Y")
                            {
                                UI.ThreadSafeAppendLog("[2]No user enum or pass spray URL discovered - attempting to find NTLM endpoints...");
                                NTLMDiscovery domainInformation = new NTLMDiscovery();
                                ObservableCollection<Hostnames> mySoloHostname = new ObservableCollection<Hostnames>();
                                mySoloHostname.Add(host);
                                domainInformation.hostnamesForDomainInformationEnumeration = mySoloHostname;
                                domainInformation.UI = UI;
                                URLObject returnedSprayURL = domainInformation.FindNTLMEndpoints(host);
                                if (returnedSprayURL != null)
                                {
                                    host.SprayURL = returnedSprayURL;
                                    host.O365 = "N";
                                    CollectionUpdates.AddHostnameRecord(host, UI.enumeratedHostnames, UI);
                                    AddService.AddServiceToOptions(UI, host.Service, false, true);
                                }
                            }
                        }
                    }
                }
            }

        }


        public bool areYouNginxOrNull(WebHeaderCollection headers)
        {
            string[] serverNames = { "nginx", "apache", "RTC", "Tengine", "openresty", "LiteSpeed", "FlyWheel", "IdeaWebServer", "Vivo", "GSE" };
            if (headers != null)
            {
                if (headers.GetValues("Server") != null)
                {
                    foreach (string server in headers.GetValues("Server"))
                    {
                        if (serverNames.Any(s => server.Contains(s)))
                        {
                            UI.ThreadSafeAppendLog("[3]Server is none windows: " + server);
                            UI.ThreadSafeAppendLog("[3]Aborting other checks on host...");
                            return true;
                        }
                    }

                }
            }else
            {
                return true;
            }
            //Should never hit here
            return false;
        }

        public bool CheckFederated()
        {
            UI.ThreadSafeAppendLog("[2]Checking organisation federated status...");
            WebRequests checkFederated = CreateWebRequestFederated("https://login.microsoftonline.com/common/userrealm/?user=john.smith@" + targetDomain + "&api-version=2.1&checkForMicrosoftAccount=true");

            HttpWebResponse federatedWebResponse = checkFederated.MakeGETRequest();
            string responseFederated = WebRequests.GetResponseString(federatedWebResponse);

            JObject federatedResponse = JObject.Parse(responseFederated);
            
            string isFederated = (string)federatedResponse.SelectToken("NameSpaceType");
            string brandName = (string)federatedResponse.SelectToken("FederationBrandName");
            UI.ThreadSafeAppendLog("[2][*]Organisation brand name: " + brandName);
            if (federatedResponse != null)
            {
                JToken interim = federatedResponse.SelectToken("TenantBrandingInfo[0]");
                if (interim != null)
                {
                    var signInDisabled = interim.SelectToken("KeepMeSignedInDisabled");
                    //bool signInDisabled = (bool)federatedResponse.SelectToken("TenantBrandingInfo[0].KeepMeSignedInDisabled");
                    UI.ThreadSafeAppendLog("[2][*]Organisation - cloud setting KeepMeSignedInDisabled: " + signInDisabled);
                }
            }
            

            if (isFederated == "Federated")
            {
                UI.ThreadSafeAppendLog("[2][*]Organisation is federated - password spraying needs to hit ADFS not O365 login...");
                string ADFSUrl = (string)federatedResponse.SelectToken("AuthURL");
                //Need to check and add ADFS record here
                //Strip out server name
                string ADFSUrlStripped = "";
                Match match = RegexClass.ReturnMatch(Regexs.ADFSAuthURL, ADFSUrl);
                if (match.Success)
                {
                    ADFSUrlStripped = match.Value;
                }
                if (ADFSUrlStripped != "")
                {
                    UI.ThreadSafeAppendLog("[2]Validating ADFS Auth URL: https://" + ADFSUrlStripped + "/adfs/ls/idpinitiatedsignon");
                    WebRequests ADFSValidationRequest = CreateWebRequest("https://" + ADFSUrlStripped + "/adfs/ls/idpinitiatedsignon");
                    
                    HttpWebResponse ADFSValidationRequestResponse = ADFSValidationRequest.MakeGETRequest();
                    string responseADFSValidation = WebRequests.GetResponseString(ADFSValidationRequestResponse);
                    WebHeaderCollection headersADFSValidation = WebRequests.GetResponseHeaders(ADFSValidationRequestResponse);

                    if (headersADFSValidation != null)
                    {
                        if (headersADFSValidation.ToString().Contains("No authentication method configured for the URL in the request"))
                        {
                            UI.ThreadSafeAppendLog("[2]Server appears to have authentication disabled for: " + ADFSValidationRequest.url);
                        }
                        else if (responseADFSValidation.Contains("/adfs/"))
                        {
                            IPAddress[] ipaddrADFS;
                            IPHostEntry ipEntry;
                            ipEntry = Dns.GetHostEntry(ADFSUrlStripped);
                            ipaddrADFS = ipEntry.AddressList;
                            Hostnames validADFS = new Hostnames() { O365 = "N", Hostname = ADFSUrlStripped, EnumURL = new URLObject() { Url = "https://" + ADFSUrlStripped + "/adfs/ls/idpinitiatedsignon", Type = URLType.UserEnum }, SprayURL = new URLObject() { Url = "https://" + ADFSUrlStripped + "/adfs/ls/idpinitiatedsignon", Type = URLType.Oauth }, Service = MicrosoftService.ADFS, ipAddress = ipaddrADFS.First() };
                            //NOT GOT ANYTHING FOR O365 DISCOVERY HERE
                            CollectionUpdates.AddHostnameRecord(validADFS, UI.enumeratedHostnames, UI);
                            AddService.AddServiceToOptions(UI, validADFS.Service, true, true);
                        }
                    }
                } else
                {
                    UI.ThreadSafeAppendLog("[2]Unable to match ADFS URL...");
                }
                return true;
            } else
            {
                UI.ThreadSafeAppendLog("[2][*]Organisation is not federated - password spraying can hit O365 login, and will provide details of MFA...");
                return false;
            }

            //Need to properly JSON parse here - get NameSpaceType (if !null - if Federated
            //Also output - KeepMeSignedInDisabled and FederationBrandName

            //Basically - output - Federated/Managed/Whatever to UI - but is either true or false - Federated
            //Also output SignInDisabled and FederationBrandName - but don't need to add to any records anywhere

            //Also - pull AuthURL and add that record if Federated only
        }

        public WebRequests CreateWebRequest(string validationURL)
        {
            UI.ThreadSafeAppendLog("[3]Validating DNS Entry: " + validationURL);
            WebRequests r = new WebRequests();
            r.url = validationURL;
            r.InitialiseRequest();
            r.UI = UI;
            r.request.Method = "GET";
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            r.request.Timeout = 10000;
            return r;
        }

        public WebRequests CreateWebRequestFederated(string validationURL)
        {
            UI.ThreadSafeAppendLog("[3]Checking Federated Status: " + validationURL);
            WebRequests r = new WebRequests();
            r.url = validationURL;
            r.InitialiseRequest();
            r.UI = UI;
            r.request.Method = "GET";
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            r.request.Timeout = 10000;
            return r;
        }


    }
}
