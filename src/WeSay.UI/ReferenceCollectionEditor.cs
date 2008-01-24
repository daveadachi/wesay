using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.UI.AutoCompleteTextBox;


namespace WeSay.UI
{
	public partial class ReferenceCollectionEditor<KV, ValueT, KEY_CONTAINER> : FlowLayoutPanel
		where ValueT :  class
		where KEY_CONTAINER : IValueHolder<ValueT>, IReferenceContainer
	{
		private readonly IBindingList _chosenItems;
		private readonly IEnumerable<KV> _sourceChoices;
		private readonly IList<WritingSystem> _writingSystems;
		private readonly CommonEnumerations.VisibilitySetting _visibility;
		private readonly IChoiceSystemAdaptor<KV,ValueT,KEY_CONTAINER> _choiceSystemAdaptor;
		private IReportEmptiness _alternateEmptinessHelper;

		private int _popupWidth=-1;
		private bool _ignoreListChanged = false;

		public event EventHandler<CreateNewArgs> CreateNewTargetItem;

		public ReferenceCollectionEditor()
		{
			InitializeComponent();
		 }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="chosenItems">The set of chosen items we are displaying/editting</param>
		 /// <param name="sourceChoices"> The set of objects that the user can choose from. The AutoCompleteAdaptor is used
		 /// to convert these into display strings.</param>
		/// <param name="writingSystems">a list of writing systems ordered by preference</param>
		/// <param name="visibility"></param>
		/// <param name="adaptor">does all the conversion between keys, wrappers, actual objects, etc.</param>
		public ReferenceCollectionEditor(IBindingList chosenItems,
			IEnumerable<KV> sourceChoices,
			IList<WritingSystem> writingSystems,
			CommonEnumerations.VisibilitySetting visibility,
			IChoiceSystemAdaptor<KV,ValueT,KEY_CONTAINER> adaptor)
		{
			if (chosenItems == null)
				throw new ArgumentException("chosenItems");
			if (adaptor == null)
				throw new ArgumentException("adaptor");
			if (writingSystems == null)
				throw new ArgumentException("writingSystems");
			if (sourceChoices == null)
				throw new ArgumentException("sourceChoices");
			InitializeComponent();

			_chosenItems = chosenItems;
			_sourceChoices = sourceChoices;
			_writingSystems = writingSystems;
			_visibility = visibility;
			_choiceSystemAdaptor = adaptor;
			chosenItems.ListChanged += chosenItems_ListChanged;
		}

		// esa: I don't like this. We are relying on the CreateNewTargetItem
		// event to be wired up before we AddControls since this passes that
		// wiring on. If we just do AddControls in the constructor where
		// it otherwise would be natural, adding a delegate to CreateNewTargetItem
		// has no effect.
		// This waits until the Control has become "real" presumably actually shown
		protected override void  OnHandleCreated(EventArgs e)
		{
			AddControls();
			 base.OnHandleCreated(e);
		}

		void chosenItems_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!_ignoreListChanged && !ContainsFocus)
			{
				AddControls();
			}
		}

		public int PopupWidth
		{
			get { return _popupWidth; }
			set { _popupWidth = value; }
		}

		void OnChildLostFocus(object sender, EventArgs e)
		{
			if(!ContainsFocus)//doing cleanup while the user is in the area will lead to much grief
			{
				IReportEmptiness x = _alternateEmptinessHelper;
				if(x==null)
				{
					x = _chosenItems as IReportEmptiness;
				}
				if (x != null)
				{
					x.RemoveEmptyStuff();
				}
			}
		}
		public IReportEmptiness AlternateEmptinessHelper
		{
			get { return _alternateEmptinessHelper; }
			set { _alternateEmptinessHelper = value; }
		}

		private void AddControls()
		{
			if (DesignMode)
				return;
			SuspendLayout();
			if (Parent != null)
			{
				BackColor = Parent.BackColor;
			}

			Controls.Clear();
			foreach (KEY_CONTAINER item in _chosenItems)
			{
				AutoCompleteWithCreationBox<KV, ValueT> picker = MakePicker();
				picker.Box.Tag =item;
				picker.Box.SelectedItem = _choiceSystemAdaptor.GetKeyValueFromKey_Container(item);
				if(picker.Box.SelectedItem ==null)//couldn't find a match for the key
				{
					picker.Box.Text = item.Key; // the box will recognize the problem and display a red background
				}

				//the binding itself doesn't need to be "owned" by us... it controls its own lifetime
				SimpleBinding<ValueT> binding = new SimpleBinding<ValueT>(item, picker);

				Controls.Add(picker);
			}
			//add a blank to type in
			if(_visibility != CommonEnumerations.VisibilitySetting.ReadOnly)
			{
				AddEmptyPicker();
			}
			ResumeLayout(false);
			PerformLayout();
		}

		private void AddEmptyPicker()
		{
			AutoCompleteWithCreationBox<KV, ValueT> emptyPicker = MakePicker();
			emptyPicker.ValueChanged += emptyPicker_ValueChanged;
			Controls.Add(emptyPicker);
		}

		void emptyPicker_ValueChanged(object sender, EventArgs e)
		{
			AutoCompleteWithCreationBox<KV, ValueT> picker = (AutoCompleteWithCreationBox<KV, ValueT>) sender;
			KV kv = (KV) picker.Box.SelectedItem;
			if (kv != null)
			{
				picker.ValueChanged -= emptyPicker_ValueChanged;
				_ignoreListChanged = true;
				KEY_CONTAINER newGuy = (KEY_CONTAINER) _chosenItems.AddNew();
				_choiceSystemAdaptor.UpdateKeyContainerFromKeyValue(kv, newGuy);
				_ignoreListChanged = false;

				//the binding itself doesn't need to be "owned" by us... it controls its own lifetime
				SimpleBinding<ValueT> binding = new SimpleBinding<ValueT>(newGuy, picker);

				AddEmptyPicker();
			}
		}

		private AutoCompleteWithCreationBox<KV, ValueT> MakePicker()
		{
			AutoCompleteWithCreationBox<KV, ValueT> picker = new AutoCompleteWithCreationBox<KV, ValueT>(_visibility);
			picker.Box.FormToObectFinder = _choiceSystemAdaptor.GetValueFromFormNonGeneric;

			picker.GetKeyValueFromValue = _choiceSystemAdaptor.GetKeyValueFromValue;
			picker.GetValueFromKeyValue = _choiceSystemAdaptor.GetValueFromKeyValue;
			picker.Box.ItemDisplayStringAdaptor = _choiceSystemAdaptor;
			picker.Box.Mode = WeSayAutoCompleteTextBox.EntryMode.List;
			picker.Box.Items = _sourceChoices;
			picker.Box.WritingSystem = _writingSystems[0];
			picker.Box.MinimumSize = new Size(20, 10);
			picker.Box.ItemFilterer = _choiceSystemAdaptor.GetItemsToOffer;
			picker.Box.PopupWidth = _popupWidth;

			picker.Box.LostFocus += OnChildLostFocus;

			if (CreateNewTargetItem != null)
			{
				picker.CreateNewClicked += OnCreateNewClicked;
			}
			return picker;
		}

		internal void OnCreateNewClicked(object sender, CreateNewArgs e)
		{
			if (CreateNewTargetItem!=null)
			{
				CreateNewTargetItem.Invoke(this, e);
			}
		}
	}
}
