using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class NTLMDiscovery
    {
        public ObservableCollection<Hostnames> hostnamesForDomainInformationEnumeration;
        private ObservableCollection<Hostnames> myHostnames = new ObservableCollection<Hostnames>();
        public MainWindow UI;
        private List<string> URLs;

        public void GetDomainInformation()
        {
            //First - check for only those that DONT have NTLM auth URL + OAuth URL
                //Other than any with O365 = Y or MicrosoftService.O365
                foreach (Hostnames host in hostnamesForDomainInformationEnumeration)
            {
                //Only do non-o365 AND not already got domain
                if (host.O365 == "Y" || host.Service == MicrosoftService.Office365 || host.OAuthDomain != "")
                {

                }
                else
                {
                    //If it's not lync and has no NTLM auth already - just add and get info
                    UI.ThreadSafeAppendLog("[4]Adding host...");
                    myHostnames.Add(host);
                }

            }
                if (myHostnames.Count > 0)
            {
                UI.ThreadSafeAppendLog("[4]Hostnames To Domain Info Count: " + myHostnames.Count);
                //Now have list of hostnames NOT on O365 - and NOT already domain gathered from...
                foreach(Hostnames host in myHostnames)
                {
                    switch (host.Service)
                    {
                        case MicrosoftService.Skype:
                                //Only do for real address - as we've already added this at validation
                                if (host.RealLync == true)
                                {
                                    UI.ThreadSafeAppendLog("[4]Lync...");
                                    //Load list of Lync related URLs
                                    addLinesFromFile("Lync", host.Hostname);
                                    MakeNTLMRequests(host, false);
                                } 
                            break;
                        case MicrosoftService.ADFS:
                                //Load list of ADFS related URLs
                                UI.ThreadSafeAppendLog("[3]Adding ADFS URLs...");
                                addLinesFromFile("ADFS", host.Hostname);
                                UI.ThreadSafeAppendLog("[4]Number of URLs: " + URLs.Count);
                                MakeNTLMRequests(host, true);
                                break;
                        case MicrosoftService.Exchange:
                                //Load list of Exchange related URLs
                                UI.ThreadSafeAppendLog("[3]Adding Exchange URLs...");
                                addLinesFromFile("Exchange", host.Hostname);
                                UI.ThreadSafeAppendLog("[4]Number of URLs: " + URLs.Count);
                                MakeNTLMRequests(host, false);
                                break;
                        case MicrosoftService.Exchange2007:
                            //Load list of Exchange related URLs
                            UI.ThreadSafeAppendLog("[3]Adding Exchange 2007 URLs...");
                            addLinesFromFile("Exchange2007", host.Hostname);
                            UI.ThreadSafeAppendLog("[4]Number of URLs: " + URLs.Count);
                            MakeNTLMRequests(host, false);
                            break;
                        case MicrosoftService.RDWeb:
                            //Load list of RDWeb related URLs
                            UI.ThreadSafeAppendLog("[3]Adding RDWeb URLs...");
                            addLinesFromFile("RDWeb", host.Hostname);
                            UI.ThreadSafeAppendLog("[4]Number of URLs: " + URLs.Count);
                            MakeNTLMRequests(host, false);
                            break;
                    }

                }




            } else
            {
                UI.ThreadSafeAppendLog("[2]No new subdomains to gather internal domain information from...");
            }

        }

        public URLObject FindNTLMEndpoints(Hostnames host)
        {
            switch (host.Service)
            {
                case MicrosoftService.Skype:
                    //Only do for real address - as we've already added this at validation
                        UI.ThreadSafeAppendLog("[3]Adding Skype URLs...");
                        //Load list of Lync related URLs
                        addLinesFromFile("Lync", host.Hostname);
                        return DiscoverNTLM();
                case MicrosoftService.ADFS:
                    //Load list of ADFS related URLs
                    UI.ThreadSafeAppendLog("[3]Adding ADFS URLs...");
                    addLinesFromFile("ADFS", host.Hostname);
                    return DiscoverNTLM();
                case MicrosoftService.Exchange:
                    //Load list of Exchange related URLs
                    UI.ThreadSafeAppendLog("[3]Adding Exchange URLs...");
                    addLinesFromFile("Exchange", host.Hostname);
                    return DiscoverNTLM();
                case MicrosoftService.Exchange2007:
                    //Load list of Exchange related URLs
                    UI.ThreadSafeAppendLog("[3]Adding Exchange 2007 URLs...");
                    addLinesFromFile("Exchange2007", host.Hostname);
                    return DiscoverNTLM();
                case MicrosoftService.RDWeb:
                    //Load list of RDWeb related URLs
                    UI.ThreadSafeAppendLog("[3]Adding RDWeb URLs...");
                    addLinesFromFile("RDWeb", host.Hostname);
                    return DiscoverNTLM();
            }
            return null;
        }

        private URLObject DiscoverNTLM()
        {
            foreach (string url in URLs)
            {

                //So this is the same for all - if NTLM - just NTLM spray it - 401/403 is bad - else good
                //Also - if response NOT CONTAIN NTLM || Negotiate - probably not sprayable? Shouldn't have got to this stage though
                WebRequests NTLMWebRequest = CreateWebRequest(url);
                UI.ThreadSafeAppendLog("[3]Testing for NTLM authentication at URL: " + NTLMWebRequest.request.RequestUri + "...");
                
                HttpWebResponse NTLMResponse = NTLMWebRequest.MakeGETRequest();
                if (NTLMResponse != null)
                {
                    WebHeaderCollection headers = WebRequests.GetResponseHeaders(NTLMResponse);

                    if (areYouNginxOrNull(headers))
                    {
                        return null;
                    }
                    if (headers != null)
                    {
                        string ntlmResponse = headers.ToString();
                        if (ntlmResponse.Contains("NTLM") || ntlmResponse.Contains("Negotiate"))
                        {
                            //Should also bail out of this whole thing?
                            return new URLObject() { Url = url, Type = URLType.NTLM };
                        }
                    }
                    NTLMResponse.Close();
                }
            }
            return null;
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
            }
            else
            {
                return true;
            }
            return false;
        }

        private void MakeNTLMRequests(Hostnames host, bool isADFS)
        {
            if (host.NTLMDomain == "" && host.OAuthDomain == "")
            {
                if (host.SprayURL.Url != "")
                {
                    if (host.SprayURL.Type == URLType.NTLM)
                    {
                        //So we already have NTLM spray URL - do it there as that's what we'll be hitting
                        //Make NTLM Auth Request
                        WebRequests NTLMWebRequest = CreateWebRequest(host.SprayURL.Url);
                        UI.ThreadSafeAppendLog("[3]Testing for NTLM authentication at URL: " + NTLMWebRequest.request.RequestUri + "...");
                        if (isADFS)
                        {
                            NTLMWebRequest.request.Headers.Add("Authorization", "Negotiate TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==");
                        }
                        else
                        {
                            NTLMWebRequest.request.Headers.Add("Authorization", "NTLM TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==");
                        }
                        HttpWebResponse NTLMResponse = NTLMWebRequest.MakeGETRequest();
                        WebHeaderCollection headers = WebRequests.GetResponseHeaders(NTLMResponse);
                        if (headers != null)
                        {
                            string ntlmResponse = headers.ToString();
                            //FOR DOMAIN INFORMATION - don't want to check has NTLM or Negotiate as we still just want to get the response even if we can't spray
                            UI.ThreadSafeAppendLog("[3]NTLM Response: " + ntlmResponse);
                            if (isADFS)
                            {
                                Match NTLMString = RegexClass.ReturnMatch(Regexs.NTLMResponseADFS, ntlmResponse);
                                if (NTLMString.Success)
                                {
                                    ParseNTLM(NTLMString.Value, host, host.SprayURL.Url);
                                }
                            }
                            else
                            {
                                Match NTLMString = RegexClass.ReturnMatch(Regexs.NTLMResponse, ntlmResponse);
                                if (NTLMString.Success)
                                {
                                    ParseNTLM(NTLMString.Value, host, host.SprayURL.Url);
                                }
                            }

                        }
                        else
                        {
                            UI.ThreadSafeAppendLog("[2]Null Resposne");
                        }
                    }
                }

                //Do yet ANOTHER test - have we cracked it above - if not - cycle through list of possibles...
                if (host.NTLMDomain == "" && host.OAuthDomain == "")
                {
                    foreach (string url in URLs)
                    {
                        //Do the check again in case the last one has now filled these in
                        if (host.NTLMDomain == "" && host.OAuthDomain == "")
                        {

                            //Make NTLM Auth Request
                            WebRequests NTLMWebRequest = CreateWebRequest(url);
                            UI.ThreadSafeAppendLog("[3]Testing for NTLM authentication at URL: " + NTLMWebRequest.request.RequestUri + "...");
                            if (isADFS)
                            {
                                NTLMWebRequest.request.Headers.Add("Authorization", "Negotiate TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==");
                            }
                            else
                            {
                                NTLMWebRequest.request.Headers.Add("Authorization", "NTLM TlRMTVNTUAABAAAAB4IIogAAAAAAAAAAAAAAAAAAAAAGAbEdAAAADw==");
                            }
                            HttpWebResponse NTLMResponse = NTLMWebRequest.MakeGETRequest();
                            WebHeaderCollection headers = WebRequests.GetResponseHeaders(NTLMResponse);
                            if (headers != null)
                            {
                                string ntlmResponse = headers.ToString();
                                //FOR DOMAIN INFORMATION - don't want to check has NTLM or Negotiate as we still just want to get the response even if we can't spray
                                UI.ThreadSafeAppendLog("[3]NTLM Response: " + ntlmResponse);
                                if (isADFS)
                                {
                                    Match NTLMString = RegexClass.ReturnMatch(Regexs.NTLMResponseADFS, ntlmResponse);
                                    if (NTLMString.Success)
                                    {
                                        ParseNTLM(NTLMString.Value, host, url);
                                    }
                                }
                                else
                                {
                                    Match NTLMString = RegexClass.ReturnMatch(Regexs.NTLMResponse, ntlmResponse);
                                    if (NTLMString.Success)
                                    {
                                        ParseNTLM(NTLMString.Value, host, url);
                                    }
                                }

                            }
                            else
                            {
                                UI.ThreadSafeAppendLog("[2]Null Resposne");
                            }
                        }
                        else
                        {
                            UI.ThreadSafeAppendLog("[4]Break");
                            break;
                        }

                    }
                }
            }
        }

        private void ParseNTLM(string ntlmResponseString, Hostnames host, string url)
        {
            byte[] test = Convert.FromBase64String(ntlmResponseString);
            string hex = BitConverter.ToString(test);

            //NEW - Thanks @ROSS
            int DOMAIN_SUPPLIED = 0x1000;
            int WORKSTATION_SUPPLIED = 0x2000;
            int END_OF_LINE = 0x0000;
            int NB_COMPUTER_NAME = 0x0001;
            int NB_DOMAIN_NAME = 0x0002;
            int DNS_COMPUTER_NAME = 0x0003;
            int DNS_DOMAIN_NAME = 0x0004;
            int DNS_FOREST_NAME = 0x0005;

            byte[] bytesFirst7 = new byte[7];
            Array.Copy(test, 0, bytesFirst7, 0, 7);
            if (Encoding.ASCII.GetString(bytesFirst7) != "NTLMSSP")
            {
                UI.ThreadSafeAppendLog("[3]Not a valid NTLM response.");
            }
            else
            {
                UI.ThreadSafeAppendLog("[3]Valid NTLM response.");
                int offset = 8;
                int version = BitConverter.ToInt32(test, offset);
                UI.ThreadSafeAppendLog("[3] NTLM Authentication Version: " + version);
                offset += 4;
                int flags = BitConverter.ToInt32(test, offset);
                offset += 4;

                //NTLM v1
                if (version == 1)
                {
                    if ((flags & DOMAIN_SUPPLIED) == DOMAIN_SUPPLIED)
                    {
                        int domainLength = BitConverter.ToInt16(test, offset);
                        offset += 2;

                        int domainOffset = BitConverter.ToInt32(test, offset);
                        offset += 4;
                        byte[] domain = new byte[domainLength];
                        Array.Copy(test, domainOffset, domain, 0, domainLength);
                        string domainString = Encoding.ASCII.GetString(domain);
                        UI.ThreadSafeAppendLog("[3]Domain: " + domainString);
                        host.NTLMDomain = domainString;

                    }
                    if ((flags & WORKSTATION_SUPPLIED) == WORKSTATION_SUPPLIED)
                    {
                        int workstationLength = BitConverter.ToInt16(test, offset);
                        offset += 2;
                        int workstationOffset = BitConverter.ToInt32(test, offset);
                        offset += 4;

                        byte[] workstation = new byte[workstationLength];
                        Array.Copy(test, workstationOffset, workstation, 0, workstationLength);
                        string workstationString = Encoding.ASCII.GetString(workstation);
                        UI.ThreadSafeAppendLog("[3]Workstation: " + workstationString);
                    }
                } else if (version == 2)
                {
                    offset = 40;
                    //Skip server challenge and reserved
                    //offset += 16;
                    int targetInfoLength = BitConverter.ToInt16(test, offset);
                    //Skip past length (which we have) and something that should also = length as well?!
                    //ThreadSafeAppendLog("[3]Target Info Length: " + targetInfoLength);
                    offset += 4;
                    int targetInfoOffset = BitConverter.ToInt32(test, offset);
                    //ThreadSafeAppendLog("[3]Target Info Offset: " + targetInfoOffset);
                    offset += 4;

                    byte[] TargetInfo = new byte[targetInfoLength];
                    Array.Copy(test, targetInfoOffset, TargetInfo, 0, targetInfoLength);
                    string testTargetInfo = BitConverter.ToString(TargetInfo);
                    //ThreadSafeAppendLog("[3]test string: " + testTargetInfo);
                    string test1Target = testTargetInfo.Replace("-", "");
                    string test2Target = test1Target.Replace("00", "");
                    //string targetInfoString = Encoding.ASCII.GetString(TargetInfo);
                    //ThreadSafeAppendLog("[3]Target Info: " + test2Target);

                    //Now need to go through and pull each bit
                    int flag_end = 1;
                    int offset_Target_Info = 0;
                    while (flag_end != END_OF_LINE)
                    {
                        int AvLd = BitConverter.ToInt16(TargetInfo, offset_Target_Info);
                        flag_end = AvLd;

                        offset_Target_Info += 2;
                        int AvLength = BitConverter.ToInt16(TargetInfo, offset_Target_Info);
                        //ThreadSafeAppendLog("[3]AvLength: " + AvLength);
                        offset_Target_Info += 2;
                        if ((flag_end & NB_COMPUTER_NAME) == NB_COMPUTER_NAME)
                        {
                            //ThreadSafeAppendLog("[3]Flag: " + flag_end);
                            byte[] NBName = new byte[AvLength];
                            Array.Copy(TargetInfo, offset_Target_Info, NBName, 0, AvLength);
                            //ThreadSafeAppendLog("[3]NB AV LD: " + AvLd);
                            //ThreadSafeAppendLog("[3]NB AV Length: " + AvLength);
                            //ThreadSafeAppendLog("[3]NBLength: " + NBName.Length);
                            string NBNameHex = BitConverter.ToString(NBName);
                            string test1NB = NBNameHex.Replace("-", "");
                            string test2NB = test1NB.Replace("00", "");
                            var rtfBytesNBName = FromHex(test2NB);
                            string NBNameString = Encoding.ASCII.GetString(rtfBytesNBName);
                            UI.ThreadSafeAppendLog("[3]NB Name String: " + NBNameString);
                            //string test2NB = test1NB.Replace("00", "");
                            //ThreadSafeAppendLog("[3]NETBIOS Computer Name: " + NBNameString);
                            offset_Target_Info += AvLength;
                        }
                        else if ((flag_end & NB_DOMAIN_NAME) == NB_DOMAIN_NAME)
                        {

                            byte[] NBDomainName = new byte[AvLength];
                            Array.Copy(TargetInfo, offset_Target_Info, NBDomainName, 0, AvLength);
                            string NBDomainNameHex = BitConverter.ToString(NBDomainName);
                            string test1NBDN = NBDomainNameHex.Replace("-", "");
                            string test2NBDN = test1NBDN.Replace("00", "");
                            var rtfBytesNBDomainName = FromHex(test2NBDN);
                            string NBDomainNameString = Encoding.ASCII.GetString(rtfBytesNBDomainName);
                            host.NTLMDomain = NBDomainNameString;
                            UI.ThreadSafeAppendLog("[4]NTLM Domain name: " + host.NTLMDomain);
                            offset_Target_Info += AvLength;
                        }
                        else if ((flag_end & DNS_COMPUTER_NAME) == DNS_COMPUTER_NAME)
                        {

                            byte[] DNSComputerName = new byte[AvLength];
                            Array.Copy(TargetInfo, offset_Target_Info, DNSComputerName, 0, AvLength);
                            string DNSComputerNameHex = BitConverter.ToString(DNSComputerName);
                            string test1DNSCN = DNSComputerNameHex.Replace("-", "");
                            string test2DNSCN = test1DNSCN.Replace("00", "");
                            var rtfBytesDNSComputerName = FromHex(test2DNSCN);
                            string DNSComputerNameString = Encoding.ASCII.GetString(rtfBytesDNSComputerName);
                            UI.ThreadSafeAppendLog("[2]Service Hostname: " + DNSComputerNameString);
                            //ThreadSafeAppendLog("[3]DNS Computer Name: " + DNSComputerNameString);
                            offset_Target_Info += AvLength;
                        }
                        else if ((flag_end & DNS_DOMAIN_NAME) == DNS_DOMAIN_NAME)
                        {

                            byte[] DNSDomainName = new byte[AvLength];
                            Array.Copy(TargetInfo, offset_Target_Info, DNSDomainName, 0, AvLength);
                            string DNSDomainNameHex = BitConverter.ToString(DNSDomainName);
                            string test1DNSDN = DNSDomainNameHex.Replace("-", "");
                            string test2DNSDN = test1DNSDN.Replace("00", "");
                            var rtfBytesDNSDomainName = FromHex(test2DNSDN);
                            string DNSDomainNameString = Encoding.ASCII.GetString(rtfBytesDNSDomainName);
                            host.OAuthDomain = DNSDomainNameString;
                            UI.ThreadSafeAppendLog("[3]OAuth Domain name: " + host.OAuthDomain);
                            //ThreadSafeAppendLog("[3]DNS Domain Name: " + DNSDomainNameString);
                            offset_Target_Info += AvLength;
                        }
                        else if ((flag_end & DNS_FOREST_NAME) == DNS_FOREST_NAME)
                        {

                            byte[] DNSForestName = new byte[AvLength];
                            Array.Copy(TargetInfo, offset_Target_Info, DNSForestName, 0, AvLength);
                            string DNSForestNameHex = BitConverter.ToString(DNSForestName);
                            string test1DNSFN = DNSForestNameHex.Replace("-", "");
                            string test2DNSFN = test1DNSFN.Replace("00", "");
                            var rtfBytesDNSForestName = FromHex(test2DNSFN);
                            string DNSForestNameString = Encoding.ASCII.GetString(rtfBytesDNSForestName);
                            UI.ThreadSafeAppendLog("[4] DNS Forest Name: " + DNSForestNameString);
                            //ThreadSafeAppendLog("[3]DNS Forest Name: " + DNSForestNameString);
                            offset_Target_Info += AvLength;
                        }
                        else
                        {
                            //ThreadSafeAppendLog("[3]Skipping");
                            offset_Target_Info += AvLength;
                        }
                        //ThreadSafeAppendLog("[3]Loop");
                    }
                    //ThreadSafeAppendLog("[3]Broken Out");
                }

            }
        }

        public static byte[] FromHex(string hex)
        {
            var result = new byte[hex.Length / 2];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public void addLinesFromFile(string path, string host)
        {
            URLs = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            string resource = "sLYNCy_WPF.DomainEnumeration.NTLMLocations." + path + ".txt";
            using (Stream resourceStream = assembly.GetManifestResourceStream(resource))
            {
                if (resourceStream == null)
                {
                    UI.ThreadSafeAppendLog("[4]NULL LINES");
                }
                else
                {
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string url = "https://" + host + line;
                            UI.ThreadSafeAppendLog("[3]URL: " + url);
                                URLs.Add(url);
                        }

                    }
                }
            }

        }

        public WebRequests CreateWebRequest(string validationURL)
        {
            WebRequests r = new WebRequests();
            r.url = validationURL;
            r.InitialiseRequest();
            r.request.Method = "GET";
            r.request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
            r.request.Timeout = 10000;
            return r;
        }

    }
}
