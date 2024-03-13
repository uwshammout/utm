using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CronBlocks.UTM.UserControls;

public partial class DigitalValueDisplay : UserControl, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Notify([CallerMemberName] string prop = null!)
	{
		if (prop != null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}

	public delegate void ButtonClickEvent(object sender, RoutedEventArgs e);
	public event ButtonClickEvent? ButtonClick;

	private Brush _backgroundColor = null!;
	private Brush _headingColor = null!;
	private Brush _unitColor = null!;
	private Brush _valueColor = null!;
	private Brush _maxColor = null!;
	private Brush _buttonBackgroundColor = null!;
	private Brush _buttonForegroundColor = null!;
	private double _headingFontSize = 0.0;
	private double _unitFontSize = 0.0;
	private double _valueFontSize = 0.0;
	private double _maxFontSize = 0.0;
	private double _buttonFontSize = 0.0;
	private string _heading = null!;
	private string _unit = null!;
	private double _value = 0.0;
	private bool _enableMaxValue = false;
	private double _maxValue = 0.0;
	private Visibility _buttonVisibility = Visibility.Collapsed;
	private string _buttonText = null!;

	public DigitalValueDisplay()
	{
		InitializeComponent();
		DataContext = this;

		BackgroundColor = new SolidColorBrush(Colors.Black);
		HeadingColor = new SolidColorBrush(Colors.FloralWhite);
		UnitColor = new SolidColorBrush(Colors.Beige);
		ValueColor = new SolidColorBrush(Colors.Orange);
		MaxColor = new SolidColorBrush(Colors.Silver);
		ButtonBackgroundColor = BackgroundColor;
		ButtonForegroundColor = new SolidColorBrush(Colors.Silver);
		HeadingFontSize = 14;
		UnitFontSize = 9;
		ValueFontSize = 42;
		MaxFontSize = 24;
		ButtonFontSize = 10;
		Heading = "--";
		Unit = "";
		Value = 0.0;
		EnableMaxValue = false;
		MaxValue = 0.0;
		ButtonVisibility = Visibility.Collapsed;
		ButtonText = "--";
	}

	public void Reset()
	{
		Value = 0.0;
		MaxValue = 0.0;
	}

	public Brush BackgroundColor
	{
		get { return _backgroundColor; }
		set
		{
			if (_backgroundColor != value) { _backgroundColor = value; Notify(); }
		}
	}
	public Brush HeadingColor
	{
		get { return _headingColor; }
		set
		{
			if (_headingColor != value) { _headingColor = value; Notify(); }
		}
	}
	public Brush UnitColor
	{
		get { return _unitColor; }
		set
		{
			if (_unitColor != value) { _unitColor = value; Notify(); }
		}
	}
	public Brush ValueColor
	{
		get { return _valueColor; }
		set
		{
			if (_valueColor != value) { _valueColor = value; Notify(); }
		}
	}
	public Brush MaxColor
	{
		get { return _maxColor; }
		set
		{
			if (_maxColor != value) { _maxColor = value; Notify(); }
		}
	}
	public Brush ButtonBackgroundColor
	{
		get { return _buttonBackgroundColor; }
		set
		{
			if (_buttonBackgroundColor != value) { _buttonBackgroundColor = value; Notify(); }
		}
	}
	public Brush ButtonForegroundColor
	{
		get { return _buttonForegroundColor; }
		set
		{
			if (_buttonForegroundColor != value) { _buttonForegroundColor = value; Notify(); }
		}
	}
	public double HeadingFontSize
	{
		get { return _headingFontSize; }
		set
		{
			if (_headingFontSize != value) { _headingFontSize = value; Notify(); }
		}
	}
	public double UnitFontSize
	{
		get { return _unitFontSize; }
		set
		{
			if (_unitFontSize != value) { _unitFontSize = value; Notify(); }
		}
	}
	public double ValueFontSize
	{
		get { return _valueFontSize; }
		set
		{
			if (_valueFontSize != value) { _valueFontSize = value; Notify(); }
		}
	}
	public double MaxFontSize
	{
		get { return _maxFontSize; }
		set
		{
			if (_maxFontSize != value) { _maxFontSize = value; Notify(); }
		}
	}
	public double ButtonFontSize
	{
		get { return _buttonFontSize; }
		set
		{
			if (_buttonFontSize != value) { _buttonFontSize = value; Notify(); }
		}
	}
	public string Heading
	{
		get { return _heading; }
		set
		{
			if (_heading != value) { _heading = value; Notify(); }
		}
	}
	public string Unit
	{
		get { return _unit; }
		set
		{
			if (_unit != value) { _unit = value; Notify(); }
		}
	}
	public double Value
	{
		get { return _value; }
		set
		{
			if (_value != value)
			{
				_value = value;
				Notify();

				if (_value > _maxValue)
				{
					MaxValue = _value;
				}
			}
		}
	}
	public bool EnableMaxValue
	{
		get { return _enableMaxValue; }
		set
		{
			if (_enableMaxValue != value) { _enableMaxValue = value; Notify(); }

			if (_enableMaxValue) MaxValuePanel.Visibility = Visibility.Visible;
			else MaxValuePanel.Visibility = Visibility.Collapsed;
		}
	}
	public double MaxValue
	{
		get { return _maxValue; }
		set
		{
			if (_maxValue != value) { _maxValue = value; Notify(); }
		}
	}
	public Visibility ButtonVisibility
	{
		get { return _buttonVisibility; }
		set
		{
			if (_buttonVisibility != value) { _buttonVisibility = value; Notify(); }
		}
	}
	public string ButtonText
	{
		get { return _buttonText; }
		set
		{
			if (_buttonText != value) { _buttonText = value; Notify(); }
		}
	}

	private void ControlButton_Click(object sender, RoutedEventArgs e)
	{
		ButtonClick?.Invoke(this, e);
	}
}
