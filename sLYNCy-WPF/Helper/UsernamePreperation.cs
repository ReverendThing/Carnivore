using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static sLYNCy_WPF.Enums;
using static sLYNCy_WPF.Utilities;

namespace sLYNCy_WPF
{
    public class FormatObject
    {
        List<string> usernames;
        UsernameFormat format;
        int count;

        public List<string> Usernames { get => usernames; set => usernames = value; }
        public UsernameFormat Format { get => format; set => format = value; }
        public int Count { get => count; set => count = value; }
    }

    class UsernamePreperation
    {

        public static List<string> PrepareUsernames(List<string> usernames, Hostnames host, MainWindow UI, SendingWindow sender, PasswordSprayType passSprayOption)
        {
            if (usernames == null)
            {
                UI.ThreadSafeAppendLog("[2]No usernames to prepare...");
                return null;
            }
            else
            {
                Hostnames ntlmHost = null;
                Hostnames oAuthHost = null;
                List<string> preparedUsernames = new List<string>();




                //Start by checking if contains @ or \ - if it does, strip to just the user, and see if we have the username (regardless of @ or \ already)
                foreach (string line in usernames)
                {
                    //IF THE INDIVIDUAL LINE HAS @ OR \ THEN USER HAS SUPPLIED THIS WITH USERNAME
                    if (line.Contains("@") || line.Contains("\\"))
                    {
                        if (line.Contains("@"))
                        {
                            //If user supplied username contains @ or \ we need to split to just username and then see if it's already
                            //In - equating to what we do with our own usernames.
                            if (sender == SendingWindow.PasswordSpray)
                            {
                                if (DoWeAdd(StripUser(line), UI, sender))
                                {
                                    preparedUsernames.Add(line);
                                }
                            }
                            else
                            if (sender == SendingWindow.UserEnum && host.Service != MicrosoftService.Office365)
                            {
                                UI.ThreadSafeAppendLog("[2]User cannot be added. This endpoint REQUIRES legacy format, and username contains @: " + line);
                            }
                            else
                            {
                                //For user enum - only add if USERNAME doesn't exist already - for pass spray - only add if USERNAME AND PASSWORD don't exist already
                                if (DoWeAdd(StripUser(line), UI, sender))
                                {
                                    preparedUsernames.Add(line);
                                }
                            }
                        }
                        else if (line.Contains("\\"))
                        {
                            if (host.Service == MicrosoftService.Office365)
                            {
                                UI.ThreadSafeAppendLog("[2]User cannot be added. O365 Enumeration REQUIRES email style format, and username contains \\: " + line);
                            }
                            else
                            {
                                //For user enum - only add if USERNAME doesn't exist already - for pass spray - only add if USERNAME AND PASSWORD don't exist already
                                if (DoWeAdd(StripUser(line), UI, sender))
                                {
                                    preparedUsernames.Add(line);
                                }

                            }
                        }

                    }
                    else if (false)
                    {
                        //NEED TO ADD THIS - MANUALLY ENTERED DOMAIN INFORMATION
                    }
                    else if (sender == SendingWindow.UserEnum)
                    {
                        //IF enumerating on-prem, HAS TO BE LEGACY
                        if (host.Service == MicrosoftService.Skype || host.Service == MicrosoftService.Exchange || host.Service == MicrosoftService.ADFS || host.Service == MicrosoftService.RDWeb)
                        {
                            //TRUE = ONLY LEGACY
                            AddLegacy(line, host, ref preparedUsernames, UI, sender, ref ntlmHost, ref oAuthHost, true);
                        }
                        else
                        {
                            //TRUE = ONLY MODERN
                            //If O365 - add "email style" - HAS TO BE EMAIL STYLE
                            AddModern(line, host, ref preparedUsernames, UI, sender, ref ntlmHost, ref oAuthHost, true);
                        }
                    }
                    else if (sender == SendingWindow.PasswordSpray)
                    {
                        if (passSprayOption == PasswordSprayType.UseDiscoveredFormat)
                        {
                            //If we are adding users with the discovered format - this was discovered in legacy format
                            //so TRUE - Add in ONLY legacy format
                            AddLegacy(line, host, ref preparedUsernames, UI, sender, ref ntlmHost, ref oAuthHost, true);
                        }
                        else
                        {
                            //FALSE = TRY MODERN FIRST, THEN DO LEGACY IF NOT
                            AddModern(line, host, ref preparedUsernames, UI, sender, ref ntlmHost, ref oAuthHost, false);
                        }
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]Unable to add username...");
                    }

                }

                return preparedUsernames;
            }

        }

