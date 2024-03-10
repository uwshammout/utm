using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CronBlocks.UserControls.Wpf.ThreePointerDial;

public partial class ThreePointerDial : UserControl
{
    private ThreePointerDialViewModel viewModel;

    public ThreePointerDial()
    {
        InitializeComponent();

        viewModel = new ThreePointerDialViewModel();
        DataContext = viewModel;

        IsDial1Visible = true;
        IsDial2Visible = true;
        IsDial3Visible = true;
        
        GaugeMinValue = 0;
        GaugeMaxValue = 10.0;

        Dial1Value = 0;
        Dial2Value = 0;
        Dial3Value = 0;

        backgroundColor = viewModel.BackgroundColor;
        borderColor = viewModel.BorderColor;

        titleTextColor = viewModel.TitleTextColor;
        gradingTextColor = viewModel.GradingTextColor;
        gradingLineColor = viewModel.GradingLineColor;

        dial1Color = viewModel.Dial1Color;
        dial2Color = viewModel.Dial2Color;
        dial3Color = viewModel.Dial3Color;

        dial1BorderColor = viewModel.Dial1BorderColor;
        dial2BorderColor = viewModel.Dial2BorderColor;
        dial3BorderColor = viewModel.Dial3BorderColor;
    }

    #region Dial Visibility
    private bool isDial1Visible = false;
    private bool isDial2Visible = false;
    private bool isDial3Visible = false;

    public bool IsDial1Visible
    {
        get => isDial1Visible;
        set
        {
            if (isDial1Visible != value)
            {
                isDial1Visible = value;
                if (value)
                {
                    viewModel.Dial1Visibility = Visibility.Visible;
                }
                else
                {
                    viewModel.Dial1Visibility = Visibility.Hidden;
                }
            }
        }
    }

    public bool IsDial2Visible
    {
        get => isDial2Visible;
        set
        {
            if (isDial2Visible != value)
            {
                isDial2Visible = value;
                if (value)
                {
                    viewModel.Dial2Visibility = Visibility.Visible;
                }
                else
                {
                    viewModel.Dial2Visibility = Visibility.Hidden;
                }
            }
        }
    }

    public bool IsDial3Visible
    {
        get => isDial3Visible;
        set
        {
            if (isDial3Visible != value)
            {
                isDial3Visible = value;
                if (value)
                {
                    viewModel.Dial3Visibility = Visibility.Visible;
                }
                else
                {
                    viewModel.Dial3Visibility = Visibility.Hidden;
                }
            }
        }
    }
    #endregion
    #region Colors
    private Brush backgroundColor;
    private Brush borderColor;

    private Brush titleTextColor;
    private Brush gradingTextColor;
    private Brush gradingLineColor;

