using System;
using System.IO;
using System.Collections.Generic;

using GameServer;
using GameServer.addon;
using GameServer.events;
using GameServer.player;

namespace Owners
{
	public class Addon : IModule, IEventListener
	{
		public const int VERSION = 1;
		
		public string SOURCEFILE;
		
		List<string> OwnersList = new List<string>();
		
		public string GetMetadata()
		{
			return "Owners v." + VERSION;
		}

		public string GetDescription()
		{
			return "Commands to chat by owners!";
		}
		
		public void OnLoaded()
		{
			SOURCEFILE = Addons.GetDirectory() + "owners.txt";
			
			if(!File.Exists(SOURCEFILE)) File.WriteAllText(SOURCEFILE, "");
			else OwnersList.AddRange(File.ReadAllLines(SOURCEFILE));
			
			ConsoleReader.HelpCommandLines.Add("addowner <nick> / removeowner <nick> - Owners");
			
			Events.AddListener(this);
		}

		public void OnDisabled()
		{
			Events.RemoveListener(this);
		}

		public void Handler(Event he)
		{
			if(he.GetCode() == Events.Code_PlayerActionEvent)
			{
				PlayerActionEvent e = (PlayerActionEvent) he;
				
				if(e.Action == PlayerActionEvent.Actions.Chat)
				{
					string message = (string) e.Data[0];
					
					if(message[0] == '/')
					{
						if(IsOwner(e.Player.Name))
						{
							e.Player.SendChatMessage(
								ConsoleReader.HandleCommand(message.Substring(1).Split(' '), e.Player.Name)
							);
							
							e.Cancelled = true;
						}
					}
				}
				
				if(e.Action == PlayerActionEvent.Actions.Born)
				{
					if(IsOwner(e.Player.Name)) 
					{
						Server.Log("Player {0} owner is!", e.Player.Name);
					}
				}
			}
			
			if(he.GetCode() == Events.Code_ConsoleCommandEvent)
			{
				ConsoleCommandEvent e = (ConsoleCommandEvent) he;
				
				if(e.Command.Length > 1)
				{
					if(e.Command[0] == "addowner" && !IsOwner(e.Command[1])) 
					{
						AddOwner(e.Command[1]);
						
						e.Metadata = "New owner " + e.Command[1];
						
						Server.BroadcastMessage(e.Metadata);
						
						e.Cancelled = true;
					}
					
					if(e.Command[0] == "removeowner" && !IsOwner(e.Command[1])) 
					{
						RemoveOwner(e.Command[1]);
						
						e.Metadata = "Deleted owner " + e.Command[1];
						
						Server.BroadcastMessage(e.Metadata);
						
						e.Cancelled = true;
					}
				}
			}
		}
		
		public bool IsOwner(string playerName)
		{
			foreach(string owner in OwnersList)
			{
				if(owner == playerName.ToLower()) return true;
			}
			
			return false;
		}
		
		public void AddOwner(string playerName, bool autoSave = true)
		{
			OwnersList.Add(playerName.ToLower());
			
			if(autoSave) Save();
		}
		
		public void RemoveOwner(string playerName, bool autoSave = true)
		{
			OwnersList.Remove(playerName.ToLower());
			
			if(autoSave) Save();
		}
		
		void Save()
		{
			File.WriteAllLines(SOURCEFILE, OwnersList.ToArray());
		}
	}
}