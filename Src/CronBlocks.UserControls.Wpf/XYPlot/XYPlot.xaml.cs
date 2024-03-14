using LiveCharts;
using LiveCharts.Configurations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CronBlocks.UserControls.Wpf.XYPlot;

public class XYPlotModel
{
    public double XValue { get; set; }
    public double YValue { get; set; }
}

public partial class XYPlot : UserControl, INotifyPropertyChanged
{
    private static readonly int MAX_NUMBER_OF_VALUES = 500;

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

    private Brush _axesTextColor, _axesLinesColor;
    private Brush _plotColor1, _plotColor2;

    public XYPlot()
    {
        InitializeComponent();
        DataContext = this;

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

        Background = new SolidColorBrush(Colors.Black);
        AxesTextColor = new SolidColorBrush(Colors.White);
        AxesLinesColor = AxesTextColor;
        PlotColor1 = new SolidColorBrush(Colors.Orange);
        PlotColor2 = new SolidColorBrush(Colors.Cyan);
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
            Notify();
        }
    }
    public double XAxisMax
    {
        get { return _xAxisMax; }
        set
        {
            _xAxisMax = value;
            Notify();
        }
    }
    public double XAxisStep
    {
        get { return _xAxisStep; }
        set
        {
            _xAxisStep = value;
            Notify();
        }
    }

    public double YAxisMin
    {
        get { return _yAxisMin; }
        set
        {
            _yAxisMin = value;
            Notify();
        }
    }
    public double YAxisMax
    {
        get { return _yAxisMax; }
        set
        {
            _yAxisMax = value;
            Notify();
        }
    }
    public double YAxisStep
    {
        get { return _yAxisStep; }
        set
        {
            _yAxisStep = value;
            Notify();
        }
    }

    public Visibility PlotVisibility1
    {
        get { return _plotVisibility1; }
        set
        {
            _plotVisibility1 = value;
            Notify();
        }
    }
    public Visibility PlotVisibility2
    {
        get { return _plotVisibility2; }
        set
        {
            _plotVisibility2 = value;
            Notify();
        }
    }

    public string XAxisTitle
    {
        get { return _xAxisTitle; }
        set
        {
            _xAxisTitle = value;
            Notify();
        }
    }
    public string YAxisTitle
    {
        get { return _yAxisTitle; }
        set
        {
            _yAxisTitle = value;
            Notify();
        }
    }

    public bool IsAutoRangeEnabled
    {
        get { return _isAutoRangeEnabled; }
        set
        {
            _isAutoRangeEnabled = value;
            Notify();
        }
    }

    public Brush AxesTextColor
    {
        get { return _axesTextColor; }
        set
        {
            if (value != _axesTextColor) { _axesTextColor = value; Notify(); }
        }
    }
    public Brush AxesLinesColor
    {
        get { return _axesLinesColor; }
        set
        {
            if (value != _axesLinesColor) { _axesLinesColor = value; Notify(); }
        }
    }
    public Brush PlotColor1
    {
        get { return _plotColor1; }
        set
        {
            if (value != _plotColor1) { _plotColor1 = value; Notify(); }
        }
    }
    public Brush PlotColor2
    {
        get { return _plotColor2; }
        set
        {
            if (value != _plotColor2) { _plotColor2 = value; Notify(); }
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

    protected virtual void Notify([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
