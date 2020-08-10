using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    class AddService
    {
        public static void AddServiceToOptions(MainWindow UI, MicrosoftService service, bool userEnum, bool passSpray)
        {
            UI.Dispatcher.Invoke(() =>
            {
                //Add any service - enable enum/pass spray tabs
                if (userEnum)
                    UI.LyncEnabled = 1;
                if (passSpray)
                    UI.PasswordSprayEnabled = 1;

                switch (service)
                {

                    case MicrosoftService.Skype:
                        if (UI.UserEnumSurfacePicker.Items.Contains("Skype"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("Skype");
                                UI.UserEnumSurfacePicker.SelectedItem = "Skype";
                            }

                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("Skype"))
                        {

                        }
                        else
                        {
                            if (passSpray)
                            {
                                //Add and select on all forms
                                UI.PasswordSpraySurfacePicker.Items.Add("Skype");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "Skype";
                            }
                        }
                        break;
                    case MicrosoftService.Office365:
                        if (UI.UserEnumSurfacePicker.Items.Contains("O365"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("O365");
                                UI.UserEnumSurfacePicker.SelectedItem = "O365";
                            }
                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("O365"))
                        {

                        }
                        else
                        {
                            //Select last added
                            if (passSpray)
                            {
                                UI.PasswordSpraySurfacePicker.Items.Add("O365");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "O365";
                            }
                        }
                        if (passSpray == false)
                        {
                            UI.ThreadSafeAppendLog("[1]The organisation O365 was found to be federated, you can enumerate users, but password spraying must hit the organisation's ADFS server, not the O365 portal...");
                            UI.ThreadSafeAppendLog("[1]O365 has therefore not been added to the Password Spray tab available surfaces...");
                        }
                        break;
                    case MicrosoftService.RDWeb:
                        if (UI.UserEnumSurfacePicker.Items.Contains("RDWeb"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("RDWeb");
                                UI.UserEnumSurfacePicker.SelectedItem = "RDWeb";
                            }
                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("RDWeb"))
                        {

                        }
                        else
                        {
                            //Select last added
                            if (passSpray)
                            {
                                UI.PasswordSpraySurfacePicker.Items.Add("RDWeb");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "RDWeb";
                            }
                        }
                        break;
                    case MicrosoftService.Exchange:
                        if (UI.UserEnumSurfacePicker.Items.Contains("Exchange"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("Exchange");
                                UI.UserEnumSurfacePicker.SelectedItem = "Exchange";
                            }
                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("Exchange"))
                        {

                        }
                        else
                        {
                            //Select last added
                            if (passSpray)
                            {
                                UI.PasswordSpraySurfacePicker.Items.Add("Exchange");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "Exchange";
                            }
                        }
                        break;
                    case MicrosoftService.ADFS:
                        if (UI.UserEnumSurfacePicker.Items.Contains("ADFS"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("ADFS");
                                UI.UserEnumSurfacePicker.SelectedItem = "ADFS";
                            }
                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("ADFS"))
                        {

                        }
                        else
                        {
                            //Select last added
                            if (passSpray)
                            {
                                UI.PasswordSpraySurfacePicker.Items.Add("ADFS");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "ADFS";
                            }
                        }
                        break;
                    case MicrosoftService.Exchange2007:
                        if (UI.UserEnumSurfacePicker.Items.Contains("Exchange2007"))
                        {

                        }
                        else
                        {
                            //Add and select on all forms
                            if (userEnum)
                            {
                                UI.UserEnumSurfacePicker.Items.Add("Exchange2007");
                                UI.UserEnumSurfacePicker.SelectedItem = "Exchange2007";
                            }
                        }
                        if (UI.PasswordSpraySurfacePicker.Items.Contains("Exchange2007"))
                        {

                        }
                        else
                        {
                            //Select last added
                            if (passSpray)
                            {
                                UI.PasswordSpraySurfacePicker.Items.Add("Exchange2007");
                                UI.PasswordSpraySurfacePicker.SelectedItem = "Exchange2007";
                            }
                        }
                        break;

                }

            });
        }

    }
}
