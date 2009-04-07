using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.i8n;
using WeSay.Foundation;
using WeSay.Project;

namespace WeSay.ConfigTool
{
	public partial class WritingSystemSetup: ConfigurationControlBase
	{
		public WritingSystemSetup(ILogger logger)
			: base("set up fonts, keyboards, and sorting", logger)
		{
			InitializeComponent();
			Resize += WritingSystemSetup_Resize;
			_basicControl.Logger = logger;
			_fontControl.Logger = logger;
			_sortControl.Logger = logger;
		}

		private void WritingSystemSetup_Resize(object sender, EventArgs e)
		{
			//this is part of dealing with .net not adjusting stuff well for different dpis
			splitContainer1.Dock = DockStyle.None;
			splitContainer1.Width = Width - 25;
		}

		public void WritingSystemSetup_Load(object sender, EventArgs e)
		{
			if (DesignMode)
			{
				return;
			}

			LoadWritingSystemListBox();
			//for checking that ids are unique
			_basicControl.WritingSystemCollection = BasilProject.Project.WritingSystems;
		}

		private void LoadWritingSystemListBox()
		{
			_wsListBox.Items.Clear();
			foreach (WritingSystem w in BasilProject.Project.WritingSystems.Values)
			{
				_wsListBox.Items.Add(new WsDisplayProxy(w));
			}
			_wsListBox.Sorted = true;
			if (_wsListBox.Items.Count > 0)
			{
				_wsListBox.SelectedIndex = 0;
			}
		}

		private void _wsListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateSelection();
		}

		/// <summary>
		/// nb: seperate from the event handler because the handler isn't called if the last item is deleted
		/// </summary>
		private void UpdateSelection()
		{
			_tabControl.Visible = SelectedWritingSystem != null;
			if (SelectedWritingSystem == null)
			{
				Refresh();
				return;
			}

			_btnRemove.Enabled = true;
			//                (SelectedWritingSystem != BasilProject.Project.WritingSystems.AnalysisWritingSystemDefault)
			//              && (SelectedWritingSystem != BasilProject.Project.WritingSystems.VernacularWritingSystemDefault);
			_basicControl.WritingSystem = SelectedWritingSystem;
			_sortControl.WritingSystem = SelectedWritingSystem;
			_fontControl.WritingSystem = SelectedWritingSystem;

			_sortingPage.Enabled = !SelectedWritingSystem.IsAudio;
			_fontsPage.Enabled = !SelectedWritingSystem.IsAudio;
		}

		private WritingSystem SelectedWritingSystem
		{
			get
			{
				WsDisplayProxy proxy = _wsListBox.SelectedItem as WsDisplayProxy;
				if (proxy != null)
				{
					return proxy.WritingSystem;
				}
				else
				{
					return null;
				}
			}
		}

		private void _btnRemove_Click(object sender, EventArgs e)
		{
			if (SelectedWritingSystem != null &&
				BasilProject.Project.WritingSystems.ContainsKey(SelectedWritingSystem.Id))
			{
				var doomedId = SelectedWritingSystem.Id;
				BasilProject.Project.WritingSystems.Remove(SelectedWritingSystem.Id);
				LoadWritingSystemListBox();
				UpdateSelection();
				_logger.WriteConciseHistoricalEvent(StringCatalog.Get("Removed writing system '{0}'", "Checkin Description in WeSay Config Tool used when you remove a writing system."), doomedId);

			}
		}

		private void _btnAddWritingSystem_Click(object sender, EventArgs e)
		{
			WritingSystem w = null;
			string[] keys = {"xx", "x1", "x2", "x3"};
			foreach (string s in keys)
			{
				if (!BasilProject.Project.WritingSystems.ContainsKey(s))
				{
					Font font;
					try
					{
						font = new Font("Doulos SIL", 12);
					}
					catch(Exception )
					{
					   font = new Font(System.Drawing.SystemFonts.DefaultFont.SystemFontName, 12);
					}

					w = new WritingSystem(s, font);
					break;
				}
			}
			if (w == null)
			{
				ErrorReport.NotifyUserOfProblem("Could not produce a unique ID.");
			}
			else
			{
				BasilProject.Project.WritingSystems.Add(w.Id, w);
				WsDisplayProxy item = new WsDisplayProxy(w);
				_wsListBox.Items.Add(item);
				_wsListBox.SelectedItem = item;
			}

			_logger.WriteConciseHistoricalEvent(StringCatalog.Get("Added writing system", "Checkin Description in WeSay Config Tool used when you add a writing system."));

		}

		/// <summary>
		/// Called when, for example, the user changes the id of the selected ws
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWritingSystemIdChanged(object sender, EventArgs e)
		{
			WritingSystem ws = sender as WritingSystem;
			PropertyValueChangedEventArgs args = e as PropertyValueChangedEventArgs;
			if (args != null && args.ChangedItem.PropertyDescriptor.Name == "Id")
			{
				string oldId = args.OldValue.ToString();
				if(!WeSayWordsProject.Project.MakeWritingSystemIdChange(ws, oldId))
				{
					ws.Id = oldId; //couldn't make the change
				}
				//                Reporting.ErrorReporter.NotifyUserOfProblem(
				//                    "Currently, WeSay does not make a corresponding change to the id of this writing system in your LIFT xml file.  Please do that yourself, using something like NotePad to search for lang=\"{0}\" and change to lang=\"{1}\"",
				//                    ws.Id, oldId);
			}

			//_wsListBox.Refresh(); didn't work
			//this.Refresh();   didn't work
			for (int i = 0;i < _wsListBox.Items.Count;i++)
			{
				_wsListBox.Items[i] = _wsListBox.Items[i];
			}
			UpdateSelection();
			if (args != null && args.ChangedItem.PropertyDescriptor.Name == "Id")
			{
				LoadWritingSystemListBox();
				foreach (WsDisplayProxy o in _wsListBox.Items)
				{
					if (o.WritingSystem == ws)
					{
						_wsListBox.SelectedItem = o;
						break;
					}
				}
			}
		}

		private void OnIsAudioChanged(object sender, EventArgs e)
		{
			UpdateSelection();
		}
	}

	/// <summary>
	/// An item to stick in the listview which represents a ws
	/// </summary>
	public class WsDisplayProxy
	{
		private WritingSystem _writingSystem;

		public WsDisplayProxy(WritingSystem ws)
		{
			_writingSystem = ws;
		}

		public WritingSystem WritingSystem
		{
			get { return _writingSystem; }
			set { _writingSystem = value; }
		}

		public override string ToString()
		{
			string s = _writingSystem.ToString();

			switch (s)
			{
				default:
					if (s == WritingSystem.IdForUnknownVernacular)
					{
						s += " (Change to your Vernacular)";
					}
					break;
				case "fr":
					s += " (French)";
					break;
				case "id":
					s += " (Indonesian)";
					break;
				case "tpi":
					s += " (Tok Pisin)";
					break;
				case "th":
					s += " (Thai)";
					break;
				case "es":
					s += " (Spanish)";
					break;
				case "en":
					s += " (English)";
					break;
				case "my":
					s += " (Burmese)";
					break;
			}

			return s;
		}
	}
}