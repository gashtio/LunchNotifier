using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System;

namespace Skype4Sharp.Skype4SharpCore
{
    class MessageModule
    {
        private Skype4Sharp parentSkype;
        public MessageModule(Skype4Sharp skypeToUse)
        {
            parentSkype = skypeToUse;
        }
        public void editMessage(ChatMessage messageInfo, string newMessage)
        {
            HttpWebRequest webRequest = parentSkype.mainFactory.createWebRequest_POST(messageInfo.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + newMessage.JsonEscape() + "\",\"messagetype\":\"" + ((messageInfo.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"skypeeditedid\":\"" + messageInfo.ID + "\"}"), "application/json");
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) { }
        }
        public ChatMessage createMessage(Chat targetChat, string chatMessage, Enums.MessageType messageType)
        {
            ChatMessage toReturn = new ChatMessage(parentSkype);
            toReturn.Body = chatMessage;
            toReturn.Chat = targetChat;
            toReturn.Type = messageType;
            toReturn.ID = Helpers.Misc.getTime().ToString();
            toReturn.Sender = parentSkype.selfProfile;
            sendChatmessage(toReturn);
            return toReturn;
        }
        public Chat[] getRecentChats()
        {
            HttpWebRequest webRequest = parentSkype.mainFactory.createWebRequest_GET("https://client-s.gateway.messenger.live.com/v1/users/ME/conversations?startTime=1486073711834&pageSize=100&view=msnp24Equivalent&targetType=Passport|Skype|Lync|Thread|PSTN", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken } });
            string rawInfo = "";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                rawInfo = new System.IO.StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            List<Chat> toReturn = new List<Chat>();
            dynamic jsonObject = JsonConvert.DeserializeObject(rawInfo);
            foreach (dynamic singleConversation in jsonObject.conversations)
            {
                toReturn.Add(chatFromConversation(singleConversation));
            }
            return toReturn.ToArray();
        }

        private Chat chatFromConversation(dynamic singleConversation)
        {
            Chat result = new Chat(parentSkype);
            result.ID = singleConversation.id;
            result.ChatLink = singleConversation.targetLink;
            if (result.ID.StartsWith("8:"))
            {
                result.Type = Enums.ChatType.Private;
            }
            else if (result.ID.StartsWith("19:"))
            {
                result.Type = Enums.ChatType.Group;
            }
            return result;
        }

        private void sendChatmessage(ChatMessage messageToSend)
        {
            HttpWebRequest webRequest = parentSkype.mainFactory.createWebRequest_POST(messageToSend.Chat.ChatLink + "/messages", new string[][] { new string[] { "RegistrationToken", parentSkype.authTokens.RegistrationToken }, new string[] { "X-Skypetoken", parentSkype.authTokens.SkypeToken } }, Encoding.ASCII.GetBytes("{\"content\":\"" + messageToSend.Body.JsonEscape() + "\",\"messagetype\":\"" + ((messageToSend.Type == Enums.MessageType.RichText) ? "RichText" : "Text") + "\",\"contenttype\":\"text\",\"clientmessageid\":\"" + messageToSend.ID + "\"}"), "application/json");
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) { }
        }
    }
}
