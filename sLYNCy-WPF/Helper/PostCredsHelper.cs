using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public static class PostCredsHelper
    {
        public static ServiceInterface getOrCreateServiceInterface(List<ServiceInterface> serviceInterfaces, MainWindow UI, ObservableCollection<Hostnames> enumeratedHostnames)
        {
            if (serviceInterfaces.Any(u => u.service == MicrosoftService.Skype))
            {
                UI.ThreadSafeAppendLog("[3]Selecting existing Skype interface...");
                IEnumerable<ServiceInterface> allSkype = serviceInterfaces.Where(x => x.service == MicrosoftService.Skype);
                foreach (ServiceInterface serv in allSkype)
                {
                    if (serv.host.RealLync == true)
                    {
                        serv.postCompromise.RealAddress = serv.host.Hostname;
                        return serv;
                    }
                }
            }
            else
            {
                //Don't already have service interface for skype - so create if we can
                if (enumeratedHostnames.Any(x => x.Service == MicrosoftService.Skype && x.RealLync == true))
                {
                    Hostnames enumeratorHostname = (enumeratedHostnames.Where(x => x.Service == MicrosoftService.Skype && x.RealLync == true)).First();
                    UI.ThreadSafeAppendLog("[3]Adding new enumerator with service: " + MicrosoftService.Skype);
                    ServiceInterface serviceInterface = new ServiceInterface() { host = enumeratorHostname, service = MicrosoftService.Skype, UI = UI, enumerator = new UsernameEnumeration(), sprayer = new PasswordSpray(), postCompromise = new PostCompromise() { RealAddress = enumeratorHostname.Hostname, UI = UI } };
                    serviceInterfaces.Add(serviceInterface);
                    return serviceInterface;
                }
                else
                {
                    //error message comes from what calls this
                    return null;
                }
            }
            return null;
        }

        public static Boolean PreEmptOAuthPostExploit(CredentialsRecord record, List<ServiceInterface> serviceInterfaces, MainWindow UI, ObservableCollection<Hostnames> enumeratedHostnames)
        {
            if (record.Token != "" && record.Token != null)
            {
                return true;
            }
            else
            {
                ServiceInterface serviceInterface = getOrCreateServiceInterface(serviceInterfaces, UI, enumeratedHostnames);
                if (serviceInterface != null)
                {
                    //So basically - we need to auth to the REAL SKYPE interface AUTH ENDPOINT - IF it is oauth - and get the token - add it to this record and change the record's service to Skype
                    //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST


                    //Check it is not an NTLM endpoint or such for Skype
                    if (serviceInterface.host.SprayURL.Url != "" && serviceInterface.host.SprayURL.Url != null)
                    {
                        if (serviceInterface.host.SprayURL.Url.Contains("oauthtoken"))
                        {
                            //This CAN BE either legacy or modern format - so just go with what we have as known valid
                            //Only causes issue with legacy here if user is invalid = v slow
                            string domainUser = record.Username;
                            UI.ThreadSafeAppendLog("[4]Record username in required format: " + domainUser);

                            string postData = "grant_type=password&username=" + domainUser + "&password=" + Uri.EscapeDataString(record.Password);

                            //Add headers and timout to web request
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceInterface.host.SprayURL.Url);
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.12; rv:55.0) Gecko/20100101 Firefox/55.0";
                            request.Method = "POST";
                            var data = Encoding.ASCII.GetBytes(postData);
                            request.ContentLength = data.Length;

                            try
                            {

                                using (var stream = request.GetRequestStream())
                                {
                                    stream.Write(data, 0, data.Length);
                                }
                                var response = (HttpWebResponse)request.GetResponse();
                                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                response.Close();
                                UI.ThreadSafeAppendLog("[4]Response to PreemptPostExploit: " + responseString);

                                if (responseString.Contains("access_token"))
                                {
                                    //Add access token to existing record

                                    Match accessTokenMatch = RegexClass.ReturnMatch(Regexs.cwtToken, responseString);
                                    if (accessTokenMatch.Success)
                                    {
                                        string accessToken = accessTokenMatch.Value;
                                        record.Token = accessToken;
                                        record.Service = MicrosoftService.Skype;
                                        UI.ThreadSafeAppendLog("[2]Valid access token added for user: " + record.Username);
                                        UI.saveValidUsersAndCreds(null, SaveType.autoLog);
                                        return true;
                                    }
                                    else
                                    {
                                        UI.ThreadSafeAppendLog("[1]Valid Credentials found for user: " + record.Username + ", but unable to match access token.");
                                        return false;
                                    }
                                }
                            }
                            catch (WebException webException3)
                            {
                                HttpWebResponse response = webException3.Response as HttpWebResponse;
                                try
                                {

                                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                    UI.ThreadSafeAppendLog("[4]Response to PreemptPostExploit: " + responseString);
                                    if (responseString.Contains("access_token"))
                                    {
                                        Match accessTokenMatch = RegexClass.ReturnMatch(Regexs.cwtToken, responseString);
                                        if (accessTokenMatch.Success)
                                        {
                                            string accessToken = accessTokenMatch.Value;
                                            record.Token = accessToken;
                                            record.Service = MicrosoftService.Skype;
                                            UI.ThreadSafeAppendLog("[2]Valid access token added for user: " + record.Username);
                                            UI.saveValidUsersAndCreds(null, SaveType.autoLog);
                                            return true;
                                        }
                                        else
                                        {
                                            UI.ThreadSafeAppendLog("[1]Valid Credentials found for user: " + record.Username + ", but unable to match access token.");
                                            return false;
                                        }
                                    }
                                    else if (responseString.Contains("Bad Request"))
                                    {
                                        UI.ThreadSafeAppendLog("[1]Valid credentials found - User not SIP enabled or similar: " + record.Username);
                                        return false;

                                    }
                                }
                                catch (Exception esdf)
                                {
                                    UI.ThreadSafeAppendLog("[1]Exception: " + esdf.ToString());
                                    return false;
                                }

                            }
                            catch (Exception ex)
                            {
                                UI.ThreadSafeAppendLog("[1]Exception: " + ex.ToString());
                                return false;
                            }
                        }
                        else
                        {
                            UI.ThreadSafeAppendLog("[1]Oauth URL not found for Skype host - perhaps the password spray URL is NTLM authentication or other...");
                            return false;
                        }


                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[1]No valid authentication URL...");
                        return false;
                    }
                }
                else
                {
                    UI.ThreadSafeAppendLog("[1]No valid Skype host for authenticaton...");
                    return false;
                }
            }
            return false;

        }

        public static WebRequests CreateWebRequest(string url, MainWindow UI, HttpMethodChoice method, string accessToken)
        {
            WebRequests r = new WebRequests();
            r.url = url;
            r.InitialiseRequest();
            r.UI = UI;
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            r.request.ContentType = "application/json";
            r.request.Accept = "application/json";
            r.request.Headers.Add("Authorization: Bearer " + accessToken);
            if (method == HttpMethodChoice.GET)
                r.request.Method = "GET";
            else if (method == HttpMethodChoice.POST)
                r.request.Method = "POST";
            return r;
        }

    }
}
