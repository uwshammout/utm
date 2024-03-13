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
	public event ButtonClickEvent? ButtonClicked;

	private Brush _background = null!;
	private Brush _headingColor = null!;
	private Brush _valueColor = null!;
	private Brush _maxColor = null!;
	private Brush _buttonBackground = null!;
	private Brush _buttonForeground = null!;
	private string _heading = null!;
	private double _value = 0.0;
	private bool _enableMaxValue = false;
	private double _maxValue = 0.0;
	private Visibility _buttonVisibility = Visibility.Collapsed;
	private string _buttonText = null!;

	public DigitalValueDisplay()
	{
		InitializeComponent();
		DataContext = this;

		Background = new SolidColorBrush(Colors.Black);
		HeadingColor = new SolidColorBrush(Colors.FloralWhite);
		ValueColor = new SolidColorBrush(Colors.Orange);
		MaxColor = new SolidColorBrush(Colors.Silver);
		ButtonBackground = new SolidColorBrush(Colors.Black);
		ButtonForeground = new SolidColorBrush(Colors.Silver);
		Heading = "--";
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

	public new Brush Background
	{
		get { return _background; }
		set
		{
			if (_background != value) { _background = value; Notify(); }
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
	public Brush ButtonBackground
	{
		get { return _buttonBackground; }
		set
		{
			if (_buttonBackground != value) { _buttonBackground = value; Notify(); }
		}
	}
	public Brush ButtonForeground
	{
		get { return _buttonForeground; }
		set
		{
			if (_buttonForeground != value) { _buttonForeground = value; Notify(); }
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
}
