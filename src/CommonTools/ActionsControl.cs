using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mono.Addins;
using WeSay.AddinLib;
using WeSay.Foundation;
using WeSay.Project;

namespace WeSay.CommonTools
{
    public partial class ActionsControl : UserControl, ITask
    {
        private bool _isActive;
        private bool _wasLoaded=false;

        public ActionsControl()
        {
            InitializeComponent();
       }

        #region ITask

        public bool MustBeActivatedDuringPreCache
        {
            get{ return false;}
        }

        #region ITask Members

        public void RegisterWithCache(ViewTemplate viewTemplate)
        {
            
        }

        #endregion

        public void Activate()
        {
            //get everything into the LIFT file
            if (WeSayWordsProject.Project.LiftUpdateService != null) // can be null when SampleDataProcessor runs
            {
                WeSayWordsProject.Project.LiftUpdateService.DoLiftUpdateNow(true);
            }
            if (!_wasLoaded)
            {
                _wasLoaded = true;
                LoadAddins();
            }
            _isActive = true;

        }

        private void LoadAddins()
        {
            try
            {
             List<string> alreadyFound = new List<string>();
               Palaso.Reporting.Logger.WriteMinorEvent("Loading Addins");
               _addinsList.Controls.Clear();
               _addinsList.RowStyles.Clear();
               if (!AddinManager.IsInitialized)
                {
                    //                AddinManager.Initialize(Application.UserAppDataPath);
                    //                AddinManager.Registry.Rebuild(null);
                    //                AddinManager.Shutdown();
                    AddinManager.Initialize(Application.UserAppDataPath);
                    AddinManager.Registry.Update(null);
                    //these (at least AddinLoaded) does get called after initialize, when you
                    //do a search for objects (e.g. GetExtensionObjects())

                    //TODO: I added these back on 13 oct because I was seeing no addins!
                    AddinManager.Registry.Rebuild(null);
                    AddinManager.Shutdown();
                    AddinManager.Initialize(Application.UserAppDataPath);
                }

                foreach (IWeSayAddin addin in AddinManager.GetExtensionObjects(typeof(IWeSayAddin)))
                {
                    if (AddinSet.Singleton.DoShowInWeSay(addin.ID))
                    {
                        //this alreadyFound business is a hack to prevent duplication in some
                        // situation I haven't tracked down yet.
                        if (!alreadyFound.Contains(addin.ID))
                        {
                            alreadyFound.Add(addin.ID);
                          AddAddin(addin);
                        }
                    
                    }
                }

                //            AddAddin(new ComingSomedayAddin("Send My Work to Sangkran", "Send email containing all your WeSay work to your advisor.",
                //                 WeSay.CommonTools.Properties.Resources.emailAction));
            }
            catch (Exception error)
            {
                Palaso.Reporting.ErrorReport.ReportNonFatalMessage(
                    "WeSay encountered an error while looking for Addins (e.g., Actions).  The error was: {0}",
                    error.Message);
            }
        }




        private void AddAddin(IWeSayAddin addin)
        {
            ActionItemControl control = new ActionItemControl(addin,false, null);
            control.TabIndex = _addinsList.RowCount;
            control.Launch += OnLaunchAction;

            _addinsList.SuspendLayout();
            _addinsList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _addinsList.Controls.Add(control);
            _addinsList.ResumeLayout();
        }

        private void OnLaunchAction(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                ((IWeSayAddin)sender).Launch(null, WeSay.Project.WeSayWordsProject.Project.GetProjectInfoForAddin());
            }
            catch (Exception error)
            {
                Palaso.Reporting.ErrorReport.ReportNonFatalMessage(error.Message);
            }

            Cursor.Current = Cursors.Default;
        }

        public void Deactivate()
        {
            if(!IsActive)
            {
                throw new InvalidOperationException("Deactivate should only be called once after Activate.");
            }
           // this._vbox.Clear();
            _isActive = false;
        }

        #region ITask Members

        public void GoToUrl(string url)
        {
            
        }

        #endregion

        public bool IsActive
        {
            get { return this._isActive; }
        }

        public string Label
        {
            get { return StringCatalog.Get("~Actions"); }
        }

        public Control Control
        {
            get { return this; }
        }

        public bool IsPinned
        {
            get
            {
                return true;
            }
        }

        public int Count
        {
            get
            {
                return CountNotRelevant;
            }
        }
        public int ExactCount
        {
            get
            {
                return CountNotRelevant;
            }
        }

        private const int CountNotRelevant = -1;
        /// <summary>
        /// Not relevant for this task
        /// </summary>
        public int ReferenceCount
        {
            get
            {
                return CountNotRelevant;
            }
        }

        public string Description
        {
            get
            {
                return StringCatalog.Get("~Backup, print, etc.", "The description of the Actions task.");
            }
        }

        #endregion

    }
}