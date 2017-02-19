using Newtonsoft.Json;
using Skype4Sharp.Auth;
using System.IO;
using System;
using Skype4Sharp;

namespace LunchNotifier.Targets
{
    class SkypeNotifier : ILunchNotifyTarget
    {
        private SkypeCredentials _credentials;
        private string _chatLink;

        public SkypeNotifier(string jsonInitFilePath)
        {
            string rawJson = File.ReadAllText(jsonInitFilePath);
            dynamic decodedJSON = JsonConvert.DeserializeObject(rawJson);

            _credentials = new SkypeCredentials(decodedJSON.user.ToString(), decodedJSON.password.ToString());
            _chatLink = decodedJSON.chatLink;
        }

        public void BroadcastLunchInfo(LunchInfo info)
        {
            Skype4Sharp.Skype4Sharp mainSkype = new Skype4Sharp.Skype4Sharp(_credentials);
            mainSkype.tokenType = Skype4Sharp.Enums.SkypeTokenType.OAuth;
            mainSkype.Login();
            
            Chat targetChat = new Chat(mainSkype);
            targetChat.ChatLink = _chatLink;
            mainSkype.SendMessage(targetChat, info.MenuMessage);
        }
    }
}
