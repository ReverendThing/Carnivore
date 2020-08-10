using System;
using System.Collections.ObjectModel;

namespace sLYNCy_WPF
{
    class AddDataRecord
    {
        public static void AddOrUpdateDataRecords(DataRecord record, ObservableCollection<DataRecord> dataList, MainWindow UI)
        {
            int recordChecker = 0;
            foreach (DataRecord recordCheck in dataList)
            {
                if (recordCheck.Name == record.Name)
                {
                    if (recordCheck.Note == "" || recordCheck.Note == null)
                    {
                        recordCheck.Note = record.Note;
                    }
                    if (recordCheck.PhoneNumber == "" || recordCheck.PhoneNumber == null)
                    {
                        recordCheck.PhoneNumber = record.PhoneNumber;
                    }
                    if (recordCheck.SIPUsername == "" || recordCheck.SIPUsername == null)
                    {
                        recordCheck.SIPUsername = record.SIPUsername;
                    }
                    if (recordCheck.Department == "" || recordCheck.Department == null)
                    {
                        recordCheck.Department = record.Department;
                    }
                    if (recordCheck.EmailAddress == "" || recordCheck.EmailAddress == null)
                    {
                        recordCheck.EmailAddress = record.EmailAddress;
                    }
                    if (recordCheck.Title == "" || recordCheck.Title == null)
                    {
                        recordCheck.Title = record.Title;
                    }
                    if (recordCheck.Office == "" || recordCheck.Office == null)
                    {
                        recordCheck.Office = record.Office;
                    }

                    //ALWAYS UPDATE PRESENCE - as this is now the latest - even if unknown
                    recordCheck.Presence = record.Presence;
                    recordChecker = 1;
                    UI.saveAddressBook(null, Enums.SaveType.autoLog);
                }
            }
            if (recordChecker == 0)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    dataList.Add(record);
                    UI.SetUserNumberAddressBook(dataList.Count.ToString());
                });
                UI.saveAddressBook(null, Enums.SaveType.autoLog);
            }
        }
    }
}
