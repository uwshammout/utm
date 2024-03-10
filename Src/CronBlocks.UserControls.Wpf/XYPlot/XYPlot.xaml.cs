using LiveCharts;
using LiveCharts.Configurations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CronBlocks.UserControls.Wpf.XYPlot;

public class XYPlotModel
{
    public double XValue { get; set; }
    public double YValue { get; set; }
}

public partial class XYPlot : UserControl, INotifyPropertyChanged
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

    private bool _isAutoRangeEnabled;

    public XYPlot()
    {
        InitializeComponent();

        var mapper = Mappers.Xy<XYPlotModel>()
            .X(model => model.XValue)
            .Y(model => model.YValue);

        Charting.For<XYPlotModel>(mapper);

        PlotValues1 = new ChartValues<XYPlotModel>();
        PlotValues2 = new ChartValues<XYPlotModel>();

        ClearData();

        XValueFormatter = value => value.ToString("0.00");
        YValueFormatter = value => value.ToString("0.00");

        XAxisStep = 1;
        XAxisMin = 0;
        XAxisMax = 5;

        YAxisStep = 1;
        YAxisMin = 0;
        YAxisMax = 5;

        _xAxisTitle = "";
        _yAxisTitle = "";

        IsAutoRangeEnabled = false;

        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ChartValues<XYPlotModel> PlotValues1 { get; set; }
    public ChartValues<XYPlotModel> PlotValues2 { get; set; }

    public Func<double, string> XValueFormatter { get; set; }
    public Func<double, string> YValueFormatter { get; set; }

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

    public bool IsAutoRangeEnabled
    {
        get { return _isAutoRangeEnabled; }
        set
        {
            _isAutoRangeEnabled = value;
            OnPropertyChanged();
        }
    }

    public void Update(double valueX1, double valueY1, double valueX2, double valueY2)
    {
        PlotValues1.Add(new XYPlotModel
        {
            XValue = valueX1,
            YValue = valueY1
        });

        PlotValues2.Add(new XYPlotModel
        {
            XValue = valueX2,
            YValue = valueY2
        });

        if (IsAutoRangeEnabled)
        {
            SetXAxisLimits(Math.Min(valueX1, valueX2), Math.Max(valueX1, valueX2));
            SetYAxisLimits(Math.Min(valueY1, valueY2), Math.Max(valueY1, valueY2));
        }

        if (PlotValues1.Count > MAX_NUMBER_OF_VALUES) PlotValues1.RemoveAt(0);
        if (PlotValues2.Count > MAX_NUMBER_OF_VALUES) PlotValues2.RemoveAt(0);
    }

    public void ClearData()
    {
        PlotValues1.Clear();
        PlotValues2.Clear();
    }

    private void SetXAxisLimits(double min, double max)
    {
        if (min < 0) min -= 1.0;
        if (max > 0) max += 1.0;

        if (min < XAxisMin)
            XAxisMin = min;

        if (max > XAxisMax)
            XAxisMax = max;
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
