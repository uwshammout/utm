using CronBlocks.Helpers;
using CronBlocks.Helpers.Extensions;
using CronBlocks.SerialPortInterface.Entities;
using CronBlocks.SerialPortInterface.Interfaces;
using CronBlocks.UTM.InternalServices;
using CronBlocks.UTM.Settings;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CronBlocks.UTM.Windows;

public partial class DeviceCalibrationWindow : Window
{
    private readonly string _passwordKey = "Admin/PWD";

    private readonly ISerialModbusClientService _modbus;
    private readonly ISerialModbusDataScalingService _modbusScaling;
    private readonly ISerialModbusDataCalibrationService _modbusCalibration;
    private readonly DataExchangeService _dataExchange;
    private readonly IniConfigIO _passwordFile;
    private readonly TextBox[] _mfInputs;
    private readonly TextBox[] _offInputs;
    private readonly TextBlock[] _scaledOutputs;
    private readonly TextBox[] _distanceCalInputs, _distanceCalOutputs;

    private readonly Brush _originalTextBoxBg;

    public DeviceCalibrationWindow(
            ISerialModbusClientService modbus,
            ISerialModbusDataScalingService modbusScaling,
            ISerialModbusDataCalibrationService calibration,
            DataExchangeService dataExchange,
            IniConfigIO passwordFile)
    {
        InitializeComponent();

        _modbus = modbus;
        _modbusScaling = modbusScaling;
        _modbusCalibration = calibration;
        _dataExchange = dataExchange;
        _passwordFile = passwordFile;
        _originalTextBoxBg = MultiplicationFactorA1.Background;

        _mfInputs = 
            [
            MultiplicationFactorA1,   MultiplicationFactorA2,   MultiplicationFactorA3,   MultiplicationFactorA4,
            MultiplicationFactorA5,   MultiplicationFactorA6,   MultiplicationFactorA7,   MultiplicationFactorA8,
            MultiplicationFactorA9,   MultiplicationFactorA10,  MultiplicationFactorA11,  MultiplicationFactorA12,
            MultiplicationFactorA13,  MultiplicationFactorA14,  MultiplicationFactorA15,  MultiplicationFactorA16
            ];
        _offInputs =
            [
            OffsetA1,   OffsetA2,   OffsetA3,   OffsetA4,  OffsetA5,   OffsetA6,   OffsetA7,   OffsetA8,
            OffsetA9,   OffsetA10,  OffsetA11,  OffsetA12, OffsetA13,  OffsetA14,  OffsetA15,  OffsetA16
            ];
        _scaledOutputs =
            [
            ProcessedValueA1,   ProcessedValueA2,   ProcessedValueA3,   ProcessedValueA4,
            ProcessedValueA5,   ProcessedValueA6,   ProcessedValueA7,   ProcessedValueA8,
            ProcessedValueA9,   ProcessedValueA10,  ProcessedValueA11,  ProcessedValueA12,
            ProcessedValueA13,  ProcessedValueA14,  ProcessedValueA15,  ProcessedValueA16
            ];

        _distanceCalInputs =
            [
            DistanceCalibrationInput01, DistanceCalibrationInput02, DistanceCalibrationInput03, DistanceCalibrationInput04,
            DistanceCalibrationInput05, DistanceCalibrationInput06, DistanceCalibrationInput07, DistanceCalibrationInput08,
            DistanceCalibrationInput09, DistanceCalibrationInput10, DistanceCalibrationInput11, DistanceCalibrationInput12,
            DistanceCalibrationInput13, DistanceCalibrationInput14, DistanceCalibrationInput15, DistanceCalibrationInput16,
            DistanceCalibrationInput17, DistanceCalibrationInput18, DistanceCalibrationInput19, DistanceCalibrationInput20,
            ];
        _distanceCalOutputs =
            [
            DistanceCalibrationOutput01, DistanceCalibrationOutput02, DistanceCalibrationOutput03, DistanceCalibrationOutput04,
            DistanceCalibrationOutput05, DistanceCalibrationOutput06, DistanceCalibrationOutput07, DistanceCalibrationOutput08,
            DistanceCalibrationOutput09, DistanceCalibrationOutput10, DistanceCalibrationOutput11, DistanceCalibrationOutput12,
            DistanceCalibrationOutput13, DistanceCalibrationOutput14, DistanceCalibrationOutput15, DistanceCalibrationOutput16,
            DistanceCalibrationOutput17, DistanceCalibrationOutput18, DistanceCalibrationOutput19, DistanceCalibrationOutput20,
            ];

        ImmutableList<double> mfs = _modbusScaling.GetMultiplicationFactors();
        for (int i = 0; i < Math.Min(_mfInputs.Length, mfs.Count); i++)
        {
            _mfInputs[i].Text = mfs[i].ToString();
        }

        ImmutableList<double> ofs = _modbusScaling.GetOffsets();
        for (int i = 0; i < Math.Min(_offInputs.Length, ofs.Count); i++)
        {
            _offInputs[i].Text = ofs[i].ToString();
        }

        SetDistanceCalibrationData(_modbusCalibration.GetCalibrationValues(0));

        _modbus.OperationStateChanged += OnDeviceOperationStateChanged;
        OnDeviceOperationStateChanged(_modbus.OperationState);

        _modbusScaling.NewValuesReceived += OnNewScaledValuesReceived;
        _modbusCalibration.NewValuesReceived += OnNewCalibratedValuesReceived;
    }

