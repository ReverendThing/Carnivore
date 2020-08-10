using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Data;
using System.Windows.Documents;
using static sLYNCy_WPF.Enums;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace sLYNCy_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //Global Vars
        public object addressBookUserNumberLock = new object();
        public static bool doWeHaveAnyUserAndPass = false;
        public static bool doWeHaveEnumeratedUsers = false;
        public static bool doWeHaveAnyDiscoveredFormat = false;
        public static object enumUsersLock = new object();
        public static object discoveredFormatLock = new object();
        private static int totalUsernamesToTest;
        private static int usernamesDone;
        public static object lockTotalUsernames = new object();
        public static object lockUsernamesRemaining = new object();
        public static object doWeHaveAnyUserAndPassLock = new object();
        public int oneTimeUserEnum = 0;
        public int oneTimePassSpray = 0;
        public UsernameFormat discoveredFormat = UsernameFormat.nullFormat;
        public List<ServiceInterface> serviceInterfaces = new List<ServiceInterface>();
        public List<PasswordSpray> passwordSprayers = new List<PasswordSpray>();
        public int postCredsUserNumber = 0;
        public int O365ListNumberThreads = 0;
        public int O365ListCountOff = 0;
        public int passwordSprayCountOff = 0;
        public List<string> O365EnumerationListPart2;
        public string O365Domain = "";
        public string surfaceChosen = "";
        public Boolean exclusionSuccess = false;
        public Boolean exclusionSuccessSpray = false;
        public List<SubdomainRecord> preValidatedSubdomainRecords;
        public string ntlmAuthURLSkype = "";
        public string localNTLMAuthURL = "";
        public List<SubdomainRecord> subdomainRecords;
        public int LyncEnabled = 0;
        public int PasswordSprayEnabled = 0;
        public int PostExploitEnabled = 0;
        public List<string> exclusions = new List<string>();
        public int O365Part2NumberThreads = 0;
        public int O365Part2CountOff = 0;
        public int O365Part1NumberThreads = 0;
        public int O365Part1CountOff = 0;
        public int lookupWorkerKillSwitch = 0;
        public int postCredsThreadCount = 0;
        public int added365 = 0;
        public int logOnceDNS = 0;
        public int validationTime = 0;
        public int enumHostnamesBefore = 0;
        public object validateSubdomainsLock = new object();
        public Dictionary<string, Hostnames> subdomainsToValidate;
        public string verbosityLevel = "";
        public int validatingThreads = 0;
        public int countOffValidatingThreads = 0;
        public int alreadySaved = 0;
        public string communicationLink;
        public int alreadySaidOauth = 0;
        public string messagingLink;
        public string locationLink;
        public List<MessagingObject> userDetailsForMessaging = new List<MessagingObject>();
        public int onlyOneContacts = 0;
        public string NTLMDomainName = "";
        public string OAuthSprayDomain = "";
        public int onlyOneFull = 0;
        public int contactsLookupCount = 0;
        public int fullLookupCount = 0;
        public int oAuthOutputYet = 0;
        public string contactsLookupToken;
        public string fullLookupToken;
        public int oAuthFinalThread = 0;
        public int oAuthSprayUsernameCount = 0;
        public int oAuthSprayPosition = 0;
        public int NTLMSprayUsernameCount = 0;
        public int NTLMSprayPosition = 0;
        public int enumO365HostnamesBefore = 0;
        public int killNTLM = 0;
        public Thread newWorker;
        public Thread newPostExploitWorker;
        public Thread newWorkerChosenList;
        public Thread newWorkerSmartStart;
        public Thread newWorkerSpray;
        public Thread newWorkerSpray2;
        public Thread newWorkerNTLMSpray;
        public int killOauth = 0;
        public List<string> FormatsO365;
        public string O365ChosenFormat = "";
        public int O365Switch = 0;
        public string targetDomain;
        public string passwordToTest;
        public bool enumerateInternalDomain;
        public ObservableCollection<DataRecord> dataList = new ObservableCollection<DataRecord>();
        public ObservableCollection<Hostnames> enumeratedHostnames = new ObservableCollection<Hostnames>();
        public ObservableCollection<CredentialsRecord> accessTokens = new ObservableCollection<CredentialsRecord>();
        public ObservableCollection<MeetingsObject> meetingsRecords = new ObservableCollection<MeetingsObject>();
        public List<O365CredentialsRecord> O365CredsRecords = new List<O365CredentialsRecord>();
        public int waitDomains = 1;
        public static int foundNTLMSkype = 0;
        public static int foundNTLMExchange = 0;
        public string realAddress;
        public string chosenFormat;
        public string targetSuffix;
        public Thread newWorkerInternalDomains;
        public object enumeratedHostnamesLock = new object();
        public object O365EnumList = new object();
        public object saveHostnamesLock = new object();
        public object CheckHostnameLock = new object();
        public object saveUsersLock = new object();
        public object saveMeetingsLock = new object();
        public object O365PickFormat = new object();
        public object O365EnumPart2 = new object();
        public object O365Part2Lock = new object();
        public object O365Part1Lock = new object();
        public object O365Lock = new object();
        public bool enumeratedLockWasTaken = false;
        public int killEnumO365 = 0;
        public static Stopwatch timer;
        public static TimeSpan timeTaken;
        public List<string> usernames_jjs;
        public List<string> usernames_jjsmith;
        public List<string> usernames_john_smith;
        public List<string> usernames_john;
        public List<string> usernames_johnjs;
        public List<string> usernames_johns;
        public Dictionary<string, List<string>> contactsThreadLookups = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> fullThreadLookups = new Dictionary<string, List<string>>();
        public List<string> addressThreadLookups;
        public List<string> usernames_johnsmith;
        public List<string> usernames_jsmith;
        public List<string> usernames_smithj;
        public List<string> usernames;
        public List<string> usernamesToTest;
        public string loggingDirectory;
        public CounterUpdate counter = new CounterUpdate();

        public MainWindow()
        {
            InitializeComponent();
            counter.Initialise();
            EnumeratedHostnames.ItemsSource = enumeratedHostnames;
            Credentials.ItemsSource = accessTokens;
            DataRecords.ItemsSource = dataList;
            MeetingRecords.ItemsSource = meetingsRecords;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            string resource1 = "sLYNCy_WPF.Newtonsoft.Json.dll";
            EmbeddedAssembly.Load(resource1, "Newtonsoft.Json.dll");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            ThreadSafeAppendLog("[1]Carnivore started at: " + DateTime.Now);
            ThreadSafeAppendLog("[1]Output will be automatically logged to: " + Directory.GetCurrentDirectory());
            string fileName = loggingDirectory + "_ValidCredentials.csv";

            ChooseVerbosity.Items.Add("1 - Low");
            ChooseVerbosity.Items.Add("2 - Normal");
            ChooseVerbosity.Items.Add("3 - Verbose");
            ChooseVerbosity.Items.Add("4 - Debugging");
            ChooseVerbosity.SelectedItem = "2 - Normal";
        }

        public bool GetDoWeHaveAnyValidUserAndPass()
        {
            lock (doWeHaveAnyUserAndPassLock)
            {
                //Only unlock post compromise if we have valid user + pass
                if (doWeHaveAnyUserAndPass)
                {
                    //AND a valid Skype host - AND have discovered valid "real" skype host
                    if (enumeratedHostnames.Any(x => x.Service == MicrosoftService.Skype && x.RealLync == true))
                    {
                        Hostnames tempHost = (enumeratedHostnames.Where(x => x.Service == MicrosoftService.Skype && x.RealLync == true)).First();

                        if (tempHost != null)
                        {
                            //AND the skype host spray URL is the UCWA OAUTH authentication endpoint
                            string tempSprayURL = tempHost.SprayURL.Url;
                            if (tempSprayURL != null)
                            {
                                if (tempHost.SprayURL.Url.Contains("oauthtoken"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                //Otherwise false
                return false;
            }
        }

        public bool GetDoWeHaveMeetingRecords()
        {
            if (meetingsRecords.Count > 0)
            {
                return true;
            }
            return false;

        }

        public bool GetO365Federated()
        {
            if (enumeratedHostnames.Any(p => p.Federated == "Y"))
            {
                return true;
            }
            return false;
        }

        public bool GetO365NotFederated()
        {
            if (enumeratedHostnames.Any(p => p.Federated == "Y"))
            {
                return false;
            }
            else if (enumeratedHostnames.Any(p => p.O365 == "Y"))
            {
                return true;
            }
            return false; ;
        }

        public static void SetDoWeHaveAnyUserAndPass(bool value)
        {
            lock (doWeHaveAnyUserAndPassLock)
            {
                doWeHaveAnyUserAndPass = value;
            }
        }

        public static bool GetDoWeHaveEnumeratedUsers()
        {
            lock (enumUsersLock)
            {
                return doWeHaveEnumeratedUsers;
            }
        }

        public static void SetDoWeHaveEnumeratedUsers(bool value)
        {
            lock (enumUsersLock)
            {
                doWeHaveEnumeratedUsers = value;
            }
        }

        public static bool GetDoWeHaveAnyDiscoveredFormat()
        {
            lock (discoveredFormatLock)
            {
                return doWeHaveAnyDiscoveredFormat;
            }
        }

        public static void SetDoWeHaveAnyDiscoveredFormat(bool value)
        {
            lock (discoveredFormatLock)
            {
                doWeHaveAnyDiscoveredFormat = value;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        public static int getUsernamesDone()
        {
            lock (lockUsernamesRemaining)
            {
                return usernamesDone;
            }

        }

        public static void addToUsernamesDone()
        {
            lock (lockUsernamesRemaining)
            {
                usernamesDone++;
            }
        }

        public static int getTotalUsernamesToTest()
        {
            lock (lockTotalUsernames)
            {
                return totalUsernamesToTest;
            }
        }

        public static void removeFromUsernamesRemaining()
        {
            lock (lockUsernamesRemaining)
            {
                usernamesDone--;
            }
        }

        public static void setUsernamesDone(int usernamesNumber)
        {
            lock (lockUsernamesRemaining)
            {
                usernamesDone = usernamesNumber;
            }
        }

        public static void setTotalUsernames(int usernamesNumber)
        {
            lock (lockTotalUsernames)
            {
                totalUsernamesToTest = usernamesNumber;
            }
        }

        public void lockUI(SendingWindow sender)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (sender)
                {
                    case SendingWindow.AddressBook:
                        PostExploitRetrieveDataFrom.IsEnabled = false;
                        PostExploitRetrieveDataData.IsEnabled = false;
                        PostExploitGo.IsEnabled = false;
                        PostExploitExport.IsEnabled = false;

                        Lync.IsEnabled = false;
                        //Can cancel on go
                        PostExploitCancel.IsEnabled = true;
                        PasswordSpray.IsEnabled = false;
                        MeetingSnooper.IsEnabled = false;
                        Enumeration.IsEnabled = false;
                        Common3Chars.IsEnabled = false;
                        All3Chars.IsEnabled = false;
                        break;
                    case SendingWindow.DomainEnum:
                        Lync.IsEnabled = false;
                        PasswordSpray.IsEnabled = false;
                        PostExploitation.IsEnabled = false;
                        MeetingSnooper.IsEnabled = false;
                        TargetDomain.IsEnabled = false;
                        EnumerateButton.IsEnabled = false;
                        ExportButton.IsEnabled = false;
                        break;
                    case SendingWindow.UserEnum:
                        Enumeration.IsEnabled = false;
                        PasswordSpray.IsEnabled = false;
                        PostExploitation.IsEnabled = false;
                        MeetingSnooper.IsEnabled = false;
                        TargetDomain.IsEnabled = false;
                        ExpoCreds.IsEnabled = false;
                        UserEnumButton.IsEnabled = false;
                        UserEnumSurfacePicker.IsEnabled = false;
                        IndividualUser.IsEnabled = false;
                        IndivUser.IsEnabled = false;
                        SmartEnumerationCheckbox.IsEnabled = false;
                        SmartAdvanced.IsEnabled = false;
                        UsernameListCheckbox.IsEnabled = false;
                        UsernameListBox.IsEnabled = false;
                        ChoosePrebuilt.IsEnabled = false;
                        ChooseUserList.IsEnabled = false;
                        PasswordBox.IsEnabled = false;
                        break;
                    case SendingWindow.PasswordSpray:
                        Enumeration.IsEnabled = false;
                        Lync.IsEnabled = false;
                        PostExploitation.IsEnabled = false;
                        MeetingSnooper.IsEnabled = false;
                        TargetDomain.IsEnabled = false;
                        ExpoCreds.IsEnabled = false;
                        Lync.IsEnabled = false;
                        PasswordSprayPause.IsEnabled = true;
                        PasswordSprayStop.IsEnabled = true;
                        PasswordSprayGo.IsEnabled = false;
                        PasswordSpraySurfacePicker.IsEnabled = false;
                        UseDiscoveredFormat.IsEnabled = false;
                        UseChosenFormat.IsEnabled = false;
                        ChosenFormatSpray.IsEnabled = false;
                        UsernameListCheckboxSpray.IsEnabled = false;
                        UsernameListBoxSpray.IsEnabled = false;
                        ChoosePrebuiltSpray.IsEnabled = false;
                        ChooseUserListSpray.IsEnabled = false;
                        EnumeratedUsersCheckboxSpray.IsEnabled = false;
                        PassLabelSpray.IsEnabled = false;
                        PasswordFromSpray.IsEnabled = false;
                        break;
                    case SendingWindow.MeetingSnooper:
                        MeetingSnoop.IsEnabled = false;
                        MeetingSnooperAllUsers.IsEnabled = false;
                        MeetingSnooperSelectedUser.IsEnabled = false;
                        PasswordSpray.IsEnabled = false;
                        ExpoMeetings.IsEnabled = false;
                        Enumeration.IsEnabled = false;
                        Lync.IsEnabled = false;
                        PostExploitation.IsEnabled = false;
                        break;
                }
            });

        }

        public void unlockUI()
        {
            this.Dispatcher.Invoke(() =>
            {
                PostExploitRetrieveDataFrom.IsEnabled = true;
                PostExploitRetrieveDataData.IsEnabled = true;
                PostExploitGo.IsEnabled = true;
                PostExploitExport.IsEnabled = true;
                EnumerateButton.IsEnabled = true;
                MeetingSnoop.IsEnabled = true;
                MeetingSnooperAllUsers.IsEnabled = true;
                MeetingSnooperSelectedUser.IsEnabled = true;
                Enumeration.IsEnabled = true;
                PostExploitCancel.IsEnabled = false;
                Common3Chars.IsEnabled = true;
                All3Chars.IsEnabled = true;
                NormalCreds.IsEnabled = true;
                PostExploitCancel.IsEnabled = false;
                //Always do these - check for what should be enabled and re-enable

                //So - if we have an NTLM or OAuth domain - Or have O365 that ISNT federated - enable enum/spray if we have any services
                //Only remaining problem is will unlock on-prem if we don't do active - but have done O365
                if (enumeratedHostnames.Any(k => k.NTLMDomain != "" && k.NTLMDomain != null) || enumeratedHostnames.Any(l => l.OAuthDomain != "" && l.OAuthDomain != null))
                {
                    if (LyncEnabled == 1)
                    {
                        Lync.IsEnabled = true;
                    }
                    if (PasswordSprayEnabled == 1)
                    {
                        PasswordSpray.IsEnabled = true;
                    }
                }

                if (GetO365Federated())
                {
                    Lync.IsEnabled = true;
                }
                if (GetO365NotFederated())
                {
                    PasswordSpray.IsEnabled = true;
                    Lync.IsEnabled = true;
                }
                if (GetDoWeHaveAnyValidUserAndPass())
                {
                    PostExploitation.IsEnabled = true;
                    MeetingSnooper.IsEnabled = true;
                }
                if (GetDoWeHaveMeetingRecords())
                {
                    ExpoMeetings.IsEnabled = true;
                }
                //Enable the export button if we have something to export
                if (enumeratedHostnames.Count > 0)
                {
                    ExportButton.IsEnabled = true;
                }
                else
                {
                    //Only enable target domain if there are no enumerated hostnames - can't change once we've started (unless clear)
                    TargetDomain.IsEnabled = true;
                }

                if (UserEnumPauseButton.Content != "Resume")
                {
                    //Maybe just always allow Enumerate button? You're welcome to keep on enumerating - pick different options - now active, now not - run again
                    //No negative as we still then check if doubling up
                    EnumerateButton.IsEnabled = true;
                    Enumeration.IsEnabled = true;
                    UserEnumButton.IsEnabled = true;
                    //Maybe just always enable these? If the tab is locked - won't see anyway
                    NormalCreds.IsEnabled = true;

                    UserEnumPauseButton.IsEnabled = false;
                    UserEnumStopButton.IsEnabled = false;
                    PasswordBox.IsEnabled = true;
                    UserEnumSurfacePicker.IsEnabled = true;
                    //Always do these - check for what should be enabled and re-enable
                    IndividualUser.IsEnabled = true;
                    if (IndividualUser.IsChecked == true)
                        IndivUser.IsEnabled = true;
                    SmartEnumerationCheckbox.IsEnabled = true;
                    if (SmartEnumerationCheckbox.IsChecked == true)
                        SmartAdvanced.IsEnabled = true;
                    UsernameListCheckbox.IsEnabled = true;
                    if (UsernameListCheckbox.IsChecked == true)
                    {
                        UsernameListBox.IsEnabled = true;
                        ChoosePrebuilt.IsEnabled = true;
                        ChooseUserList.IsEnabled = true;
                    }
                    if (PostExploitEnabled == 1)
                    {
                        PostExploitation.IsEnabled = true;
                    }

                    if (accessTokens.Count > 0)
                    {
                        ExpoCreds.IsEnabled = true;
                    }

                    //Enable the export button if we have something to export
                    if (enumeratedHostnames.Count > 0)
                    {
                        ExportButton.IsEnabled = true;
                    }
                    else
                    {
                        //Only enable target domain if there are no enumerated hostnames - can't change once we've started (unless clear)
                        TargetDomain.IsEnabled = true;
                    }
                }

                if (PasswordSprayPause.Content != "Resume")
                {
                    PasswordSprayPause.IsEnabled = false;
                    PasswordSprayStop.IsEnabled = false;
                    PasswordSprayGo.IsEnabled = true;
                    PasswordSpraySurfacePicker.IsEnabled = true;
                    Enumeration.IsEnabled = true;

                    if (PostExploitEnabled == 1)
                    {
                        PostExploitation.IsEnabled = true;
                    }
                    PassLabelSpray.IsEnabled = true;
                    PasswordFromSpray.IsEnabled = true;
                    if (GetDoWeHaveAnyDiscoveredFormat())
                        UseDiscoveredFormat.IsEnabled = true;
                    UseChosenFormat.IsEnabled = true;
                    UsernameListCheckboxSpray.IsEnabled = true;
                    if (GetDoWeHaveEnumeratedUsers())
                        EnumeratedUsersCheckboxSpray.IsEnabled = true;

                    if (UseChosenFormat.IsChecked == true)
                    {
                        ChosenFormatSpray.IsEnabled = true;
                    }
                    if (UsernameListCheckboxSpray.IsChecked == true)
                    {
                        UsernameListBoxSpray.IsEnabled = true;
                        ChoosePrebuiltSpray.IsEnabled = true;
                        ChooseUserListSpray.IsEnabled = true;
                    }
                }

            });

        }

        public bool validateArgs(SendingButton sender)
        {
            switch (sender)
            {
                case SendingButton.DomainEnumerate:
                    if (targetDomain.Contains("http") || targetDomain.Contains("https"))
                    {
                        ThreadSafeAppendLog("[1]Please enter the target domain without HTTP or HTTPS");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
            return false;
        }



        private void EnumerateButton_Click(object sender, RoutedEventArgs e)
        {
            preValidatedSubdomainRecords = new List<SubdomainRecord>();
            subdomainRecords = new List<SubdomainRecord>();
            lockUI(SendingWindow.DomainEnum);
            logOnceDNS = 0;
            alreadySaved = 0;
            //Get domain from text box
            targetDomain = TargetDomain.Text;

            loggingDirectory = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("dd.MM.hh.mm") + "." + targetDomain;
            ThreadSafeAppendLog("[1][*] Domain enumeration beginning...");

            if (validateArgs(SendingButton.DomainEnumerate))
            {

                //Are we enumerating internal domain info?
                if (DiscoverDomains.IsChecked == true)
                {
                    ThreadSafeAppendLog("[1]Enumerating subdomain DNS entries and validating...");
                    enumerateInternalDomain = true;

                }
                else
                {
                    ThreadSafeAppendLog("[1]Enumerating subdomain DNS entries and validating...");
                    if (DiscoverO365.IsChecked == true)
                    {
                        if (DiscoverRDWeb.IsChecked == true || DiscoverADFS.IsChecked == true || DiscoverExchange.IsChecked == true || DiscoverSkype.IsChecked == true)
                        {
                            ThreadSafeAppendLog("[1]WARNING if you do not select Discover Internal Domain Information and you HAVE selected O365 enumeration - Carnivore will use the O365 domain as the internal domain for on-premises users...");
                            ThreadSafeAppendLog("[1]It is HIGHLY UNLIKELY these will match - you should re-run Enumeration with Discover Internal Domain Information if you intend on assessing on-premises servers...");
                        }
                    }
                    enumerateInternalDomain = false;
                }

                DNSLookup lookup = new DNSLookup();
                lookup.UI = this;
                lookup.targetDomain = targetDomain;
                lookup.enumeratedHostnames = new ObservableCollection<Hostnames>();
                lookup.validatedHostnames = enumeratedHostnames;
                if (lookup.validatedHostnames == null)
                    lookup.validatedHostnames = new ObservableCollection<Hostnames>();
                if (DiscoverSkype.IsChecked == true)
                    lookup.Skype = true;
                if (DiscoverExchange.IsChecked == true)
                    lookup.Exchange = true;
                if (DiscoverO365.IsChecked == true)
                    lookup.O365 = true;
                if (DiscoverADFS.IsChecked == true)
                    lookup.ADFS = true;
                if (DiscoverRDWeb.IsChecked == true)
                    lookup.RDWeb = true;

                EnumerateSubdomains(lookup);
            }
            else
            {
                unlockUI();
            }
        }

        public async void EnumerateSubdomains(DNSLookup lookup)
        {
            Task taskE = Task.Run(() =>
            {
                EnumerateAndValidateTask(lookup);
            });

        }

        public void EnumerateAndValidateTask(DNSLookup lookup)
        {
            //Need to not re-enumerate things already enumerated - basically if validatedHostnames values contains subdomain don't bother
            ThreadSafeAppendLog("[2]Looking up subdomain DNS records...");
            ObservableCollection<Hostnames> enumHostames = lookup.DNSEnumerate();
            ThreadSafeAppendLog("[4]Enumerated Hostnames Count: " + enumHostames.Count);
            //So now need to setup new validation object - with enumHostnames (by design now only newly enumerated hostnames)
            //That needs to return validHostnames
            //Then here add any new validated hostnames to enumeratedHostnames
            Validation validation = new Validation();
            validation.UI = this;
            validation.targetDomain = targetDomain;
            validation.toValidate = enumHostames;
            ThreadSafeAppendLog("[2]Validating subdomain records...");
            validation.Validate();

            //DO INTERNAL DOMAIN GATHERING - IF SELECTED
            if (enumerateInternalDomain)
            {
                if (enumeratedHostnames.Count > 0)
                {
                    ThreadSafeAppendLog("[2]Enumerating Internal Domain Information...");
                    NTLMDiscovery domainInformation = new NTLMDiscovery();
                    //Have already been updated - so this is now all
                    domainInformation.hostnamesForDomainInformationEnumeration = enumeratedHostnames;
                    domainInformation.UI = this;
                    domainInformation.GetDomainInformation();
                }
                else
                {
                    ThreadSafeAppendLog("[2]No validated hostnames to enumerate...");
                }
            }

            //THEN LOOK THROUGH EACH HOST IN ENUMERATEDHOSTNAMES - IF ANY ARE O365 - Y - OR IF ANY HAVE NTLM SET RELEVANT LYNCENABLED ETC - AND ADD SERVICES - AND UNLOCK WINDOWS?
            ThreadSafeAppendLog("[1]Finished subdomain enumeration and validation...");
            saveEnumeratedHostnames(null, SaveType.autoLog);
            //THEN CALL UNLOCK UI - WHICH WILL NOW BE BASED ON THIS RUN - AND PREVIOUS - WHETHER OR NOT WE DID INTERNAL THIS TIME
            unlockUI();
        }






        public void ThreadSafeAppendLog(string sUpdateText)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    string temp = (string)ChooseVerbosity.SelectedItem;
                    if (temp == null)
                    {
                        temp = "1";
                    }

                    if (temp.Contains("1"))
                    {
                        verbosityLevel = "1";
                    }
                    else if (temp.Contains("2"))
                    {
                        verbosityLevel = "2";
                    }
                    else if (temp.Contains("3"))
                    {
                        verbosityLevel = "3";
                    }
                    else if (temp.Contains("4"))
                    {
                        verbosityLevel = "4";
                    }
                });


                switch (verbosityLevel)
                {
                    case "1":
                        if (sUpdateText.Contains("[1]"))
                        {
                            //If verbosity level 1 - only output ones starting 1
                            string sUpdateText1 = sUpdateText.Replace("[1]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            outputText(sUpdateText1);
                        }
                        break;
                    case "2":
                        if (sUpdateText.Contains("[1]") || sUpdateText.Contains("[2]"))
                        {
                            //Verbosity level 2 - output 1 AND 2
                            string sUpdateText1 = sUpdateText.Replace("[1]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText2 = sUpdateText1.Replace("[2]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            outputText(sUpdateText2);
                        }
                        break;
                    case "3":
                        if (sUpdateText.Contains("[1]") || sUpdateText.Contains("[2]") || sUpdateText.Contains("[3]"))
                        {
                            //Verbosity level 3 - output 1, 2 or 3
                            //Will just do nothing if its 1 not 3 etc
                            string sUpdateText1 = sUpdateText.Replace("[1]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText2 = sUpdateText1.Replace("[2]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText3 = sUpdateText2.Replace("[3]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            outputText(sUpdateText3);
                        }
                        break;
                    case "4":
                        if (sUpdateText.Contains("[1]") || sUpdateText.Contains("[2]") || sUpdateText.Contains("[3]") || sUpdateText.Contains("[4]"))
                        {
                            //Verbosity level 3 - output 1, 2 or 3
                            //Will just do nothing if its 1 not 3 etc
                            string sUpdateText1 = sUpdateText.Replace("[1]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText2 = sUpdateText1.Replace("[2]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText3 = sUpdateText2.Replace("[3]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            string sUpdateText4 = sUpdateText3.Replace("[4]", "[" + DateTime.Now.ToShortTimeString() + "] ");
                            outputText(sUpdateText4);
                        }
                        break;
                }

            }
            catch (Exception e)
            {

            }
        }

        public void outputText(string sUpdateText)
        {
            if (sUpdateText.Contains("[*]"))
            {
                //Add phrase with colour
                if (!Dispatcher.CheckAccess())
                {
                    OutputBox.Dispatcher.BeginInvoke((System.Action)(() =>
                    {
                        //ThreadSafeAppendLog("Here");


                        TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                        rangeOfText1.Text = sUpdateText;
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                        OutputBox.AppendText(Environment.NewLine);

                        OutputBox.ScrollToEnd();
                    }));
                }
                else
                {

                    TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                    rangeOfText1.Text = sUpdateText;
                    rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                    OutputBox.AppendText(Environment.NewLine);



                    OutputBox.ScrollToEnd();
                }
            }
            else if (sUpdateText.Contains("[$]"))
            {
                //Add phrase with colour
                if (!Dispatcher.CheckAccess())
                {
                    OutputBox.Dispatcher.BeginInvoke((System.Action)(() =>
                    {


                        TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                        rangeOfText1.Text = sUpdateText;
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Brown);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                        OutputBox.AppendText(Environment.NewLine);

                        OutputBox.ScrollToEnd();
                    }));
                }
                else
                {

                    TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                    rangeOfText1.Text = sUpdateText;
                    rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Brown);
                    rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                    OutputBox.AppendText(Environment.NewLine);



                    OutputBox.ScrollToEnd();
                }
            }
            else if (sUpdateText.Contains("[!]"))
            {
                //Add phrase with colour
                if (!Dispatcher.CheckAccess())
                {
                    OutputBox.Dispatcher.BeginInvoke((System.Action)(() =>
                    {
                        TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                        rangeOfText1.Text = sUpdateText;
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                        OutputBox.AppendText(Environment.NewLine);



                        OutputBox.ScrollToEnd();
                    }));
                }
                else
                {
                    TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                    rangeOfText1.Text = sUpdateText;
                    rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                    rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                    OutputBox.AppendText(Environment.NewLine);



                    OutputBox.ScrollToEnd();
                }
            }
            else
            {
                //Add as normal
                if (!Dispatcher.CheckAccess())
                {
                    OutputBox.Dispatcher.BeginInvoke((System.Action)(() =>
                    {

                        TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                        rangeOfText1.Text = sUpdateText;
                        rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                        rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                        OutputBox.AppendText(Environment.NewLine);

                        OutputBox.ScrollToEnd();
                    }));
                }
                else
                {
                    TextRange rangeOfText1 = new TextRange(OutputBox.Document.ContentEnd, OutputBox.Document.ContentEnd);
                    rangeOfText1.Text = sUpdateText;
                    rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                    OutputBox.AppendText(Environment.NewLine);

                    OutputBox.ScrollToEnd();
                }
            }
        }


        public void saveEnumeratedHostnames(string fileName, SaveType saveType)
        {
            lock (saveHostnamesLock)
            {
                try
                {
                    if (fileName == null)
                        fileName = loggingDirectory + "_ValidHostnames.csv";

                    StringBuilder hostnamesCSV = new StringBuilder();
                    hostnamesCSV.Append("Hostname, IP Address, Service, Domain Name, User Enumeration URL, Password Spray URL, Federated O365" + Environment.NewLine);
                    lock (enumeratedHostnamesLock)
                    {
                        foreach (Hostnames host in enumeratedHostnames)
                        {
                            if (host.OAuthDomain != null && host.OAuthDomain != "")
                                hostnamesCSV.Append(host.Hostname + "," + host.ipAddress + "," + host.Service + "," + host.OAuthDomain + "," + host.EnumURL.Url + "," + host.SprayURL.Url + "," + host.Federated + Environment.NewLine);
                            else
                                hostnamesCSV.Append(host.Hostname + "," + host.ipAddress + "," + host.Service + "," + host.NTLMDomain + "," + host.EnumURL.Url + "," + host.SprayURL.Url + "," + host.Federated + Environment.NewLine);

                        }
                    }



                    System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                    file.WriteLine(hostnamesCSV.ToString());
                    if (saveType == SaveType.expo)
                        ThreadSafeAppendLog("[2]Hostnames file written to: " + fileName);
                    file.Close();

                }
                catch (IOException ioExcept)
                {
                    if (ioExcept.ToString().Contains("another process"))
                    {
                        ThreadSafeAppendLog("[1]The file is currently open and cannot be saved...");
                    }
                }
                catch (Exception exctsfdt)
                {
                    ThreadSafeAppendLog("[1]" + exctsfdt.ToString());
                }


                //Save Output Log:
                string fileNameOutput = loggingDirectory + "_OutputLog.txt";

                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        TextRange textRange = new TextRange(OutputBox.Document.ContentStart, OutputBox.Document.ContentEnd);
                        using (FileStream fileOutput = new FileStream(fileNameOutput, FileMode.Create))
                        {
                            textRange.Save(fileOutput, System.Windows.DataFormats.Text);
                        }
                    }
                    catch (IOException ioExcept)
                    {
                        if (ioExcept.ToString().Contains("another process"))
                        {
                            ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                        }
                    }
                    catch (Exception rtf)
                    {
                        ThreadSafeAppendLog("[1]" + rtf.ToString());
                    }
                });

            }
        }

        public void saveAddressBook(string fileName, SaveType saveType)
        {
            if (fileName == null)
                fileName = loggingDirectory + "_AddressBook.csv";


            StringBuilder dataRecordsCSV = new StringBuilder();
            dataRecordsCSV.Append("Name, Sip Username, Email Address, Title, Department, Office, Presence, Phone Number, Note" + Environment.NewLine);
            foreach (DataRecord record in dataList)
            {
                string presence = "";
                if (record.Presence != null)
                {
                    string prePresence = record.Presence;
                    presence = prePresence.Replace(",", " ");
                }
                else
                {
                    presence = record.Presence;
                }
                dataRecordsCSV.Append(record.Name + "," + record.SIPUsername + "," + record.EmailAddress + "," + record.Title + "," + record.Department + "," + record.Office + "," + presence + "," + record.PhoneNumber + "," + record.Note + Environment.NewLine);
            }

            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                file.WriteLine(dataRecordsCSV.ToString());
                file.Close();
                if (saveType == SaveType.expo)
                    ThreadSafeAppendLog("[2]Address Book written to: " + fileName);
            }
            catch (IOException ioExcept)
            {
                if (ioExcept.ToString().Contains("another process"))
                {
                    ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                }
            }
            catch (Exception exctsfdt)
            {
                ThreadSafeAppendLog("[1]" + exctsfdt.ToString());
            }

            string fileNameOutput = loggingDirectory + "_OutputLog.txt";

            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    TextRange textRange = new TextRange(OutputBox.Document.ContentStart, OutputBox.Document.ContentEnd);
                    using (FileStream fileOutput = new FileStream(fileNameOutput, FileMode.Create))
                    {
                        textRange.Save(fileOutput, System.Windows.DataFormats.Text);
                    }
                }
                catch (IOException ioExcept)
                {
                    if (ioExcept.ToString().Contains("another process"))
                    {
                        ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                    }
                }
                catch (Exception rtf)
                {
                    ThreadSafeAppendLog("[1]" + rtf.ToString());
                    ThreadSafeAppendLog("[4] This error.");
                }
            });
        }

        public void saveValidUsersAndCreds(string fileName, SaveType saveType)
        {
            lock (saveUsersLock)
            {
                try
                {
                    if (fileName == null)
                        fileName = loggingDirectory + "_ValidCredentials.csv";
                    string fileNameOutput = loggingDirectory + "_OutputLog.txt";

                    StringBuilder credentialsCSV = new StringBuilder();
                    credentialsCSV.Append("Username, Password, O365 MFA, Service, Sip Enabled, Account Disabled, Password Expired, Server Error Authenticating, Access Token" + Environment.NewLine);
                    foreach (CredentialsRecord record in accessTokens)
                    {
                        credentialsCSV.Append(record.Username + "," + record.Password + "," + record.MFA + "," + record.Service + "," + record.SipEnabled + "," + record.AccountDisabled + "," + record.PasswordExpired + "," + record.ServerError + "," + record.Token + Environment.NewLine);
                    }


                    System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                    file.WriteLine(credentialsCSV.ToString());
                    if (saveType == SaveType.expo)
                        ThreadSafeAppendLog("[1]Credentials records written to: " + fileName);
                    file.Close();

                    //Save Output Log
                    this.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            TextRange textRange = new TextRange(OutputBox.Document.ContentStart, OutputBox.Document.ContentEnd);
                            using (FileStream fileOutput = new FileStream(fileNameOutput, FileMode.Create))
                            {
                                textRange.Save(fileOutput, System.Windows.DataFormats.Text);
                            }
                        }
                        catch (IOException ioExcept)
                        {
                            if (ioExcept.ToString().Contains("another process"))
                            {
                                ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                            }
                        }
                        catch (Exception rtf)
                        {
                            ThreadSafeAppendLog("[1]" + rtf.ToString());
                        }
                    });
                }
                catch (IOException ioExcept)
                {
                    if (ioExcept.ToString().Contains("another process"))
                    {
                        ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                    }
                }
                catch (Exception exctsfdt)
                {
                    ThreadSafeAppendLog("[1]" + exctsfdt.ToString());
                }

            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            //Popup file dialogue to get location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "ValidHostnames.csv";
            saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() != true)
                return;

            // Get the output file name from the text box.
            string fileName = saveFileDialog.FileName;
            if (fileName != null)
                saveEnumeratedHostnames(fileName, SaveType.expo);
            else
                ThreadSafeAppendLog("[1]No valid filename...");
        }

        public void saveValidMeetings(string fileName, SaveType saveType)
        {
            lock (saveMeetingsLock)
            {
                if (fileName == null)
                    fileName = loggingDirectory + "_ValidMeetings.csv";

                StringBuilder meetingsCSV = new StringBuilder();
                meetingsCSV.Append("Username, Conference ID, Subject, Attendees, Meeting Expires, Join URL, Lobby Bypass" + Environment.NewLine);
                foreach (MeetingsObject meeting in meetingsRecords)
                {
                    meetingsCSV.Append(meeting.UserRecord.Username + "," + meeting.ConfID + "," + meeting.Subject + "," + meeting.Attendees + "," + meeting.ExpirationTime + "," + meeting.JoinURL + "," + meeting.LobbyBypass + Environment.NewLine);
                }

                try
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                    file.WriteLine(meetingsCSV.ToString());
                    if (saveType == SaveType.expo)
                        ThreadSafeAppendLog("Meetings records written to: " + fileName);

                    file.Close();

                    string fileNameOutput = loggingDirectory + "_OutputLog.txt";

                    this.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            TextRange textRange = new TextRange(OutputBox.Document.ContentStart, OutputBox.Document.ContentEnd);
                            using (FileStream fileOutput = new FileStream(fileNameOutput, FileMode.Create))
                            {
                                textRange.Save(fileOutput, System.Windows.DataFormats.Text);
                            }
                        }
                        catch (IOException ioExcept)
                        {
                            if (ioExcept.ToString().Contains("another process"))
                            {
                                ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                            }
                        }
                        catch (Exception rtf)
                        {
                            ThreadSafeAppendLog("[1]" + rtf.ToString());
                        }
                    });

                }
                catch (IOException ioExcept)
                {
                    if (ioExcept.ToString().Contains("another process"))
                    {
                        ThreadSafeAppendLog("[1]The file is currently open and autologging cannot save...");
                    }
                }
                catch (Exception exctsfdt)
                {
                    ThreadSafeAppendLog("[1]" + exctsfdt.ToString());
                }
            }
        }


        private void SmartEnumerationCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (SmartEnumerationCheckbox.IsChecked == true)
            {
                UsernameListCheckbox.IsChecked = false;
                ChooseUserList.IsEnabled = false;
                ChoosePrebuilt.IsEnabled = false;
                UsernameListBox.IsEnabled = false;
                SmartAdvanced.IsEnabled = true;
                IndivUser.IsEnabled = false;
                IndividualUser.IsChecked = false;
            }
        }

        private void UsernameListCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameListCheckbox.IsChecked == true)
            {
                SmartEnumerationCheckbox.IsChecked = false;
                ChooseUserList.IsEnabled = true;
                ChoosePrebuilt.IsEnabled = true;
                UsernameListBox.IsEnabled = true;
                SmartAdvanced.IsEnabled = false;
                IndivUser.IsEnabled = false;
                IndividualUser.IsChecked = false;
            }

        }




        private void UserEnumButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadSafeAppendLog("[1][*]Username enumeration beginning...");
            if (oneTimeUserEnum == 0)
            {
                ThreadSafeAppendLog("[2]Username Enumeration will add Domain information in the following order:");
                ThreadSafeAppendLog("[2]Domain given with username > Domain information gathered for specified service > Domain information gathered for any surface > Fail");
                oneTimeUserEnum = 1;
            }
            UserEnumPauseButton.IsEnabled = true;
            UserEnumStopButton.IsEnabled = true;

            //Set service based on chosen option
            lockUI(SendingWindow.UserEnum);
            MicrosoftService service = MicrosoftService.nullService;
            UsernameEnumerationType method = UsernameEnumerationType.nullMethod;
            if (UserEnumSurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }
            //Set method
            if (IndividualUser.IsChecked == true)
                method = UsernameEnumerationType.Individual;
            else if (UsernameListCheckbox.IsChecked == true)
                method = UsernameEnumerationType.UsernameList;
            else if (SmartEnumerationCheckbox.IsChecked == true)
                method = UsernameEnumerationType.SmartEnumeration;

            //PROBABLY ALSO NEED TESTING HERE - IF UNABLE TO GET ONE - WE DON'T SEEM TO HAVE ONE FOR THIS SURFACE
            //IE - WE SET SKYPE - BUT NOT PULLED REAL - SO CANT SET THIS HOST TO "REAL" SKYPE HOST
            //Want to say - if we already have a UsernameEnumeration for this service - use it - else - create one
            if (serviceInterfaces.Any(u => u.service == service))
            {
                //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                ThreadSafeAppendLog("[3]Selecting existing service interface...");
                ServiceInterface servInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                //So this should now be the only enumerator for that service - we can load and go
                servInterface.EnumerateUsers(method, UsernameListBox.Text, (bool)ChooseStartSmart.IsChecked, ChosenFormat.Text, ChosenStart.Text, PasswordBox.Text);
            }
            else
            {
                Hostnames enumeratorHostname = null;
                if (service == MicrosoftService.Skype)
                {
                    enumeratorHostname = (enumeratedHostnames.Where(x => x.Service == service && x.RealLync == true)).First();
                }
                else
                {
                    enumeratorHostname = (enumeratedHostnames.Where(x => x.Service == service)).First();
                }

                ThreadSafeAppendLog("[3]Adding new enumerator with service: " + service);
                ServiceInterface serInterface = new ServiceInterface() { host = enumeratorHostname, service = service, UI = this, enumerator = new UsernameEnumeration(), sprayer = new PasswordSpray(), postCompromise = new PostCompromise() { UI = this } };
                serviceInterfaces.Add(serInterface);
                serInterface.EnumerateUsers(method, UsernameListBox.Text, (bool)ChooseStartSmart.IsChecked, ChosenFormat.Text, ChosenStart.Text, PasswordBox.Text);
            }


        }






        private void UserEnumPauseButton_Click(object sender, RoutedEventArgs e)
        {
            MicrosoftService service = MicrosoftService.nullService;
            if (UserEnumSurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }
            if (UserEnumPauseButton.Content.ToString() == "Pause")
            {
                UserEnumPauseButton.Content = "Resume";
                ThreadSafeAppendLog("[1][*] Username enumeration paused at: " + DateTime.Now.ToShortTimeString());

                if (serviceInterfaces.Any(u => u.service == service))
                {
                    //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                    ThreadSafeAppendLog("[3]Selecting existing service interface...");
                    ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                    //So this should now be the only enumerator for that servie - we can load and go
                    serviceInterface.Pause(SendingWindow.UserEnum);
                }

            }
            else
            {
                UserEnumPauseButton.Content = "Pause";
                ThreadSafeAppendLog("[1][*] Username enumeration resumed at: " + DateTime.Now.ToShortTimeString());

                if (serviceInterfaces.Any(u => u.service == service))
                {
                    //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                    ThreadSafeAppendLog("[3]Selecting existing service interface...");
                    ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                    //So this should now be the only enumerator for that service - we can load and go
                    serviceInterface.Resume(SendingWindow.UserEnum);
                }
            }
        }

        private void UserEnumStopButton_Click(object sender, RoutedEventArgs e)
        {
            UserEnumPauseButton.Content = "Pause";

            MicrosoftService service = MicrosoftService.nullService;
            if (UserEnumSurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }

            if (serviceInterfaces.Any(u => u.service == service))
            {
                //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                ThreadSafeAppendLog("[3]Selecting existing service interface...");
                ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                //So this should now be the only enumerator for that servie - we can load and go
                serviceInterface.Resume(SendingWindow.UserEnum);
                serviceInterface.Stop(SendingWindow.UserEnum);
            }

        }






        private void IndividualUser_Click(object sender, RoutedEventArgs e)
        {
            if (IndividualUser.IsChecked == true)
            {
                IndivUser.IsEnabled = true;
                UsernameListCheckbox.IsChecked = false;
                SmartEnumerationCheckbox.IsChecked = false;
                ChooseUserList.IsEnabled = false;
                ChoosePrebuilt.IsEnabled = false;
                UsernameListBox.IsEnabled = false;
                SmartAdvanced.IsEnabled = false;
            }
            else
            {
                IndivUser.IsEnabled = false;
            }
        }

        private void ChooseCouncil_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseCouncil.IsChecked == true)
            {
                ChooseService.IsChecked = false;
                ChooseStandards.IsChecked = false;

                UsernameListBox.Text = "CouncilKillerv5.txt";
            }
            else
            {

            }
        }

        private void ChooseService_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseService.IsChecked == true)
            {
                ChooseCouncil.IsChecked = false;
                ChooseStandards.IsChecked = false;

                UsernameListBox.Text = "service-accounts.txt";
            }
            else
            {

            }
        }

        private void ChooseStandards_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseStandards.IsChecked == true)
            {
                ChooseService.IsChecked = false;
                ChooseCouncil.IsChecked = false;

                UsernameListBox.Text = "standard-accounts.txt";
            }
            else
            {

            }
        }

        private void ChooseUserList_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
                return;
            ChooseCouncil.IsChecked = false;
            ChooseService.IsChecked = false;
            ChooseStandards.IsChecked = false;
            UsernameListBox.Text = openFileDialog.FileName;
        }



        private void UseDiscoveredFormat_Click(object sender, RoutedEventArgs e)
        {
            if (UseDiscoveredFormat.IsChecked == true)
            {
                UseChosenFormat.IsChecked = false;
                UsernameListCheckboxSpray.IsChecked = false;
                EnumeratedUsersCheckboxSpray.IsChecked = false;

                UsernameListBoxSpray.IsEnabled = false;
                ChoosePrebuiltSpray.IsEnabled = false;
                ChooseUserListSpray.IsEnabled = false;

                ChosenFormatSpray.IsEnabled = false;

            }
        }

        private void UseChosenFormat_Click(object sender, RoutedEventArgs e)
        {
            if (UseChosenFormat.IsChecked == true)
            {
                ChosenFormatSpray.IsEnabled = true;

                UseDiscoveredFormat.IsChecked = false;
                UsernameListCheckboxSpray.IsChecked = false;
                EnumeratedUsersCheckboxSpray.IsChecked = false;

                UsernameListBoxSpray.IsEnabled = false;
                ChoosePrebuiltSpray.IsEnabled = false;
                ChooseUserListSpray.IsEnabled = false;


            }
        }

        private void UsernameListCheckboxSpray_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameListCheckboxSpray.IsChecked == true)
            {
                UseDiscoveredFormat.IsChecked = false;
                UseChosenFormat.IsChecked = false;
                EnumeratedUsersCheckboxSpray.IsChecked = false;

                UsernameListBoxSpray.IsEnabled = true;
                ChoosePrebuiltSpray.IsEnabled = true;
                ChooseUserListSpray.IsEnabled = true;

                ChosenFormatSpray.IsEnabled = false;
            }
        }

        private void EnumeratedUsersCheckboxSpray_Click(object sender, RoutedEventArgs e)
        {
            if (EnumeratedUsersCheckboxSpray.IsChecked == true)
            {
                UseDiscoveredFormat.IsChecked = false;
                UseChosenFormat.IsChecked = false;
                UsernameListCheckboxSpray.IsChecked = false;

                UsernameListBoxSpray.IsEnabled = false;
                ChoosePrebuiltSpray.IsEnabled = false;
                ChooseUserListSpray.IsEnabled = false;

                ChosenFormatSpray.IsEnabled = false;

            }
        }

        private void PasswordSprayGo_Click(object sender, RoutedEventArgs e)
        {
            ThreadSafeAppendLog("[1][*]Password spray beginning...");
            if (oneTimePassSpray == 0)
            {
                ThreadSafeAppendLog("[2]Password Spraying will add Domain information to given usernames in the following order:");
                ThreadSafeAppendLog("[2]Domain given with username > Domain information gathered for specified service > Domain information gathered for any surface > Fail");
                ThreadSafeAppendLog("[2]\"Legacy format\" usernames may contain numbers, or be linked to payroll ID (jsmith945 or PT32423432423423234) and therefore not be discoverable by Smart Enumeration, however, the modern format is more likely to match email style (jsmith@domain.com or john.smith@domain.com)...");
                oneTimePassSpray = 1;
            }

            PasswordSprayPause.IsEnabled = true;
            PasswordSprayStop.IsEnabled = true;

            //Set service based on chosen option
            MicrosoftService service = MicrosoftService.nullService;
            PasswordSprayType method = PasswordSprayType.nullMethod;
            if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }

            //Set method
            if (UseDiscoveredFormat.IsChecked == true)
                method = PasswordSprayType.UseDiscoveredFormat;
            else if (UseChosenFormat.IsChecked == true)
                method = PasswordSprayType.UseChosenFormat;
            else if (UsernameListCheckboxSpray.IsChecked == true && ChooseCouncilSpray.IsChecked == true)
                method = PasswordSprayType.UsernameListCouncil;
            else if (UsernameListCheckboxSpray.IsChecked == true && ChooseServiceSpray.IsChecked == true)
                method = PasswordSprayType.UsernameListService;
            else if (UsernameListCheckboxSpray.IsChecked == true && ChooseStandardsSpray.IsChecked == true)
                method = PasswordSprayType.UsernameListStandard;
            else if (UsernameListCheckboxSpray.IsChecked == true && ChooseStandardsSpray.IsChecked == false && ChooseServiceSpray.IsChecked == false && ChooseCouncilSpray.IsChecked == false)
                method = PasswordSprayType.UsernameListFile;
            else if (EnumeratedUsersCheckboxSpray.IsChecked == true)
                method = PasswordSprayType.EnumeratedUsers;

            //PROBABLY ALSO NEED TESTING HERE - IF UNABLE TO GET ONE - WE DON'T SEEM TO HAVE ONE FOR THIS SURFACE
            //IE - WE SET SKYPE - BUT NOT PULLED REAL - SO CANT SET THIS HOST TO "REAL" SKYPE HOST
            //Want to say - if we already have a UsernameEnumeration for this service - use it - else - create one
            if (serviceInterfaces.Any(p => p.service == service))
            {
                //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                ThreadSafeAppendLog("[3]Selecting existing service interface...");
                ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                //So we have a service interface for the service - now check if it has a sprayer:
                serviceInterface.SprayUsers(method, UsernameListBoxSpray.Text, ChosenFormatSpray.Text, PasswordFromSpray.Text, discoveredFormat);
            }
            else
            {
                Hostnames sprayerHostname = null;
                if (service == MicrosoftService.Skype)
                {
                    sprayerHostname = (enumeratedHostnames.Where(x => x.Service == service && x.RealLync == true)).First();
                }
                else
                {
                    sprayerHostname = (enumeratedHostnames.Where(x => x.Service == service)).First();
                }

                ThreadSafeAppendLog("[3]Adding new service interface with service: " + service);
                ServiceInterface serviceInterface = new ServiceInterface() { service = service, UI = this, host = sprayerHostname, enumerator = new UsernameEnumeration(), sprayer = new PasswordSpray(), postCompromise = new PostCompromise() { UI = this } };
                serviceInterfaces.Add(serviceInterface);
                serviceInterface.SprayUsers(method, UsernameListBoxSpray.Text, ChosenFormatSpray.Text, PasswordFromSpray.Text, discoveredFormat);
            }

        }


        public void dispatcherTimerUserEnum_Tick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentPositionNTLM.Text = "Current Position: " + getUsernamesDone() + "/" + getTotalUsernamesToTest();
            });
        }

        public void updateUserEnumText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentPositionNTLM.Text = text;
            });
        }

        public void updatePassSprayText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                oauthPosition.Text = text;
            });
        }



        public void dispatcherTimerPassSpray_Tick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                oauthPosition.Text = "Current Position: " + getUsernamesDone() + "/" + getTotalUsernamesToTest();
            });

        }

        private void PasswordSprayPause_Click(object sender, RoutedEventArgs e)
        {
            MicrosoftService service = MicrosoftService.nullService;
            if (UserEnumSurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (UserEnumSurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }
            if (PasswordSprayPause.Content.ToString() == "Pause")
            {
                PasswordSprayPause.Content = "Resume";
                ThreadSafeAppendLog("[1][*] Password spraying paused at: " + DateTime.Now.ToShortTimeString());


                if (serviceInterfaces.Any(u => u.service == service))
                {
                    //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                    ThreadSafeAppendLog("[3]Selecting existing service interface...");
                    ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                    //So this should now be the only enumerator for that servie - we can load and go
                    serviceInterface.Pause(SendingWindow.PasswordSpray);
                }

            }
            else
            {
                PasswordSprayPause.Content = "Pause";
                ThreadSafeAppendLog("[1][*] Password spraying resumed at: " + DateTime.Now.ToShortTimeString());

                if (serviceInterfaces.Any(u => u.service == service))
                {
                    //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                    ThreadSafeAppendLog("[3]Selecting existing service interface...");
                    ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                    //So this should now be the only enumerator for that servie - we can load and go
                    serviceInterface.Resume(SendingWindow.PasswordSpray);
                }
            }
        }

        private void PasswordSprayStop_Click(object sender, RoutedEventArgs e)
        {
            ThreadSafeAppendLog("[1][*] Password spraying stopped at...");
            saveValidUsersAndCreds(null, SaveType.autoLog);
            PasswordSprayPause.Content = "Pause";

            MicrosoftService service = MicrosoftService.nullService;
            if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "Skype")
            {
                service = MicrosoftService.Skype;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "Exchange")
            {
                service = MicrosoftService.Exchange;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "O365")
            {
                service = MicrosoftService.Office365;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "ADFS")
            {
                service = MicrosoftService.ADFS;
            }
            else if (PasswordSpraySurfacePicker.SelectedItem.ToString() == "RDWeb")
            {
                service = MicrosoftService.RDWeb;
            }

            if (serviceInterfaces.Any(u => u.service == service))
            {
                //THIS IS THEN ONE ALREADY SET UP - REAL LYNC ETC - SO JUST GO - WITH NEW USERNAME LIST
                ThreadSafeAppendLog("[3]Selecting existing service interface...");
                ServiceInterface serviceInterface = (serviceInterfaces.Where(x => x.service == service)).First();
                //So this should now be the only enumerator for that servie - we can load and go
                serviceInterface.Resume(SendingWindow.PasswordSpray);
                serviceInterface.Stop(SendingWindow.PasswordSpray);
            }
        }



        private void ChooseCouncilSpray_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseCouncilSpray.IsChecked == true)
            {
                UsernameListBoxSpray.Text = "CouncilKillerv5.txt";

                ChooseServiceSpray.IsChecked = false;
                ChooseStandardsSpray.IsChecked = false;
            }
        }

        private void ChooseServiceSpray_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseServiceSpray.IsChecked == true)
            {
                UsernameListBoxSpray.Text = "service-accounts.txt";

                ChooseCouncilSpray.IsChecked = false;
                ChooseStandardsSpray.IsChecked = false;
            }
        }

        private void ChooseStandardsSpray_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseStandardsSpray.IsChecked == true)
            {
                UsernameListBoxSpray.Text = "standard-accounts.txt";

                ChooseServiceSpray.IsChecked = false;
                ChooseCouncilSpray.IsChecked = false;
            }
        }

        private void ChooseUserListSpray_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
                return;
            ChooseCouncilSpray.IsChecked = false;
            ChooseServiceSpray.IsChecked = false;
            ChooseStandardsSpray.IsChecked = false;
            UsernameListBoxSpray.Text = openFileDialog.FileName;
        }


        private void ExpoCreds_Click(object sender, RoutedEventArgs e)
        {
            if (accessTokens.Count == 0)
            {
                ThreadSafeAppendLog("[1]Nothing to export..");
            }
            else
            {
                //Popup file dialogue to get location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "ValidCredentials.csv";
                saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Get the output file name from the text box.
                string fileName = saveFileDialog.FileName;

                if (fileName != null)
                    saveValidUsersAndCreds(fileName, SaveType.expo);
            }
        }


        private void ChooseDefault2_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseDefault2.IsChecked == true)
            {
                ChooseAll.IsChecked = false;
                ChooseDepartment.IsChecked = true;
                ChooseTitle.IsChecked = true;
                ChooseEmailAddress.IsChecked = true;
                ChoosePhoneNumber.IsChecked = true;
                ChooseOffice.IsChecked = false;
                ChoosePresence.IsChecked = false;
                ChooseNote.IsChecked = false;
            }
            else
            {
                ChooseDepartment.IsChecked = false;
                ChooseDepartment.IsChecked = false;
                ChooseEmailAddress.IsChecked = false;
                ChooseTitle.IsChecked = false;
                ChooseDepartment.IsChecked = false;
                ChoosePhoneNumber.IsChecked = false;
                ChooseNote.IsChecked = false;
                ChooseOffice.IsChecked = false;
                ChoosePresence.IsChecked = false;
            }
        }


        private void ChooseEmailAddress_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseEmailAddress.IsChecked == false)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChooseTitle_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseTitle.IsChecked == false)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChooseDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseDepartment.IsChecked == false)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChoosePhoneNumber_Click(object sender, RoutedEventArgs e)
        {
            if (ChoosePhoneNumber.IsChecked == false)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChooseNote_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseNote.IsChecked == true)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void PostExploitGo_Click(object sender, RoutedEventArgs e)
        {
            //Select Real Skype or return null if can't
            ServiceInterface postExploitServiceInterface = PostCredsHelper.getOrCreateServiceInterface(serviceInterfaces, this, enumeratedHostnames);
            if (postExploitServiceInterface != null)
            {
                string baseURL = postExploitServiceInterface.host.Hostname;
                lockUI(SendingWindow.AddressBook);
                lookupWorkerKillSwitch = 0;
                postCredsThreadCount = 0;

                if (ChooseDefault2.IsChecked == false && ChooseEmailAddress.IsChecked == false && ChooseTitle.IsChecked == false && ChoosePhoneNumber.IsChecked == false && ChooseDepartment.IsChecked == false && ChooseOffice.IsChecked == false && ChoosePresence.IsChecked == false && ChooseNote.IsChecked == false)
                {
                    ThreadSafeAppendLog("[1]Please choose at least one piece of information to retrieve...");
                }
                else
                {
                    //Get Options
                    bool check = false;
                    bool checkContacts = false;
                    bool checkFullAddressList = false;
                    bool chooseEmailAddress = false;
                    bool chooseDepartment = false;
                    bool choosePhoneNumber = false;
                    bool chooseTitle = false;
                    bool chooseNote = false;
                    bool chooseOffice = false;
                    bool choosePresence = false;
                    bool allChars = false;
                    bool commonChars = false;

                    CredentialsRecord record = null;

                    //Get Options and selected credential record
                    if (Credentials.Items.Count > 0)
                    {
                        record = (CredentialsRecord)Credentials.SelectedItem;
                        if (record != null)
                        {
                            check = true;
                        }
                        else
                        {
                            ThreadSafeAppendLog("[1]Please select a user...");
                            unlockUI();
                        }
                    }
                    if (All3Chars.IsChecked == true)
                    {
                        allChars = true;
                    }
                    if (Common3Chars.IsChecked == true)
                    {
                        commonChars = true;
                    }
                    if (ChoosePersonalContacts.IsChecked == true)
                    {
                        checkContacts = true;
                    }
                    if (ChooseAddressList.IsChecked == true)
                    {
                        checkFullAddressList = true;
                    }
                    if (ChooseEmailAddress.IsChecked == true)
                    {
                        chooseEmailAddress = true;
                    }
                    if (ChooseTitle.IsChecked == true)
                    {
                        chooseTitle = true;
                    }
                    if (ChooseDepartment.IsChecked == true)
                    {
                        chooseDepartment = true;
                    }
                    if (ChoosePhoneNumber.IsChecked == true)
                    {
                        choosePhoneNumber = true;
                    }
                    if (ChooseNote.IsChecked == true)
                    {
                        chooseNote = true;
                    }
                    if (ChooseOffice.IsChecked == true)
                    {
                        chooseOffice = true;
                    }
                    if (ChoosePresence.IsChecked == true)
                    {
                        choosePresence = true;
                    }

                    //GO
                    if (check)
                    {
                        postExploitServiceInterface.postCompromise.PullAddressBook(checkContacts, checkFullAddressList, chooseEmailAddress, chooseDepartment, choosePhoneNumber, chooseTitle, chooseNote, chooseOffice, choosePresence, allChars, commonChars, record, dataList, baseURL);
                    }
                }
            }
            else
            {
                //Say - no valid Real Skype host
                ThreadSafeAppendLog("[1]No valid real skype host...");
            }
        }







        private void ChooseOffice_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseOffice.IsChecked == true)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChoosePresence_Click(object sender, RoutedEventArgs e)
        {
            if (ChoosePresence.IsChecked == true)
            {
                ChooseDefault2.IsChecked = false;
            }
        }

        private void ChooseAll_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseAll.IsChecked == true)
            {
                ChooseAll.IsChecked = true;
                ChooseDepartment.IsChecked = true;
                ChooseTitle.IsChecked = true;
                ChooseEmailAddress.IsChecked = true;
                ChoosePhoneNumber.IsChecked = true;
                ChooseOffice.IsChecked = true;
                ChoosePresence.IsChecked = true;
                ChooseNote.IsChecked = true;
                ChooseDefault2.IsChecked = false;
            }
        }


        public System.Data.DataTable ConvertToDataTable<T>(IList<T> data)

        {

            PropertyDescriptorCollection properties =

            TypeDescriptor.GetProperties(typeof(T));

            System.Data.DataTable table = new System.Data.DataTable();

            foreach (PropertyDescriptor prop in properties)

                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)

            {

                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)

                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);

            }

            return table;

        }

        private void PostExploitExport_Click(object sender, RoutedEventArgs e)
        {
            if (dataList.Count == 0)
            {
                ThreadSafeAppendLog("[1]Nothing to export..");
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "_AddressBook.csv";
                saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Get the output file name from the text box.
                string fileName = saveFileDialog.FileName;

                if (fileName != null)
                    saveAddressBook(fileName, SaveType.expo);

            }
        }





        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Name
            if (SelectNameColumn.IsChecked == false)
            {
                NameColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                NameColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Sip
            if (SelectSipColumn.IsChecked == false)
            {
                SipColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                SipColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //Email
            if (SelectEmailColumn.IsChecked == false)
            {
                EmailColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmailColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //Title
            if (SelectTitleColumn.IsChecked == false)
            {
                TitleColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            //Department
            if (SelectDepartmentColumn.IsChecked == false)
            {
                DepartmentColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                DepartmentColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            //Office
            if (SelectOfficeColumn.IsChecked == false)
            {
                OfficeColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                OfficeColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            //Presence
            if (SelectPresenceColumn.IsChecked == false)
            {
                PresenceColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                PresenceColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            //Phone
            if (SelectPhoneColumn.IsChecked == false)
            {
                PhoneColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                PhoneColumn.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            //Note
            if (SelectNoteColumn.IsChecked == false)
            {
                NoteColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoteColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectUsernameColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectUsernameColumn.IsChecked == false)
            {
                UsernameColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                UsernameColumn.Visibility = Visibility.Visible;
            }
        }

        public void SetUserNumberAddressBook(string number)
        {
            lock (addressBookUserNumberLock)
            {
                UserNumber.Text = number;
            }
        }

        private void SelectPasswordColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPasswordColumn.IsChecked == false)
            {
                PasswordColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectSipEnabledColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectSipEnabledColumn.IsChecked == false)
            {
                SipEnabledColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                SipEnabledColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectAccountDisabledColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectAccountDisabledColumn.IsChecked == false)
            {
                AccountDisabledColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                AccountDisabledColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectPasswordExpiredColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPasswordExpiredColumn.IsChecked == false)
            {
                PasswordExpiredColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordExpiredColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectServerErrorColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectServerErrorColumn.IsChecked == false)
            {
                ServerErrorColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                ServerErrorColumn.Visibility = Visibility.Visible;
            }
        }

        private void SelectAccessTokenColumn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectAccessTokenColumn.IsChecked == false)
            {
                AccessTokenColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                AccessTokenColumn.Visibility = Visibility.Visible;
            }
        }

        private void PostExploitCancel_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<ServiceInterface> allSkype = serviceInterfaces.Where(x => x.service == MicrosoftService.Skype);
            foreach (ServiceInterface serv in allSkype)
            {
                if (serv.host.RealLync == true)
                {
                    serv.postCompromise.Stop(this);
                }
            }
        }





        private void O365ExpoCreds_Click(object sender, RoutedEventArgs e)
        {
            if (O365CredsRecords.Count == 0)
            {
                ThreadSafeAppendLog("[1]Nothing to export..");
            }
            else
            {
                try
                {
                    //Popup file dialogue to get location
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = "ValidO365Credentials.csv";
                    saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
                    if (saveFileDialog.ShowDialog() != true)
                        return;

                    // Get the output file name from the text box.
                    string fileName = saveFileDialog.FileName;

                    StringBuilder credentialsCSV = new StringBuilder();
                    credentialsCSV.Append("Email Address, Password, MFA" + Environment.NewLine);
                    foreach (O365CredentialsRecord record in O365CredsRecords)
                    {
                        credentialsCSV.Append(record.Email + "," + record.Password + "," + record.MFA + Environment.NewLine);
                    }


                    System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
                    file.WriteLine(credentialsCSV.ToString());
                    ThreadSafeAppendLog("[1]Credentials records written to: " + fileName);
                    file.Close();

                }
                catch (IOException ioExcept)
                {
                    if (ioExcept.ToString().Contains("another process"))
                    {
                        ThreadSafeAppendLog("[1]The file is currently open and cannot be saved...");
                    }
                }
                catch (Exception exctsfdt)
                {
                    ThreadSafeAppendLog("[1]" + exctsfdt.ToString());
                }
            }
        }

        private void UserEnumSurfacePicker_DropDownClosed(object sender, EventArgs e)
        {
            if (UserEnumSurfacePicker.SelectedItem.ToString() == "O365")
            {
                PasswordBox.IsEnabled = false;
            }
            else
            {
                PasswordBox.IsEnabled = true;
                //Disable O365 options if not O365?
            }
        }





        private void Common3Chars_Click(object sender, RoutedEventArgs e)
        {
            if (Common3Chars.IsChecked == true)
            {
                All3Chars.IsChecked = false;
            }
        }

        private void All3Chars_Click(object sender, RoutedEventArgs e)
        {
            if (All3Chars.IsChecked == true)
            {
                Common3Chars.IsChecked = false;
            }
        }

        private void PasswordSpraySurfacePicker_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void MeetingSnoop_Click(object sender, RoutedEventArgs e)
        {
            ServiceInterface postExploitServiceInterface = PostCredsHelper.getOrCreateServiceInterface(serviceInterfaces, this, enumeratedHostnames);
            if (postExploitServiceInterface != null)
            {
                lockUI(SendingWindow.MeetingSnooper);
                string baseURL = postExploitServiceInterface.host.Hostname;
                IList<CredentialsRecord> snooperRecords = new List<CredentialsRecord>();

                //Get Options
                if (MeetingSnooperAllUsers.IsChecked == true)
                {
                    foreach (CredentialsRecord rec in accessTokens)
                    {
                        if (rec.Password != null && rec.Password != "")
                        {
                            snooperRecords.Add(rec);
                        }
                    }
                    ThreadSafeAppendLog("[2]All users count: " + snooperRecords.Count);
                }
                else if (MeetingSnooperSelectedUser.IsChecked == true)
                {
                    if (Credentials.SelectedItems.Count > 0)
                    {
                        try
                        {
                            System.Collections.IList test = Credentials.SelectedItems;
                            foreach (var test2 in test)
                            {
                                snooperRecords.Add((CredentialsRecord)test2);
                            }
                            ThreadSafeAppendLog("[2]Selected user count: " + snooperRecords.Count);
                        }
                        catch (Exception sdf)
                        {

                        }
                    }
                }
                else
                {
                    ThreadSafeAppendLog("[1]Please choose either all users or selected users...");
                }

                if (snooperRecords.Count > 0)
                {
                    postExploitServiceInterface.postCompromise.SnoopMeetings(snooperRecords, this, meetingsRecords, baseURL);
                }
                else
                {
                    ThreadSafeAppendLog("[1]No compromised users to snoop...");
                    unlockUI();
                }

            }
            else
            {
                ThreadSafeAppendLog("[1]No valid skype server...");
                unlockUI();
            }

        }

        private void MeetingSnooperAllUsers_Checked(object sender, RoutedEventArgs e)
        {
            if (MeetingSnooperAllUsers.IsChecked == true)
            {
                MeetingSnooperSelectedUser.IsChecked = false;
            }
        }

        private void MeetingSnooperSelectedUser_Checked(object sender, RoutedEventArgs e)
        {
            if (MeetingSnooperSelectedUser.IsChecked == true)
            {
                MeetingSnooperAllUsers.IsChecked = false;
            }
        }

        private void ExpoMeetings_Click(object sender, RoutedEventArgs e)
        {
            if (meetingsRecords.Count == 0)
            {
                ThreadSafeAppendLog("[1]Nothing to export..");
            }
            else
            {
                //Popup file dialogue to get location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "_ValidMeetings.csv";
                saveFileDialog.Filter = "Csv file (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Get the output file name from the text box.
                string fileName = saveFileDialog.FileName;

                if (fileName != null)
                    saveValidMeetings(fileName, SaveType.expo);
            }
        }

    }

    public class O365CredentialsRecord
    {
        private string password;
        private string email;
        private string mfa;

        public string Password { get => password; set => password = value; }
        public string Email { get => email; set => email = value; }
        public string MFA { get => mfa; set => mfa = value; }
    }


    public class DataRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string SipUsername;
        private string emailAddress;
        private string name;
        private string tite;
        private string dept;
        private string phneNumber;
        private string nte;
        private string office;
        private string presence;

        public string Office
        {
            get { return office; }
            set
            {
                office = value;
                OnPropertyChanged("Office");
            }
        }

        public string Presence
        {
            get { return presence; }
            set
            {
                presence = value;
                OnPropertyChanged("Presence");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string SIPUsername
        {
            get { return SipUsername; }
            set
            {
                SipUsername = value;
                OnPropertyChanged("SIPUsername");
            }
        }

        public string EmailAddress
        {
            get { return emailAddress; }
            set
            {
                emailAddress = value;
                OnPropertyChanged("EmailAddress");
            }
        }

        public string Title
        {
            get { return tite; }
            set
            {
                tite = value;
                OnPropertyChanged("Title");
            }
        }

        public string Department
        {
            get { return dept; }
            set
            {
                dept = value;
                OnPropertyChanged("Department");
            }
        }

        public string Note
        {
            get { return nte; }
            set
            {
                nte = value;
                OnPropertyChanged("Note");
            }
        }


        public string PhoneNumber
        {
            get { return phneNumber; }
            set
            {
                phneNumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }


    public class MessagingObject
    {

        private string operatorId;
        private string username;
        private string startMessagingLink;
        private string eventsLink;
        private string name;
        private string sipUsername;
        private string etag;

        public string Etag
        {
            get { return etag; }
            set
            {
                etag = value;
            }
        }

        public string SIPUsername
        {
            get { return sipUsername; }
            set
            {
                sipUsername = value;
            }
        }
        public string EventsLink
        {
            get { return eventsLink; }
            set
            {
                eventsLink = value;
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        public string OperatorID
        {
            get { return operatorId; }
            set
            {
                operatorId = value;
            }
        }
        public string StartMessagingLink
        {
            get { return startMessagingLink; }
            set
            {
                startMessagingLink = value;
            }
        }
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
            }
        }

    }

    public class SubdomainRecord
    {
        private string service;
        private string subdomain;
        private string ntlmAuthURL;
        private string OAuthURL1;

        public string Service { get => service; set => service = value; }
        public string Subdomain { get => subdomain; set => subdomain = value; }
        public string NtlmAuthURL { get => ntlmAuthURL; set => ntlmAuthURL = value; }
        public string OAuthURL { get => OAuthURL1; set => OAuthURL1 = value; }
    }


}

