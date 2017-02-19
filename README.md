# LunchNotifier

Going to lunch is a very complex task that requires massive brainpower to choose where you would actually eat.

It's becoming increasingly annoying to go to a restaurant's website, sift through the pages and find the daily menu, which is where LunchNotifier comes to help. It does all that automatically and then notifies whoever is interested!

# License 
See: https://github.com/gashtio/LunchNotifier/blob/master/LICENSE.md

# Dependencies
- [Json.NET] (http://www.newtonsoft.com/json)
- [HtmlAgilityPack] (https://www.nuget.org/packages/HtmlAgilityPack)
- [Skype4Sharp] (https://github.com/lin-e/Skype4Sharp)

# Overview

The program consists of lunch providers (`ILunchProvider`) and lunch notify targets (`ILunchNotifyTarget`).
These are registered in the beginning of the program and then every provider supplies every target with information that it should broadcast.

At the moment, there is one provider and one target (Skype). The communication between the provider and target is done through the `LunchInfo` structure, which is simply a `string` message for the moment, but it could possibly be extended to have an attachment or something else you might find useful.

# Usage

The default implementation consists of 1 [provider](https://www.krivoto.com) and 1 notify target (Skype).
The provider works without modifications, but for Skype to work you need to supply some information.

By default, the Skype notidfier tries to read from the `cred.json` file from the current working directory a JSON-formatted file, which contains the *username*, *password* and *chat link* which are going to be used. The username and password are reqiured for logging in, and the chat link determines which chat to paste the message into. A sample file would look like this:

```{JSON}
{  
	"user":"myskype@live.com",
	"password":"superstrongpassword",
	"chatLink":"https://db5-client-s.gateway.messenger.live.com/v1/threads/19:a480242bd5d54642aa0b52f21b7acab9@thread.skype"
}
```

## Chat link
You can find the chat link using the provided Skype API. A small program that gets a list for all recent chats would be something like this:

```{C#}
Skype4Sharp.Skype4Sharp mainSkype = new Skype4Sharp.Skype4Sharp(new SkypeCredentials(user, pass));
mainSkype.tokenType = Skype4Sharp.Enums.SkypeTokenType.OAuth;
mainSkype.Login();
foreach (Chat chat in mainSkype.GetRecentChats())
{
	Console.WriteLine(string.Format("Chat \"{0}\": {1}",
		chat.Type == Skype4Sharp.Enums.ChatType.Group ? chat.Topic : chat.Participants[1].DisplayName,
		chat.ChatLink));
}
```

You'll then need to go through the list and find the chat you want to dump the information in.

## Scheduling a task

This program is intended to be used as a daily reminder for your lunch options, so it's best to have 1 person set it up as a scheduled task (e.g. in Windows Task Scheduler) that gives you the meal options for today.

