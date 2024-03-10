using LiveCharts;
using LiveCharts.Configurations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CronBlocks.UserControls.Wpf.TimedPlot;

public class TimedPlotModel
{
    public DateTime DateTime { get; set; }
    public double Value { get; set; }
}

public partial class TimedPlot : UserControl, INotifyPropertyChanged
{
    private static readonly int MAX_NUMBER_OF_VALUES = 250;

    private double _xAxisMin;
    private double _xAxisMax;
    private double _xAxisStep;

    private double _yAxisMin;
    private double _yAxisMax;
    private double _yAxisStep;

    private Visibility _plotVisibility1;
    private Visibility _plotVisibility2;

    private string _xAxisTitle;
    private string _yAxisTitle;

    private bool _isAutoYRangeEnabled;

    private DateTime startTime;

    public TimedPlot()
    {
        InitializeComponent();

        startTime = DateTime.Now;

        var mapper = Mappers.Xy<TimedPlotModel>()
            .X(model => model.DateTime.Ticks)
            .Y(model => model.Value);

        Charting.For<TimedPlotModel>(mapper);

        PlotValues1 = new ChartValues<TimedPlotModel>();
        PlotValues2 = new ChartValues<TimedPlotModel>();

        ClearData();

        TimeFormatter = value => new DateTime((long)value).ToString("mm:ss");
        ValueFormatter = value => value.ToString("0.00");

        XAxisStep = TimeSpan.FromSeconds(1).Ticks;
        XAxisUnit = TimeSpan.TicksPerSecond;

        SetXAxisLimits(startTime);

        YAxisStep = 1;
        YAxisMin = 0;
        YAxisMax = 5;

        _xAxisTitle = "";
        _yAxisTitle = "";

        IsAutoYRangeEnabled = false;

        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ChartValues<TimedPlotModel> PlotValues1 { get; set; }
    public ChartValues<TimedPlotModel> PlotValues2 { get; set; }

    public Func<double, string> TimeFormatter { get; set; }
    public Func<double, string> ValueFormatter { get; set; }

    public double XAxisUnit { get; set; }

    public double XAxisMin
    {
        get { return _xAxisMin; }
        set
        {
            _xAxisMin = value;
            OnPropertyChanged();
        }
    }

    public double XAxisMax
    {
        get { return _xAxisMax; }
        set
        {
            _xAxisMax = value;
            OnPropertyChanged();
        }
    }

    public double XAxisStep
    {
        get { return _xAxisStep; }
        set
        {
            _xAxisStep = value;
            OnPropertyChanged();
        }
    }

    public double YAxisMin
    {
        get { return _yAxisMin; }
        set
        {
            _yAxisMin = value;
            OnPropertyChanged();
        }
    }

    public double YAxisMax
    {
        get { return _yAxisMax; }
        set
        {
            _yAxisMax = value;
            OnPropertyChanged();
        }
    }

    public double YAxisStep
    {
        get { return _yAxisStep; }
        set
        {
            _yAxisStep = value;
            OnPropertyChanged();
        }
    }

    public Visibility PlotVisibility1
    {
        get { return _plotVisibility1; }
        set
        {
            _plotVisibility1 = value;
            OnPropertyChanged();
        }
    }

    public Visibility PlotVisibility2
    {
        get { return _plotVisibility2; }
        set
        {
            _plotVisibility2 = value;
            OnPropertyChanged();
        }
    }

    public string XAxisTitle
    {
        get { return _xAxisTitle; }
        set
        {
            _xAxisTitle = value;
            OnPropertyChanged();
        }
    }

    public string YAxisTitle
    {
        get { return _yAxisTitle; }
        set
        {
            _yAxisTitle = value;
            OnPropertyChanged();
        }
    }

    public bool IsAutoYRangeEnabled
    {
        get { return _isAutoYRangeEnabled; }
        set
        {
            _isAutoYRangeEnabled = value;
            OnPropertyChanged();
        }
    }

    public void Update(double value1, double value2)
    {
        DateTime now = DateTime.Now;

        PlotValues1.Add(new TimedPlotModel
        {
            DateTime = now,
            Value = value1
        });

        PlotValues2.Add(new TimedPlotModel
        {
            DateTime = now,
            Value = value2
        });

        SetXAxisLimits(now);

        if (IsAutoYRangeEnabled)
        {
            SetYAxisLimits(Math.Min(value1, value2), Math.Max(value1, value2));
        }

        if (PlotValues1.Count > MAX_NUMBER_OF_VALUES) PlotValues1.RemoveAt(0);
        if (PlotValues2.Count > MAX_NUMBER_OF_VALUES) PlotValues2.RemoveAt(0);
    }

    public void ClearData()
    {
        PlotValues1.Clear();
        PlotValues2.Clear();
        /*
        for (int i = 0; i < MAX_NUMBER_OF_VALUES; i++)
        {
            PlotValues1.Add(new TimedPlotModel()
            {
                DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
                Value = 0
            });

            PlotValues2.Add(new TimedPlotModel()
            {
                DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
                Value = 0
            });
        }*/
    }

    private void SetXAxisLimits(DateTime now)
    {
        XAxisMax = now.Ticks + TimeSpan.FromSeconds(0).Ticks;
        XAxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks;
    }

    private void SetYAxisLimits(double min, double max)
    {
        if (min < 0) min -= 1.0;
        if (max > 0) max += 1.0;

        if (min < YAxisMin)
            YAxisMin = min;

        if (max > YAxisMax)
            YAxisMax = max;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        if (PropertyChanged != null)
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
