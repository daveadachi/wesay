﻿using System;
using System.IO;
using Palaso.Code;
using Palaso.Reporting;
using WeSay.Project;

namespace WeSay.ConfigTool.NewProjectCreation
{
	/// <summary>
	/// This class should be on its way to being un-needed, with the advent of "Send/Receive".  Its use was primarily pre-chorus,
	/// pre-lift bridge, when we wanted to copy lift-related files around.  In the new, better system, we are givena a chorus
	/// repository with all the lift files, just lacking the wesay specific ones. Then, ProjectFromLiftFolderCreator is
	/// used to just add the files we need for WeSay.
	/// This class uses that, too, but it also has to create a folder & copy files around.
	/// </summary>
	public class ProjectFromRawFLExLiftFilesCreator
	{
		public static bool Create(string pathToNewDirectory, string pathToSourceLift)
		{
			try
			{
				Logger.WriteEvent(@"Starting Project creation from " + pathToSourceLift);

				if (!ReportIfLocked(pathToSourceLift))
					return false;

				if (!CreateProjectDirectory(pathToNewDirectory))
					return false;

				CopyOverLiftFile(pathToSourceLift, pathToNewDirectory);

				CopyOverRangeFileIfExists(pathToSourceLift, pathToNewDirectory);

				CopyOverLdmlFiles(pathToSourceLift, BasilProject.GetPathToLdmlWritingSystemsFolder(pathToNewDirectory));

				//TODO: this, I think, won't be needed at all once we have the project invoking ProjectFromLiftFolderCreator automatically
				//when we open any project folder
				//ProjectFromLiftFolderCreator.PrepareLiftFolderForWeSay(pathToNewDirectory);

				Logger.WriteEvent(@"Finished Importing");
				return true;

			}
			catch(Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(e, "WeSay was unable to finish importing that LIFT file.  If you cannot fix the problem yourself, please zip and send the exported folder to issues (at) wesay (dot) org");
				try
				{
					Logger.WriteEvent(@"Removing would-be target directory");
					Directory.Delete(pathToNewDirectory, true);
				}
				catch (Exception)
				{
					//swallow
				}
				return false;
			}
		}

		private static void CopyOverLdmlFiles(string pathToSourceLift, string pathToNewDirectory)
		{
			foreach (string pathToLdml in Directory.GetFiles(Path.GetDirectoryName(pathToSourceLift), "*.ldml"))
			{
				string fileName = Path.GetFileName(pathToLdml);
				Logger.WriteMinorEvent(@"Copying LDML file " + fileName);
				File.Copy(pathToLdml, Path.Combine(pathToNewDirectory, fileName), true);
			}
		}

		private static void CopyOverLiftFile(string pathToSourceLift, string pathToNewDirectory)
		{
			var projectName = Path.GetFileNameWithoutExtension(pathToNewDirectory);
			var pathToTargetLift = Path.Combine(pathToNewDirectory, projectName+".lift");
			Logger.WriteMinorEvent(@"Copying Lift file " + pathToSourceLift);
			File.Copy(pathToSourceLift, pathToTargetLift, true);
		}

		private static void CopyOverRangeFileIfExists(string pathToSourceLift, string pathToNewDirectory)
		{
			var projectName = Path.GetFileNameWithoutExtension(pathToNewDirectory);
			var  pathToSourceRange= pathToSourceLift+ "-ranges";
			if(File.Exists(pathToSourceRange))
			{
				var pathToTargetRanges = Path.Combine(pathToNewDirectory, projectName + ".lift-ranges");
				Logger.WriteMinorEvent(@"Copying Range file " + pathToTargetRanges);
				File.Copy(pathToSourceRange, pathToTargetRanges, true);
			}
		}

		private static bool CreateProjectDirectory(string directory)
		{
			try
			{
				RequireThat.Directory(directory).DoesNotExist();
				WeSayWordsProject.CreateEmptyProjectFiles(directory);
			}
			catch (Exception e)
			{
				ErrorReport.NotifyUserOfProblem(
					"WeSay was not able to create a project there. \r\n" + e.Message);
				return false;
			}
			return true;
		}


		private static bool ReportIfLocked(string lift)
		{
			try
			{
				using (var lockTest = File.OpenRead(lift))
				{
				}
			}
			catch (Exception)
			{
				ErrorReport.NotifyUserOfProblem("Could not open the LIFT file.  Is some other program reading or writing it?");
				return false;
			}
			return true;
		}

		/*       public static IEnumerable<string> GetUniqueRegexMatches(string inputPath, string pattern)
		{
//            var ids = GetUniqueRegexMatches(path, "lang:b*=:b*((\"{.*}\")|('{.*}'))");

   var unique = new List<string>();//review probably slow for this purpose
			List<string> matches= new List<string>();
			Regex regex = new Regex(pattern, RegexOptions.Compiled);
			using (StreamReader reader = File.OpenText(inputPath))
			{
				while (!reader.EndOfStream)
				{
					foreach (Match match in regex.Matches(reader.ReadLine()))
					{
						if (match.Captures.Count > 0)
						{
							var id = match.Captures[0].ToString();
							if (!unique.Contains(id))
							{
								unique.Add(id);
							}
						}
					}
				}
				reader.Close();
			}
			return unique;
		}*/
	}
}