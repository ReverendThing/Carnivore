using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sLYNCy_WPF
{
    public class DNSLookup
    {
        public bool Skype = false;
        public bool Exchange = false;
        public bool ADFS = false;
        public bool RDWeb = false;
        public bool O365 = false;
        private List<string> subdomains = new List<string>();
        //Add what it needs based on what's ticked then fire off singular DNSLookup Worker Thread
        //Maybe based on lookup objects - so can tell when all Exchange etc done

        //Then Fire up Validation Thread
        public void DNSEnumerate()
        {
            //Do Task Async thing for each list based on which ones are ticked
            //Would be nice to still say finished for each one?
            if (Skype == true)
            {

            }
            if (Exchange == true)
            {

            }
            if (ADFS == true)
            {

            }
            if (RDWeb == true)
            {

            }
            if (O365 == true)
            {

            }

            //Thread based on what's in list
        }
    }
}
