using System.Drawing;

namespace WeSay.UI
{
	public class DisplaySettings
	{
		public static DisplaySettings Default = new DisplaySettings(Color.FromArgb(235,255,215));

		public DisplaySettings()
		{

		}
		public DisplaySettings(Color backgroundColor)
		{
			_backgroundColor = backgroundColor;
		}

		private Color _backgroundColor;
		public Color BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
		}


	}
}