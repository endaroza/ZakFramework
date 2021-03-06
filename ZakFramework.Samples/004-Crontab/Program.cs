﻿using System;
using System.Collections.Generic;
using System.IO;
using ZakCore.Utils.Commons;

namespace _004_Crontab
{
	/// <summary>
	/// Crontab program
	/// 
	/// Format string with spaces between the entries
	///  *     *     *    *     *     *    * command to be executed
	///  -     -     -    -     -     -    -
	///  |     |     |    |     |     |    +----- year
	///  |     |     |    |     |     +----- day of week (0 - 6) (Sunday=0)
	///  |     |     |    |     +------- month (1 - 12)
	///  |     |     |    +--------- day of month (1 - 31)
	///  |     |     +----------- hour (0 - 23)
	///  |     +------------- min (0 - 59)
	///  +------------- sec (0 - 59)
	/// </summary>
	class Program
	{
		private const string HELP_MESSAGE =
	@"
Usage:
 Parse and execute the command specified
  Crontab -cronfile commandfile.ctb
 Get the next execution time in yyyy/MM/dd-HH:mm:ss and write it in the [varname] file or
 write it to stdout if no var specified
  Crontab -next ""10 * * * * * *"" (-var [varname])
 Print this help
  Crontab -h
File format:
 The command is separated by the time with a TAB.
 The parts of the time are separated between each other by single spaces.
 The crontab format is considered with seconds included.
 Comment lines start with #

 #This is a comment
 
Executable lines start immediatly

 * * * * * * *  [Command]

Crontab time specification:
 Format string with spaces between the entries
   *     *     *    *     *     *    * command to be executed
   -     -     -    -     -     -    -
   |     |     |    |     |     |    +----- year
   |     |     |    |     |     +----- day of week (0 - 6) (Sunday=0)
   |     |     |    |     +------- month (1 - 12)
   |     |     |    +--------- day of month (1 - 31)
   |     |     +----------- hour (0 - 23)
   |     +------------- min (0 - 59)
   +------------- sec (0 - 59)";


		static void Main(string[] args)
		{
			var commandParser = new CommandLineParser(args, HELP_MESSAGE, new ExitBehaviour());
			if (!commandParser.HasOneAndOnlyOne("next", "cronfile"))
			{
				commandParser.ShowHelp();
			}
			else if (commandParser.Has("next"))
			{
				var crontabEntry = commandParser["next"];
				const string format = "yyyy/MM/dd-HH:mm:ss";
				var crontab = new Crontab(crontabEntry, true);
				var next = crontab.Next();
				Console.WriteLine(next.ToString(format));
				if (commandParser.Has("var") && !string.IsNullOrWhiteSpace(commandParser["var"]))
				{
					File.WriteAllText(commandParser["var"], next.ToString(format) + "\n");
				}
			}
			else if (commandParser.Has("cronfile"))
			{
				var crontabFile = commandParser["cronfile"];
				if (File.Exists(crontabFile))
				{
					Console.WriteLine("Reading crontab config {0}.", crontabFile);
				}
				var readLines = File.ReadAllLines(crontabFile);
				var corntabEntries = ParseCrontabEntries(readLines);
				var crontabThread = new CrontabThread(corntabEntries);
				crontabThread.RunThread();
				Console.WriteLine("Crontab started.");
				Console.WriteLine("Press a key to terminate.");
				Console.ReadKey();
				Console.WriteLine("Crontab terminating.");
				crontabThread.Terminate();
				try
				{
					crontabThread.WaitTermination(1000);
				}
				catch (TimeoutException)
				{
					Console.WriteLine("Unable to terminate crontab. Proceeding with abort.");
					crontabThread.Terminate(true);
				}
				crontabThread.Dispose();
			}
			else
			{
				commandParser.ShowHelp();
			}
		}

		private static List<CrontabTask> ParseCrontabEntries(string[] readLines)
		{
			var crontabTasks = new List<CrontabTask>();
			foreach (var line in readLines)
			{
				var trimmedLine = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
				if (trimmedLine.StartsWith("#")) continue;
				string[] command = trimmedLine.Split('\t');
				if (command.Length != 2) continue;

				crontabTasks.Add(new CrontabTask
					{
						CommandLine = command[1],
						CrontabEntry = new Crontab(command[0], true)
					});
			}
			return crontabTasks;
		}
	}
}
