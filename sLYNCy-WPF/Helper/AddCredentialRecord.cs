using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    class AddCredentialRecord
    {
        public static void Add(CredentialsRecord record, ObservableCollection<CredentialsRecord> accessTokens, MainWindow UI, MicrosoftService service)
        {
            try
            {
                //Will these count as same object? Might have matching properties - but created in two separate places - might need to match on values
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    //Unlock EnumerateUsers for PassSpray - as this is just definitely adding a user
                    MainWindow.SetDoWeHaveEnumeratedUsers(true);
                    if (record.Password != "" && record.Password != null)
                        MainWindow.SetDoWeHaveAnyUserAndPass(true);
                    //If record already exists with same username - grab that record and update as necessary
                    if (accessTokens.Any(p => p.Username == record.Username))
                    {
                        int changed = 0;
                        //SHOULD ONLY BE ONE RECORD WITH MATCHING USERNAME
                        IEnumerable<CredentialsRecord> alreadyExists = accessTokens.Where(x => x.Username == record.Username);
                        CredentialsRecord updateMe = alreadyExists.First();
                        //If the record we are trying to add has a password - get the existing record with matching username
                        //These are just updating all if record has it - then saving - not actually checking that it doesn't match what's already in
                        if (record.Password != null && record.Password != "")
                        {
                            //JUST UPDATE PASSWORD - EITHER WILL BE SAME OR WE'VE FOUND IT CHANGED NOW
                            updateMe.Password = record.Password;
                            changed++;
                        }
                        if (record.MFA != null && record.MFA != "")
                        {
                            updateMe.MFA = record.MFA;
                            changed++;
                        }
                        if (record.PasswordExpired != null && record.PasswordExpired != "")
                        {
                            updateMe.PasswordExpired = record.PasswordExpired;
                            changed++;
                        }
                        if (record.ServerError != null && record.ServerError != "")
                        {
                            updateMe.ServerError = record.ServerError;
                            changed++;
                        }
                        if (record.AccountDisabled != null && record.AccountDisabled != "")
                        {
                            updateMe.AccountDisabled = record.AccountDisabled;
                            changed++;
                        }
                        if (record.SipEnabled != null && record.SipEnabled != "")
                        {
                            updateMe.SipEnabled = record.SipEnabled;
                            changed++;
                        }
                        //UPDATE RECORD TO BE SERVICE WE LAST HIT - IF WE ENUMMED IN EXCHANGE - THEN SPRAYED IN LYNC AND GOT PASSWORD
                        //IS NOW LYNC
                        if (updateMe.Service != record.Service)
                        {
                            updateMe.Service = record.Service;
                            changed++;
                        }

                        //I don't fully know why this checks for record.password as well? Might have had a reason? Though also - no harm? Can't think how
                        //I'd get a new token with no password?
                        if (record.Token != null && record.Password != "")
                        {
                            updateMe.Token = record.Token;
                            changed++;
                        }

                        if (changed > 0)
                        {
                            UI.saveValidUsersAndCreds(null, SaveType.autoLog);
                        }

                    }
                    else
                    {
                        accessTokens.Add(record);
                        UI.saveValidUsersAndCreds(null, SaveType.autoLog);
                    }
                });
            }
            catch (Exception e)
            {

            }
        }
    }
}
