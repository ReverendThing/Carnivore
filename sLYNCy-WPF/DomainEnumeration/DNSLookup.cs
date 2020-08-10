using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class DNSLookup
    {
        public bool Skype = false;
        public bool Exchange = false;
        public bool ADFS = false;
        public bool RDWeb = false;
        public bool O365 = false;
        public string targetDomain;
        private List<SubdomainLookup> subdomains = new List<SubdomainLookup>();
        public ObservableCollection<Hostnames> enumeratedHostnames;
        public ObservableCollection<Hostnames> validatedHostnames;
        public List<string> validatedSubdomains = new List<string>();
        private object enumeratedHostnamesLock = new object();
        public MainWindow UI;
        //Add what it needs based on what's ticked then fire off singular DNSLookup Worker Thread
        //Then Fire up Validation Thread
        public ObservableCollection<Hostnames> DNSEnumerate()
        {
            foreach (Hostnames validHost in validatedHostnames)
            {
                validatedSubdomains.Add(validHost.Hostname);
            }
            //Do Task Async thing for each list based on which ones are ticked
            if (Skype)
            {
                addLinesFromFile("Skype", MicrosoftService.Skype);
            }
            if (Exchange)
            {
                addLinesFromFile("Exchange", MicrosoftService.Exchange);
            }
            if (ADFS)
            {
                addLinesFromFile("ADFS", MicrosoftService.ADFS);
            }
            if (RDWeb)
            {
                addLinesFromFile("RDWeb", MicrosoftService.RDWeb);
            }
            if (O365)
            {
                //So just add this to be validated - outlook.office exists - just need to then validate target domain...
                UI.ThreadSafeAppendLog("[4]Adding O365 - DNSLookup...");
                enumeratedHostnames.Add(new Hostnames() { Hostname = "outlook.office365.com", ipAddress = Dns.GetHostEntry("outlook.office365.com").AddressList[0], Service = MicrosoftService.Office365 });
            }

            UI.ThreadSafeAppendLog("[4]To enumerate count: " + subdomains.Count);
            bool test = EnumerateHostnames().Result;

            UI.ThreadSafeAppendLog("[4]Enumerated count: " + enumeratedHostnames.Count);
            return enumeratedHostnames;
        }

        public async Task<bool> EnumerateHostnames()
        {
            //Thread based on what's in list
            if (subdomains.Count > 0)
            {
                foreach (SubdomainLookup subdomainPair in subdomains)
                {
                    Task<IPAddress[]> taskA = Task.Run(() => CheckHostname(subdomainPair));
                    await taskA.ContinueWith(antecedent =>
                    {
                        IPAddress[] output = antecedent.Result;

                        if (output != null)
                        {
                            lock (enumeratedHostnamesLock)
                            {
                                foreach (IPAddress addr in output)
                                {
                                    UI.ThreadSafeAppendLog("[4]Subdomain: " + subdomainPair.subdomain + ":" + addr.ToString());
                                    enumeratedHostnames.Add(new Hostnames() { Hostname = subdomainPair.subdomain, ipAddress = addr, Service = subdomainPair.subdomainService });
                                }
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return true;
            } else
            {
                return false;
            }
            
        }


        public IPAddress[] CheckHostname(SubdomainLookup subdomainPair)
        {
            IPHostEntry ipEntry;
            IPAddress[] ipAddr;
            string subdomain = subdomainPair.subdomain;
            
            try
            {
                ipEntry = Dns.GetHostEntry(subdomain);
                ipAddr = ipEntry.AddressList;

                if (ipAddr != null)
                {
                    return ipAddr;
                } else
                {
                    return null;
                }

            }
            catch (SocketException sockception)
            {
                string socky = sockception.Message;
                if (socky.Contains("No such host is known"))
                {
                }
                return null;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        public void addLinesFromFile(string path, MicrosoftService subdomainSvc)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resource = "sLYNCy_WPF.DomainEnumeration.Subdomains." + path + ".txt";
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
                            string subdomainLine = line + "." + targetDomain;

                            if (validatedSubdomains.Contains(subdomainLine))
                            {

                            }
                            else
                            {
                                subdomains.Add(new SubdomainLookup() {subdomain = subdomainLine, subdomainService = subdomainSvc });
                            }

                            
                        }

                    }
                }
            }

        }


        public class SubdomainLookup
        {
            public string subdomain { get; set; }
            public MicrosoftService subdomainService { get; set; }
        }
    }
}
