using System;
using System.Collections.Generic;

namespace AgoraGames.Hydra
{
	public class Commands
	{
		protected List<HydraCommand> commands = new List<HydraCommand>();
		
		public Commands () {
		}
		
		public void SetValue(string key, object val) 
        {
			commands.Add(new HydraCommand(HydraCommand.Command.SET, key, val));
		}
	
		public void IncValue(string key, object val) 
        {
			commands.Add(new HydraCommand(HydraCommand.Command.INC, key, val));
		}

        public void RemoveValue(string key)
        {
            commands.Add(new HydraCommand(HydraCommand.Command.DEL, key));
        }

        public void PushValue(string key, object val)
        {
            commands.Add(new HydraCommand(HydraCommand.Command.PUSH, key, val));
        }

        public void PopValue(string key)
        {
            commands.Add(new HydraCommand(HydraCommand.Command.POP, key));
        }

        public object ConvertToRequest()
        {
			List<object> ret = new List<object>();
			
			foreach(HydraCommand c in commands) {
				List<object> commandList = new List<object>();
				
				commandList.Add (c.getCommand());
				commandList.Add (c.Key);
				commandList.Add (c.Value);
				ret.Add(commandList);
			}
			return ret;
		}
		
		public class HydraCommand
		{
			public enum Command {
				SET,
				INC,
                DEL,
                PUSH,
                POP
            };
			
			Command command;
			public string Key;
			public object Value;
			
			public HydraCommand(Command command, string key) {
				this.command = command;
				this.Key = key;
			}
			
			public HydraCommand(Command command, string key, object val) {
				this.command = command;
				this.Key = key;
				this.Value = val;
			}
			
			public string getCommand() {
				switch(command) {
				case Command.SET:
					return "set";
				case Command.INC:
					return "inc";
                case Command.DEL:
                    return "del";
                case Command.PUSH:
                    return "push";
                case Command.POP:
                    return "pop";
                }
				return "";
			}
		}
	}
}