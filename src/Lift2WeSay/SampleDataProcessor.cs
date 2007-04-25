using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.Project;

namespace WeSay.SampleDataProcessor
{
	class SampleDataProcessor
	{
		[STAThread]
		static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				PrintUsage();
				return 1;
			}
			string sourcePath = args[0];
			if (!File.Exists(sourcePath))
			{
				Console.WriteLine(string.Format("Cannot find file {0})", sourcePath));
				return 1;
			}

			Console.WriteLine("SampleDataProcessor is processing "+ sourcePath);


			ConsoleProgress progress = new ConsoleProgress();
			progress.Log += new EventHandler<ProgressState.LogEvent>(progress_Log);

			new WeSayWordsProject();
			WeSayWordsProject.Project.LoadFromLiftLexiconPath(sourcePath);
			CacheBuilder builder = new CacheBuilder(sourcePath);
			BackgroundWorker cacheBuildingWork = new BackgroundWorker();
			cacheBuildingWork.DoWork += new DoWorkEventHandler(builder.OnDoWork);
			cacheBuildingWork.RunWorkerAsync(progress);
			while (true)
			{
				switch (progress.State)
				{
					case ProgressState.StateValue.NotStarted:
						break;
					case ProgressState.StateValue.Busy:
						break;
					case ProgressState.StateValue.Finished:
						Console.WriteLine(string.Empty);
						Console.WriteLine("Done.");
						return 0;
					case ProgressState.StateValue.StoppedWithError:
						Console.Error.WriteLine(string.Empty);
						Console.Error.WriteLine("Error. Unable to complete import.");
						return 3;
					default:
						break;
				}
				Thread.Sleep(100);
			}
		}

		static void progress_Log(object sender, ProgressState.LogEvent e)
	  {
		Console.Error.WriteLine(e.message);
	  }

		private static void PrintUsage()
		{
			Console.WriteLine("SampleDataProcessor inputLiftFilePath");
		}
	}
}