        public static void AddModern(string line, Hostnames host, ref List<string> preparedUsernames, MainWindow UI, SendingWindow sender, ref Hostnames ntlmHost, ref Hostnames oAuthHost, bool onlyModern)
        {
            //Add this host's oauth domain if possible
            if (host.OAuthDomain != null)
            {
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(line + "@" + host.OAuthDomain);
                }

            }
            else if (host.NTLMDomain != null && onlyModern == false)
            {
                //Add this hosts legacy format if not
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(host.NTLMDomain + "\\" + line);
                }
            }
            else if (oAuthHost != null)
            {
                //If we have stored oauth from any other service from before add that
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(line + "@" + oAuthHost.OAuthDomain);
                }
            }
            else if (ntlmHost != null && onlyModern == false)
            {
                //Same for NTLM if not
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(ntlmHost.NTLMDomain + "\\" + line);
                }
            }
            //Else if any service has an oauth format - use and store
            else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
            {
                IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                if (hostWithOAuthDomain.Count() > 0)
                {
                    if (DoWeAdd(line, UI, sender))
                    {
                        preparedUsernames.Add(line + "@" + hostWithOAuthDomain.First().OAuthDomain);
                    }
                }
                oAuthHost = hostWithOAuthDomain.First();
            }
            //Else if any host has NTLM - use and store
            else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null) && onlyModern == false)
            {
                IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                if (hostWithNTLMDomain.Count() > 0)
                {
                    if (DoWeAdd(line, UI, sender))
                    {
                        preparedUsernames.Add(hostWithNTLMDomain.First().NTLMDomain + "\\" + line);
                    }
                }
                ntlmHost = hostWithNTLMDomain.First();
            }
            else
            {
                UI.ThreadSafeAppendLog("[2]Unable to add username...");
            }
        }

        public static void AddLegacy(string line, Hostnames host, ref List<string> preparedUsernames, MainWindow UI, SendingWindow sender, ref Hostnames ntlmHost, ref Hostnames oAuthHost, bool onlyLegacy)
        {
            //UPDATED - this is for services that REQUIRE legacy - so ONLY add legacy - or not at all

            //Add Legacy format FROM THIS SERVICE first if we can
            if (host.NTLMDomain != null)
            {
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(host.NTLMDomain + "\\" + line);
                }
            }
            else if (host.OAuthDomain != null && onlyLegacy == false)
            {
                //Then Modern style FROM THIS SERVIC Eif we have it
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(line + "@" + host.OAuthDomain);
                }
            }
            else if (ntlmHost != null)
            {
                //Do we have a stored NTLM host from any other service?
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(ntlmHost.NTLMDomain + "\\" + line);
                }
            }
            else if (oAuthHost != null && onlyLegacy == false)
            {
                //Do we have a stored OAuth host from any other service?
                if (DoWeAdd(line, UI, sender))
                {
                    preparedUsernames.Add(line + "@" + oAuthHost.OAuthDomain);
                }
            }
            else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
            {
                //IF ANY HOST HAS NTLM DOMAIN USE THAT and store
                IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                if (hostWithNTLMDomain.Count() > 0)
                {
                    if (DoWeAdd(line, UI, sender))
                    {
                        preparedUsernames.Add(hostWithNTLMDomain.First().NTLMDomain + "\\" + line);
                    }
                }
                ntlmHost = hostWithNTLMDomain.First();
            }
            else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null) && onlyLegacy == false)
            {
                //IF ANY HOST HAS OAUTH DOMAIN USE THAT and store
                IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                if (hostWithOAuthDomain.Count() > 0)
                {
                    if (DoWeAdd(line, UI, sender))
                    {
                        preparedUsernames.Add(line + "@" + hostWithOAuthDomain.First().OAuthDomain);
                    }
                }
                oAuthHost = hostWithOAuthDomain.First();
            }
            else
            {
                UI.ThreadSafeAppendLog("[2]Unable to add username...");
            }
        }

        private static bool DoWeAdd(string user, MainWindow UI, SendingWindow sender)
        {
            switch (sender)
            {
                case SendingWindow.PasswordSpray:
                    if (UI.accessTokens.Any(p => p.UserRaw == user && p.Password != null && p.Password != "") || UI.accessTokens.Any(p => p.UserRaw == user && p.AccountDisabled == "Y"))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case SendingWindow.UserEnum:
                    if (UI.accessTokens.Any(p => p.UserRaw == user))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
            return false;
        }

        public static string PrepareUsername(string username, Hostnames host, MainWindow UI, SendingWindow sender)
        {
            if (username.Contains("@") || username.Contains("\\"))
            {
                if (DoWeAdd(StripUser(username), UI, sender))
                {
                    return username;
                }
                else
                {
                    return null;
                }
            }
            else if (false)
            {
                //NEED TO ADD THIS - MANUALLY ENTERED DOMAIN INFORMATION
            }
            else if (sender == SendingWindow.UserEnum)
            {
                if (host.Service == MicrosoftService.Skype || host.Service == MicrosoftService.Exchange || host.Service == MicrosoftService.ADFS || host.Service == MicrosoftService.RDWeb)
                {
                    if (host.NTLMDomain != null)
                    {
                        //NOW GO LEGACY FORMAT
                        if (DoWeAdd(username, UI, sender))
                        {
                            return host.NTLMDomain + "\\" + username;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (host.OAuthDomain != null)
                    {
                        if (DoWeAdd(username, UI, sender))
                        {
                            return username + "@" + host.OAuthDomain;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                    {
                        //IF ANY HOST HAS NTLM DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                        if (hostWithNTLMDomain.Count() > 0)
                        {
                            if (DoWeAdd(username, UI, sender))
                            {

                                return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                    {
                        //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                        if (hostWithOAuthDomain.Count() > 0)
                        {
                            if (DoWeAdd(username, UI, sender))
                            {
                                return username + "@" + hostWithOAuthDomain.First().OAuthDomain;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]Unable to add username...");
                        return "";
                    }
                }
                else
                {
                    //FOR OTHERS - CAN FIGURE WHICH IS WHICH LATER - DO OAUTH FORMAT THEN LEGACY
                    if (host.OAuthDomain != null)
                    {
                        //ALWAYS DEFAULT TO OAUTH IF POSSIBLE - MORE LIKELY TO CONTAIN EMAIL STYLE FORMAT
                        if (DoWeAdd(username, UI, sender))
                        {
                            return username + "@" + host.OAuthDomain;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else if (host.NTLMDomain != null)
                    {
                        //NOW GO LEGACY FORMAT IF WE HAVE TO
                        if (DoWeAdd(username, UI, sender))
                        {
                            return host.NTLMDomain + "\\" + username;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                    {
                        //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                        if (hostWithOAuthDomain.Count() > 0)
                        {
                            if (DoWeAdd(username, UI, sender))
                            {
                                return username + "@" + hostWithOAuthDomain.First().OAuthDomain;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                    {
                        //IF ANY HOST HAS NTLM DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                        if (hostWithNTLMDomain.Count() > 0)
                        {
                            if (DoWeAdd(username, UI, sender))
                            {
                                return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]Unable to add username...");
                        return "";
                    }
                }
            }
            else if (sender == SendingWindow.PasswordSpray)
            {
                //Do OAuth then Legacy ALWAYS
                if (host.OAuthDomain != null)
                {
                    //ALWAYS DEFAULT TO OAUTH IF POSSIBLE - MORE LIKELY TO CONTAIN EMAIL STYLE FORMAT
                    if (DoWeAdd(username, UI, sender))
                    {
                        return username + "@" + host.OAuthDomain;
                    }
                    else
                    {
                        return null;
                    }

                }
                else if (host.NTLMDomain != null)
                {
                    //NOW GO LEGACY FORMAT IF WE HAVE TO
                    if (DoWeAdd(username, UI, sender))
                    {
                        return host.NTLMDomain + "\\" + username;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                {
                    //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                    IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                    if (hostWithOAuthDomain.Count() > 0)
                    {
                        if (DoWeAdd(username, UI, sender))
                        {
                            return username + "@" + hostWithOAuthDomain.First().OAuthDomain;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                {
                    //IF ANY HOST HAS NTLM DOMAIN USE THAT
                    IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                    if (hostWithNTLMDomain.Count() > 0)
                    {
                        if (DoWeAdd(username, UI, sender))
                        {
                            return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    UI.ThreadSafeAppendLog("[2]Unable to add username...");
                    return "";
                }
            }
            else
            {
                UI.ThreadSafeAppendLog("[2]Unable to add username...");
                return "";
            }

            return "";
        }

        public static string PrepareUsernameTimings(string username, Hostnames host, MainWindow UI, SendingWindow sender)
        {
            //FOR THE TIMINGS - WE DON'T CHECK IF ALREADY GUESSED - COS WE NEED THESE - NOONES GONNA HAVE GIBBERLYJONGI
            //AND KRBTGT CAN'T BE LOCKED OUT - IS IN COUNCIL KILLER - SO STILL NEED TO GET TIMINGS!!
            if (username.Contains("@") || username.Contains("\\"))
            {
                return username;
            }
            else if (false)
            {
                //NEED TO ADD THIS - MANUALLY ENTERED DOMAIN INFORMATION
            }
            else if (sender == SendingWindow.UserEnum)
            {
                if (host.Service == MicrosoftService.Skype || host.Service == MicrosoftService.Exchange || host.Service == MicrosoftService.ADFS || host.Service == MicrosoftService.RDWeb)
                {
                    //IF WE ARE DOING USER ENUM FOR SKYPE - MUST BE LEGACY FORMAT - SO ADD LEGACY FIRST (/probs should be only)
                    if (host.NTLMDomain != null)
                    {
                        return host.NTLMDomain + "\\" + username;
                    }
                    else if (host.OAuthDomain != null)
                    {
                        return username + "@" + host.OAuthDomain;
                    }
                    else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                    {
                        //IF ANY HOST HAS NTLM DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                        if (hostWithNTLMDomain.Count() > 0)
                        {

                            return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;

                        }
                    }
                    else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                    {
                        //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                        if (hostWithOAuthDomain.Count() > 0)
                        {

                            return username + "@" + hostWithOAuthDomain.First().OAuthDomain;

                        }
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]Unable to add username...");
                        return "";
                    }
                }
                else
                {
                    //FOR OTHERS - CAN FIGURE WHICH IS WHICH LATER - DO OAUTH FORMAT THEN LEGACY
                    if (host.OAuthDomain != null)
                    {

                        return username + "@" + host.OAuthDomain;


                    }
                    else if (host.NTLMDomain != null)
                    {

                        return host.NTLMDomain + "\\" + username;

                    }
                    else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                    {
                        //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                        if (hostWithOAuthDomain.Count() > 0)
                        {

                            return username + "@" + hostWithOAuthDomain.First().OAuthDomain;

                        }
                    }
                    else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                    {
                        //IF ANY HOST HAS NTLM DOMAIN USE THAT
                        IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                        if (hostWithNTLMDomain.Count() > 0)
                        {

                            return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;

                        }
                    }
                    else
                    {
                        UI.ThreadSafeAppendLog("[2]Unable to add username...");
                        return "";
                    }
                }
            }
            else if (sender == SendingWindow.PasswordSpray)
            {
                //Do OAuth then Legacy ALWAYS
                if (host.OAuthDomain != null)
                {

                    return username + "@" + host.OAuthDomain;


                }
                else if (host.NTLMDomain != null)
                {
                    //NOW GO LEGACY FORMAT IF WE HAVE TO

                    return host.NTLMDomain + "\\" + username;

                }
                else if (UI.enumeratedHostnames.Any(q => q.OAuthDomain != null))
                {
                    //IF ANY HOST HAS OAUTH DOMAIN USE THAT
                    IEnumerable<Hostnames> hostWithOAuthDomain = UI.enumeratedHostnames.Where(x => x.OAuthDomain != null);
                    if (hostWithOAuthDomain.Count() > 0)
                    {

                        return username + "@" + hostWithOAuthDomain.First().OAuthDomain;

                    }
                }
                else if (UI.enumeratedHostnames.Any(p => p.NTLMDomain != null))
                {
                    //IF ANY HOST HAS NTLM DOMAIN USE THAT
                    IEnumerable<Hostnames> hostWithNTLMDomain = UI.enumeratedHostnames.Where(x => x.NTLMDomain != null);
                    if (hostWithNTLMDomain.Count() > 0)
                    {

                        return hostWithNTLMDomain.First().NTLMDomain + "\\" + username;

                    }
                }
                else
                {
                    UI.ThreadSafeAppendLog("[2]Unable to add username...");
                    return "";
                }
            }
            else
            {
                UI.ThreadSafeAppendLog("[2]Unable to add username...");
                return "";
            }

            return "";



        }

        public static List<string> LoadUsernames(UsernameFileFormat format, string path, MainWindow UI, string startingName)
        {
            //So this just loads in raw - no checking on each line - then goes to prepare usernames which gives them in correct format
            List<string> usernamesToReturn = new List<string>();
            //So either it is inbuilt or from a file - take the path and load and then return list of strings
            switch (format)
            {
                case UsernameFileFormat.File:
                    try
                    {
                        var lines = File.ReadLines(path);
                        foreach (var line in lines)
                        {
                            usernamesToReturn.Add(line);
                        }
                        return usernamesToReturn;
                    }
                    catch (FileNotFoundException fe)
                    {
                        UI.ThreadSafeAppendLog("[2]File not found...");
                        return null;
                    }
                    catch (Exception e)
                    {
                        UI.ThreadSafeAppendLog("[2]File exception: " + e.ToString());
                        return null;
                    }
                case UsernameFileFormat.InBuilt:
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

                                    if (startingName == null)
                                    {
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            usernamesToReturn.Add(line);
                                        }
                                    }
                                    else
                                    {
                                        int hitStartingPoint = 0;
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            if (hitStartingPoint == 0)
                                            {
                                                if (line == startingName)
                                                {
                                                    hitStartingPoint = 1;
                                                    usernamesToReturn.Add(line);
                                                }
                                            }
                                            else
                                            {
                                                usernamesToReturn.Add(line);
                                            }

                                        }
                                    }


                                }
                                return usernamesToReturn;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        UI.ThreadSafeAppendLog("[2]File exception: " + e.ToString());
                        return null;
                    }
                case UsernameFileFormat.Enumerated:
                    foreach (CredentialsRecord token in UI.accessTokens)
                    {
                        //TAKE EACH USERNAME FROM ACCESS TOKENS RECORDS - PREPARE USERNAMES THEN CHECKS FOR USER + PASS SO DON'T NEED TO WORRY HERE
                        //ADD AS NEVTEK\CSCOTT OR WHATEVER - AS WE KNOW FOR SURE THAT USER IN THAT FORMAT EXISTS
                        usernamesToReturn.Add(token.Username);
                    }
                    return usernamesToReturn;
            }
            //SHOULD NEVER HIT HERE
            return null;


        }
    }
}
