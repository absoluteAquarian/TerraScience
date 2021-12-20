using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Content.Commands{
	public class TimeSetter : ModCommand{
		public override CommandType Type => CommandType.Chat;
 
		public override string Command => "timeset";
 
		public override string Usage => "[c/ffca00:Usage: /timeset <time string> [<AM/PM>][c/ffca00:]]";
 
		public override string Description => "Sets the time.  <time string> is a string, such as \"4:30\" (without the quotes) or \"dawn\".";
 
		public static readonly string[] Keywords = new string[]{
			"dawn",			//6:00 AM
			"midnight",		//12:00 AM
			"night",		//7:29 PM
			"morning",		//9:30 AM
			"day",			//4:29 AM
			"dusk",			//6:00 PM
			"noon",			//12:00 PM
			"afternoon"		//2:30 PM
		};
 
		public static readonly int[] KeyTimes = new int[]{
			_12PM - ToTicks(6),      //dawn
			_12AM,                   //midnight
			_7_30PM_day - 1,         //night
			_12PM - ToTicks(2, 30),  //morning
			_4_30AM_night - 1,       //day
			_12PM + ToTicks(6),      //dusk
			_12PM,                   //noon
			_12PM + ToTicks(2, 30)   //afternoon
		};
 
		public const int _4_30 = 4 * 3600 + 30 * 60;
		public const int _7_30 = 7 * 3600 + 30 * 60;
		public const int _12_00 = 12 * 3600;
		public const int _7_30PM_day = 54000;
		public const int _4_30AM_night = 32400;
		public const int _12AM = _4_30AM_night - _4_30;  //16,200
		public const int _12PM = _7_30PM_day - _7_30;    //27,000
 
		public override void Action(CommandCaller caller, string input, string[] args){
			bool am = false;
			int colonIndex, hour, minutes, tickTime = -1;
			bool usedKeyword = false;
 
			//Only allow time editing if the mod isn't a Release version
			bool b = TechMod.Release;
			if(b){
				caller.Reply("Editing time is disabled.", Color.Red);
				return;
			}
 
			//Check if the player has input one of the time keywords
			if(args.Length == 1 && Keywords.Contains(args[0])){
				int index;
				for(index = 0; index < Keywords.Length; index++)
					if(Keywords[index] == args[0])
						break;
 
				tickTime = KeyTimes[index];
				Main.dayTime = new[]{ "dawn", "morning", "noon", "afternoon", "dusk", "night" }.Contains(args[0]);
				usedKeyword = true;
			}else if(args.Length < 2){
				caller.Reply("Parameter list was too small.", Color.Red);
				caller.Reply(Usage);
				return;
			}
 
			//Check if the time format (AM/PM) is correct
			if(tickTime < 0 && !(args[1] == "PM" || args[1] == "AM")){
				caller.Reply("Time format was invalid.", Color.Red);
				caller.Reply(Usage);
				return;
			}
 
			//Check if the time value is valid (only digits and ":")
			if(tickTime < 0 && !IsValidTime(args[0])){
				caller.Reply("Time format was invalid.", Color.Red);
				caller.Reply(Usage);
				return;
			}
 
			//The time is valid.  Get the tick count and update the time accordingly
 
			if(tickTime < 0){
				am = args[1] == "AM";
				colonIndex = args[0].IndexOf(':');
				hour = int.Parse(args[0].Substring(0, colonIndex));
				minutes = int.Parse(args[0].Substring(colonIndex + 1, 2));
				tickTime = ToTicks(hour, minutes);
			}
 
			if(usedKeyword)
				goto afterTime;
 
			if(am){
				if(tickTime < _4_30){
					am = false;
					tickTime += _12AM;
				}else if(tickTime == _4_30)
					tickTime = 0;
				else if(tickTime == _12_00){
					am = false;
					tickTime = _12AM;
				}else
					tickTime -= _12AM;
			}else{
				if(tickTime < _7_30){
					am = true;
					tickTime += _12PM;
				}else if(tickTime == _7_30)
					tickTime = 0;
				else if(tickTime == _12_00){
					am = true;
					tickTime = _12PM;
				}else
					tickTime -= _12PM;
			}
 
			Main.dayTime = am;
afterTime:			
			//Set the time
			Main.time = tickTime;
 
			if(!usedKeyword)
				caller.Reply($"Time was updated to {args[0]} {args[1]}!", Color.Orange);
			else
				caller.Reply($"Time was updated to \"{args[0]}\"!", Color.Orange);
		}
 
		private bool IsValidTime(string time){
			int colonIndex = time.IndexOf(':');
			if(!time.Contains(":") || time.Length < 4 || time.Length > 5 || time.All(c => !char.IsDigit(c) && c != ':') || time.Length - colonIndex != 3)
				return false;
			return true;
		}
 
		private static int ToTicks(int hours = 0, int minutes = 0, int seconds = 0)
			=> hours * 3600 + minutes * 60 + seconds;
	}
}