    public Brush BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor != value)
            {
                backgroundColor = value;
                viewModel.BackgroundColor = value;
            }
        }
    }

    public Brush BorderColor
    {
        get => borderColor;
        set
        {
            if (borderColor != value)
            {
                borderColor = value;
                viewModel.BorderColor = value;
            }
        }
    }

    public Brush TitleTextColor
    {
        get => titleTextColor;
        set
        {
            if (titleTextColor != value)
            {
                titleTextColor = value;
                viewModel.TitleTextColor = value;
            }
        }
    }

    public Brush GradingTextColor
    {
        get => gradingTextColor;
        set
        {
            if (gradingTextColor != value)
            {
                gradingTextColor = value;
                viewModel.GradingTextColor = value;
            }
        }
    }

    public Brush GradingLineColor
    {
        get => gradingLineColor;
        set
        {
            if (gradingLineColor != value)
            {
                gradingLineColor = value;
                viewModel.GradingLineColor = value;
            }
        }
    }

    private Brush dial1Color;
    private Brush dial2Color;
    private Brush dial3Color;

    public Brush Dial1Color
    {
        get => dial1Color;
        set
        {
            if (dial1Color != value)
            {
                dial1Color = value;
                viewModel.Dial1Color = value;
            }
        }
    }

    public Brush Dial2Color
    {
        get => dial2Color;
        set
        {
            if (dial2Color != value)
            {
                dial2Color = value;
                viewModel.Dial2Color = value;
            }
        }
    }

    public Brush Dial3Color
    {
        get => dial3Color;
        set
        {
            if (dial3Color != value)
            {
                dial3Color = value;
                viewModel.Dial3Color = value;
            }
        }
    }

    private Brush dial1BorderColor;
    private Brush dial2BorderColor;
    private Brush dial3BorderColor;

    public Brush Dial1BorderColor
    {
        get => dial1BorderColor;
        set
        {
            if (dial1BorderColor != value)
            {
                dial1BorderColor = value;
                viewModel.Dial1BorderColor = value;
            }
        }
    }

    public Brush Dial2BorderColor
    {
        get => dial2BorderColor;
        set
        {
            if (dial2BorderColor != value)
            {
                dial2BorderColor = value;
                viewModel.Dial2BorderColor = value;
            }
        }
    }

    public Brush Dial3BorderColor
    {
        get => dial3BorderColor;
        set
        {
            if (dial3BorderColor != value)
            {
                dial3BorderColor = value;
                viewModel.Dial3BorderColor = value;
            }
        }
    }
    #endregion
    #region Dial Values
    private double gaugeMinValue = -1000;
    private double gaugeMaxValue = -1000;

    public double GaugeMinValue
    {
        get => gaugeMinValue;
        set
        {
            if (gaugeMinValue != value)
            {
                gaugeMinValue = value;

                viewModel.Dial1Rotation = GetDialRotation(dial1Value);
                viewModel.Dial2Rotation = GetDialRotation(dial2Value);
                viewModel.Dial3Rotation = GetDialRotation(dial3Value);

                SetGradingTexts();
            }
        }
    }

    public double GaugeMaxValue
    {
        get => gaugeMaxValue;
        set
        {
            if (gaugeMaxValue != value)
            {
                gaugeMaxValue = value;

                viewModel.Dial1Rotation = GetDialRotation(dial1Value);
                viewModel.Dial2Rotation = GetDialRotation(dial2Value);
                viewModel.Dial3Rotation = GetDialRotation(dial3Value);

                SetGradingTexts();
            }
        }
    }

    private double dial1Value = -1000;
    private double dial2Value = -1000;
    private double dial3Value = -1000;

    public double Dial1Value
    {
        get => dial1Value;
        set
        {
            if (dial1Value != value)
            {
                dial1Value = value;
                viewModel.Dial1Rotation = GetDialRotation(dial1Value);
                viewModel.Dial1Text = GetDialText(dial1Value);
            }
        }
    }

    public double Dial2Value
    {
        get => dial2Value;
        set
        {
            if (dial2Value != value)
            {
                dial2Value = value;
                viewModel.Dial2Rotation = GetDialRotation(dial2Value);
                viewModel.Dial2Text = GetDialText(dial2Value);
            }
        }
    }

    public double Dial3Value
    {
        get => dial3Value;
        set
        {
            if (dial3Value != value)
            {
                dial3Value = value;
                viewModel.Dial3Rotation = GetDialRotation(dial3Value);
                viewModel.Dial3Text = GetDialText(dial3Value);
            }
        }
    }

    private double GetDialRotation(double value)
    {
        double valuePerDegree =
            (gaugeMaxValue - gaugeMinValue) / (viewModel.MaxRotation - viewModel.MinRotation);

        double rotationDegrees = viewModel.MinRotation + (value / valuePerDegree);

        if (rotationDegrees < viewModel.MinRotation)
            rotationDegrees = viewModel.MinRotation;

        if (rotationDegrees > viewModel.MaxRotation)
            rotationDegrees = viewModel.MaxRotation;

        return rotationDegrees;
    }

    private void SetGradingTexts()
    {
        double diff = gaugeMaxValue - gaugeMinValue;
        double perGradeValue = diff / (5 - 1);

        viewModel.Grading1Text = $"{gaugeMinValue: 0.00}";
        viewModel.Grading2Text = $"{gaugeMinValue + perGradeValue: 0.00}";
        viewModel.Grading3Text = $"{diff / 2: 0.00}";
        viewModel.Grading4Text = $"{gaugeMaxValue - perGradeValue: 0.00}";
        viewModel.Grading5Text = $"{gaugeMaxValue: 0.00}";
    }

    private string GetDialText(double value)
    {
        return $"{value: 0.00}";
    }
    #endregion
    #region Title / Text
    private string gaugeTitle = "";

    public string GaugeTitle
    {
        get => gaugeTitle;
        set
        {
            if (gaugeTitle != value)
            {
                gaugeTitle = value;
                viewModel.GaugeTitle = value;
            }
        }
    }
    #endregion
}
