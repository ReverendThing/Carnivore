using System.Text.RegularExpressions;
using static sLYNCy_WPF.Enums;

namespace sLYNCy_WPF
{
    public static class RegexClass
    {
        public static Match ReturnMatch(Regexs regexToMatch, string stringToMatch)
        {
            switch (regexToMatch)
            {
                case Regexs.SkypeRealAddress:
                    Regex regex = new Regex(@"(?<=https://).*?([^/]*)");
                    Match match = regex.Match(stringToMatch);
                    return match;
                case Regexs.NTLMResponse:
                    Regex regex2 = new Regex(@"(?<=NTLM ).*?(?=\r|,)");
                    Match match2 = regex2.Match(stringToMatch);
                    return match2;
                case Regexs.NTLMResponseADFS:
                    Regex regex3 = new Regex(@"(?<=Negotiate ).*?(?=\r|,)");
                    Match match3 = regex3.Match(stringToMatch);
                    return match3;
                case Regexs.ADFSAuthURL:
                    Regex regex4 = new Regex(@"(?<=https://).*?([^/]*)");
                    Match match4 = regex4.Match(stringToMatch);
                    return match4;
                case Regexs.cwtToken:
                    Regex regex5 = new Regex(@"cwt=.*?(?="")");
                    Match match5 = regex5.Match(stringToMatch);
                    return match5;
                case Regexs.applicationsURL:
                    Regex regex6 = new Regex(@"(?<=\""applications\"":{\""href\"":\"")(.*?"")");
                    Match match6 = regex6.Match(stringToMatch);
                    return match6;
                case Regexs.sipUsername:
                    Regex regex7 = new Regex(@"(?<=sip:).*?([^/]*)");
                    Match match7 = regex7.Match(stringToMatch);
                    return match7;
            }
            return null;
        }
    }
}
