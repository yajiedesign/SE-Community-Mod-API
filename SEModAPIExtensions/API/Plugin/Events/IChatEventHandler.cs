﻿namespace SEModAPIExtensions.API.Plugin.Events
{
	public interface IChatEventHandler
	{
		void OnChatReceived(ChatManager.ChatEvent chatEvent);
		void OnChatSent(ChatManager.ChatEvent chatEvent);
	}
}
