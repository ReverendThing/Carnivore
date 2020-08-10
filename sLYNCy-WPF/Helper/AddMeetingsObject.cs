using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    class AddMeetingsObject
    {
        public static void Add(MeetingsObject record, ObservableCollection<MeetingsObject> meetingsRecords, MainWindow UI)
        {

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (meetingsRecords.Any(p => p.UserRecord.Username == record.UserRecord.Username && p.ConfID == record.ConfID))
                {
                    //So if we already have a record for this user with that conf ID - Get it - update anything else needed?
                    IEnumerable<MeetingsObject> alreadyExists = meetingsRecords.Where(x => x.UserRecord.Username == record.UserRecord.Username && x.ConfID == record.ConfID);
                    MeetingsObject updateMe = alreadyExists.First();
                    if (updateMe.Subject != record.Subject)
                        updateMe.Subject = record.Subject;
                    if (updateMe.JoinURL != record.JoinURL)
                        updateMe.JoinURL = record.JoinURL;
                    if (updateMe.Attendees != record.Attendees)
                        updateMe.Attendees = record.Attendees;
                    if (updateMe.ExpirationTime != record.ExpirationTime)
                        updateMe.ExpirationTime = record.ExpirationTime;
                    if (updateMe.LobbyBypass != record.LobbyBypass)
                        updateMe.LobbyBypass = record.LobbyBypass;
                    UI.saveValidMeetings(null, SaveType.autoLog);
                } else
                {
                    //New meeting
                    meetingsRecords.Add(record);
                    UI.saveValidMeetings(null, SaveType.autoLog);
                }
            });
        }
        }
}