    private void OnNewScaledValuesReceived(List<double> list)
    {
        Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < Math.Min(list.Count, _scaledOutputs.Length); i++)
            {
                _scaledOutputs[i].Text = list[i].ToString("0.000");
            }
        });
    }
    private void OnNewCalibratedValuesReceived(List<double> list)
    {
        Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < Math.Min(list.Count, _scaledOutputs.Length); i++)
            {
            }
        });
    }

    private void OnDeviceOperationStateChanged(OperationState state)
    {
        Dispatcher.Invoke(() =>
        {
            switch (state)
            {
                case OperationState.Running:
                    StatusMessage.Text =
                                    "Device connected -" +
                                    " Dynamic updates available";
                    break;

                case OperationState.Stopped:
                    StatusMessage.Text =
                                    "Device not connected -" +
                                    " dynamic updates not available";

                    break;
            }
        });
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        string name = textBox.Name;

        int index = -1;
        string prefix = null!;

        string mfPrefix = "MultiplicationFactorA";
        string offPrefix = "OffsetA";

        //- Let's match prefix

        if (name.StartsWith(mfPrefix))
        {
            prefix = mfPrefix;
        }
        else if (name.StartsWith(offPrefix))
        {
            prefix = offPrefix;
        }
        else
        {
            throw new NotImplementedException(
                    $"Implementation missing from {nameof(OnTextChanged)} for" +
                    $" handling {name}");
        }

        //- Let's get the index for series of inputs only

        if (prefix == mfPrefix || prefix == offPrefix)
        {
            if (int.TryParse(name.Substring(prefix.Length), out index) &&
                    index >= 1 && index <= 16)
            {
                index -= 1;
            }
            else
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        //- Check the value

        if (double.TryParse(textBox.Text, out double value))
        {
            textBox.Background = _originalTextBoxBg;

            if (prefix == mfPrefix)
            {
                _modbusScaling.SetMultiplicationFactor(index, value);
            }
            else if (prefix == offPrefix)
            {
                _modbusScaling.SetOffset(index, value);
            }
        }
        else
        {
            textBox.Background = DisplayColors.ErrorBg;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _modbusScaling.SaveValuesToFile();
        _passwordFile.SaveFile();

        base.OnClosed(e);
    }

    #region Password
    private string GetPasswordHash()
    {
        return _passwordFile.GetString(_passwordKey, ValueConstants.DefaultPasswordHash);
    }
    private void SetPasswordHash(string newHash)
    {
        _passwordFile.SetString(_passwordKey, newHash);
        _passwordFile.SaveFile();
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PasswordInput.Password))
        {
            string password = PasswordInput.Password;

            if (password.Hash() == GetPasswordHash())
            {
                PasswordInputPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                PasswordInputValidityMessage.Visibility = Visibility.Visible;
            }
        }
        else
        {
            PasswordInputValidityMessage.Visibility = Visibility.Hidden;
        }
    }

    private void PasswordNew_PasswordChanged(object sender, RoutedEventArgs e)
    {
        string p1 = PasswordNew.Password;
        string p2 = PasswordNewRepeat.Password;

        if (!string.IsNullOrEmpty(p1) && !string.IsNullOrEmpty(p2))
        {
            if (p1 == p2)
            {
                PasswordNewValidationMessage.Text = "";
                PasswordNewUpdateButton.IsEnabled = true;
            }
            else
            {
                PasswordNewValidationMessage.Text = "Passwords do not match";
                PasswordNewUpdateButton.IsEnabled = false;
            }
        }
        else
        {
            PasswordNewValidationMessage.Text = "Provide matching password in both the fields";
            PasswordNewUpdateButton.IsEnabled = false;
        }
    }

    private void PasswordNewUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        string p1 = PasswordNew.Password;
        string p2 = PasswordNewRepeat.Password;

        if (!string.IsNullOrEmpty(p1) && !string.IsNullOrEmpty(p2) && p1 == p2)
        {
            PasswordNew.Password = "";
            PasswordNewRepeat.Password = "";
            PasswordNewValidationMessage.Text = "";
            PasswordNewUpdateButton.IsEnabled = false;

            SetPasswordHash(p1.Hash());

            MessageBox.Show("Password updated", "Success", MessageBoxButton.OK);
        }
        else
        {
            MessageBox.Show("Invalid operation", "Password not provided", MessageBoxButton.OK);
        }
    }
    #endregion
    #region Calibration data handling
    private void SetDistanceCalibrationData(ImmutableDictionary<double, double> calData)
    {
        foreach (TextBox t in _distanceCalInputs)
        {
            t.Text = "";
        }
        foreach (TextBox t in _distanceCalOutputs)
        {
            t.Text = "";
        }

        int index = 0;
        foreach (KeyValuePair<double, double> item in calData)
        {
            _distanceCalInputs[index].Text = item.Key.ToString();
            _distanceCalOutputs[index].Text = item.Value.ToString();
            index++;
        }
    }
    private ImmutableDictionary<double, double>  ReadDistanceCalibrationData()
    {
        Dictionary<double, double> dict = new();

        for(int i = 0; i < _distanceCalInputs.Length; i++)
        {
            if (!string.IsNullOrEmpty(_distanceCalInputs[i].Text) &&
                !string.IsNullOrEmpty(_distanceCalOutputs[i].Text) &&
                double.TryParse(_distanceCalInputs[i].Text, out double input) &&
                double.TryParse(_distanceCalOutputs[i].Text, out double output))
            {
                dict.Add(input, output);
            }
        }

        return dict.ToImmutableDictionary();
    }
    private void CalibrationUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender == DistanceCalibrationUpdateButton)
        {
            SetDistanceCalibrationData(
                _modbusCalibration.SetCalibrationValues(0, ReadDistanceCalibrationData()));
        }
    }
    #endregion
}
