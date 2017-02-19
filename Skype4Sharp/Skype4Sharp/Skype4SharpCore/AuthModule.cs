using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Skype4Sharp.Skype4SharpCore
{
    class AuthModule
    {
        private Skype4Sharp parentSkype;
        public AuthModule(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public bool Login()
        {
            parentSkype.authState = Enums.LoginState.Processing;
            try
            {
                parentSkype.authTokens.SkypeToken = getSkypeToken();
                setRegTokenAndEndpoint();
                startSubscription();
                setProfile();
                parentSkype.authState = Enums.LoginState.Success;
                return true;
            }
            catch
            {
                parentSkype.authState = Enums.LoginState.Failed;
                return false;
            }
        }
        private string getSkypeToken()
        {
            switch (parentSkype.tokenType)
            {
                case Enums.SkypeTokenType.Standard:
                    HttpWebRequest standardTokenRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
                    string uploadData = "";
                    using (HttpWebResponse webResponse = (HttpWebResponse)standardTokenRequest.GetResponse())
                    {
                        string rawDownload = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                        uploadData = string.Format("username={0}&password={1}&timezone_field={2}&js_time={3}&pie={4}&etm={5}&client_id=578134&redirect_uri={6}", parentSkype.authInfo.Username.UrlEncode(), parentSkype.authInfo.Password.UrlEncode(), DateTime.Now.ToString("zzz").Replace(":", "|").UrlEncode(), (Helpers.Misc.getTime() / 1000).ToString(), new Regex("<input type=\"hidden\" name=\"pie\" id=\"pie\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), new Regex("<input type=\"hidden\" name=\"etm\" id=\"etm\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString().UrlEncode(), "https://web.skype.com".UrlEncode());
                    }
                    HttpWebRequest actualLogin = parentSkype.mainFactory.createWebRequest_POST("https://login.skype.com/login?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { }, Encoding.ASCII.GetBytes(uploadData), "");
                    using (HttpWebResponse webResponse = (HttpWebResponse)actualLogin.GetResponse())
                    {
                        return new Regex("type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"").Match(new StreamReader(webResponse.GetResponseStream()).ReadToEnd()).Groups[1].ToString();
                    }
                case Enums.SkypeTokenType.MSNP24:
                    HttpWebRequest MSNP24TokenRequest = parentSkype.mainFactory.createWebRequest_POST("https://api.skype.com/login/skypetoken", new string[][] { }, Encoding.ASCII.GetBytes(string.Format("scopes=client&clientVersion=0/7.17.0.105//&username={0}&passwordHash={1}", parentSkype.authInfo.Username, Convert.ToBase64String(Helpers.Misc.hashMD5_Byte(string.Format("{0}\nskyper\n{1}", parentSkype.authInfo.Username, parentSkype.authInfo.Password))))), "");
                    string rawJSON = "";
                    using (HttpWebResponse webResponse = (HttpWebResponse)MSNP24TokenRequest.GetResponse())
                    {
                        rawJSON = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                    }
                    dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
                    return decodedJSON != null ? decodedJSON.skypetoken : null;
                case Enums.SkypeTokenType.OAuth:
                    // Step 1
                    HttpWebRequest oauthTokenRequest = parentSkype.mainFactory.createWebRequest_GET("https://login.skype.com/login/oauth/microsoft?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com", new string[][] { });
                    string ppft = "";
                    string mspReq = "";
                    string mspok = "";
                    using (HttpWebResponse webResponse = (HttpWebResponse)oauthTokenRequest.GetResponse())
                    {
                        string rawDownload = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                        ppft = new Regex("<input type=\"hidden\" name=\"PPFT\" id=\"i0327\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString();
                        string cookie = webResponse.Headers.Get("Set-Cookie");
                        mspReq = new Regex("MSPRequ=(.*?);").Match(cookie).Groups[1].ToString();
                        mspok = new Regex("MSPOK=(.*?);").Match(cookie).Groups[1].ToString();
                    }
                    // Step 2
                    HttpWebRequest tokenVerifyRequest = parentSkype.mainFactory.createWebRequest_POST("https://login.live.com/ppsecure/post.srf?wa=wsignin1.0&wp=MBI_SSL&wreply=https%3A%2F%2Flw.skype.com%2Flogin%2Foauth%2Fproxy%3Fclient_id%3D578134%26site_name%3Dlw.skype.com%26redirect_uri%3Dhttps%253A%252F%252Fweb.skype.com%252F",
                        new string[][] { new string[] { "Cookie", "MSPRequ=" + mspReq }, new string[] { "Cookie", "MSPOK=" + mspok }, new string[] { "Cookie", "CkTst=G" + Helpers.Misc.getTime().ToString() } },
                        Encoding.ASCII.GetBytes(string.Format("login={0}&passwd={1}&PPFT={2}", parentSkype.authInfo.Username.UrlEncode(), parentSkype.authInfo.Password.UrlEncode(), ppft.UrlEncode())),
                        "application/x-www-form-urlencoded");
                    string tParam = "";
                    using (HttpWebResponse webResponse = (HttpWebResponse)tokenVerifyRequest.GetResponse())
                    {
                        string rawDownload = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                        tParam = new Regex("<input type=\"hidden\" name=\"t\" id=\"t\" value=\"(.*?)\">").Match(rawDownload).Groups[1].ToString();
                    }
                    // Step 3
                    HttpWebRequest loginRequest = parentSkype.mainFactory.createWebRequest_POST("https://login.skype.com/login/microsoft?client_id=578134&redirect_uri=https%3A%2F%2Fweb.skype.com",
                        new string[][] { },
                        Encoding.ASCII.GetBytes(string.Format("client_id=578134&redirect_uri=https%3A%2F%2web.skype.com&oauthPartner=999&site_name=lw.skype.com&t={0}", tParam)),
                        "application/x-www-form-urlencoded");
                    using (HttpWebResponse webResponse = (HttpWebResponse)loginRequest.GetResponse())
                    {
                        string rawDownload = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                        return new Regex("<input type=\"hidden\" name=\"skypetoken\" value=\"(.*?)\"/>").Match(rawDownload).Groups[1].ToString();
                    }
                default:
                    return null;
            }
        }
        private void setRegTokenAndEndpoint()
        {
            HttpWebRequest webRequest = parentSkype.mainFactory.createWebRequest_POST("https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints", new string[][] { new string[] { "Authentication", "skypetoken=" + parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{}"), "application/x-www-form-urlencoded");
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                parentSkype.authTokens.RegistrationToken = webResponse.GetResponseHeader("Set-RegistrationToken").Split(';')[0];
                parentSkype.authTokens.Endpoint = webResponse.GetResponseHeader("Location");
                parentSkype.authTokens.EndpointID = webResponse.GetResponseHeader("Set-RegistrationToken").Split(';')[2].Split('=')[1];
            }
        }
        private void startSubscription()
        {
            HttpWebRequest propertiesRequest = parentSkype.mainFactory.createWebRequest_PUT("https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints/SELF/properties?name=supportsMessageProperties", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"supportsMessageProperties\":true}"), "application/json");
            using (HttpWebResponse webResponse = (HttpWebResponse)propertiesRequest.GetResponse()) { }
            HttpWebRequest subscriptionRequest = parentSkype.mainFactory.createWebRequest_POST("https://client-s.gateway.messenger.live.com/v1/users/ME/endpoints/SELF/subscriptions", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } }, Encoding.ASCII.GetBytes("{\"channelType\":\"httpLongPoll\",\"template\":\"raw\",\"interestedResources\":[\"/v1/users/ME/conversations/ALL/properties\",\"/v1/users/ME/conversations/ALL/messages\",\"/v1/users/ME/contacts/ALL\",\"/v1/threads/ALL\"]}"), "application/json");
            using (HttpWebResponse webResponse = (HttpWebResponse)subscriptionRequest.GetResponse()) { }
        }
        private void setProfile()
        {
            HttpWebRequest selfProfileRequest = parentSkype.mainFactory.createWebRequest_GET("https://api.skype.com/users/self/profile", new string[][] { new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } });
            string rawJSON = "";
            using (HttpWebResponse webResponse = (HttpWebResponse)selfProfileRequest.GetResponse())
            {
                rawJSON = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJSON);
            string firstName = decodedJSON.firstname;
            string lastName = decodedJSON.lastname;
            string userName = decodedJSON.username;
            string finalName = "";
            if (firstName == null)
            {
                if (lastName != null)
                {
                    finalName = lastName;
                }
            }
            else
            {
                if (lastName == null)
                {
                    finalName = firstName;
                }
                else
                {
                    finalName = firstName + " " + lastName;
                }
            }
            parentSkype.selfProfile.DisplayName = finalName;
            parentSkype.selfProfile.Username = userName;
            parentSkype.selfProfile.Type = Enums.UserType.Normal;
        }
    }
}
