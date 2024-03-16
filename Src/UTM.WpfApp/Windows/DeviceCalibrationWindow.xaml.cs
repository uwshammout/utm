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
    private readonly TextBox[] _displacementCalInputs, _displacementCalOutputs;
    private readonly TextBox[] _loadCalInputs, _loadCalOutputs;

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

        _displacementCalInputs =
            [
            DisplacementCalibrationInput01, DisplacementCalibrationInput02, DisplacementCalibrationInput03, DisplacementCalibrationInput04,
            DisplacementCalibrationInput05, DisplacementCalibrationInput06, DisplacementCalibrationInput07, DisplacementCalibrationInput08,
            DisplacementCalibrationInput09, DisplacementCalibrationInput10, DisplacementCalibrationInput11, DisplacementCalibrationInput12,
            DisplacementCalibrationInput13, DisplacementCalibrationInput14, DisplacementCalibrationInput15, DisplacementCalibrationInput16,
            DisplacementCalibrationInput17, DisplacementCalibrationInput18, DisplacementCalibrationInput19, DisplacementCalibrationInput20,
            ];
        _displacementCalOutputs =
            [
            DisplacementCalibrationOutput01, DisplacementCalibrationOutput02, DisplacementCalibrationOutput03, DisplacementCalibrationOutput04,
            DisplacementCalibrationOutput05, DisplacementCalibrationOutput06, DisplacementCalibrationOutput07, DisplacementCalibrationOutput08,
            DisplacementCalibrationOutput09, DisplacementCalibrationOutput10, DisplacementCalibrationOutput11, DisplacementCalibrationOutput12,
            DisplacementCalibrationOutput13, DisplacementCalibrationOutput14, DisplacementCalibrationOutput15, DisplacementCalibrationOutput16,
            DisplacementCalibrationOutput17, DisplacementCalibrationOutput18, DisplacementCalibrationOutput19, DisplacementCalibrationOutput20,
            ];

        _loadCalInputs =
            [
            LoadCalibrationInput01, LoadCalibrationInput02, LoadCalibrationInput03, LoadCalibrationInput04,
            LoadCalibrationInput05, LoadCalibrationInput06, LoadCalibrationInput07, LoadCalibrationInput08,
            LoadCalibrationInput09, LoadCalibrationInput10, LoadCalibrationInput11, LoadCalibrationInput12,
            LoadCalibrationInput13, LoadCalibrationInput14, LoadCalibrationInput15, LoadCalibrationInput16,
            LoadCalibrationInput17, LoadCalibrationInput18, LoadCalibrationInput19, LoadCalibrationInput20,
            ];
        _loadCalOutputs =
            [
            LoadCalibrationOutput01, LoadCalibrationOutput02, LoadCalibrationOutput03, LoadCalibrationOutput04,
            LoadCalibrationOutput05, LoadCalibrationOutput06, LoadCalibrationOutput07, LoadCalibrationOutput08,
            LoadCalibrationOutput09, LoadCalibrationOutput10, LoadCalibrationOutput11, LoadCalibrationOutput12,
            LoadCalibrationOutput13, LoadCalibrationOutput14, LoadCalibrationOutput15, LoadCalibrationOutput16,
            LoadCalibrationOutput17, LoadCalibrationOutput18, LoadCalibrationOutput19, LoadCalibrationOutput20,
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

        SetDisplacementCalibrationData(_modbusCalibration.GetCalibrationValues(0));
        SetLoadCalibrationData(_modbusCalibration.GetCalibrationValues(1));

        _modbus.OperationStateChanged += OnDeviceOperationStateChanged;
        OnDeviceOperationStateChanged(_modbus.OperationState);

        _modbusScaling.NewValuesReceived += OnNewScaledValuesReceived;
        _modbusCalibration.NewValuesReceived += OnNewCalibratedValuesReceived;
    }

    private void OnNewScaledValuesReceived(List<double> list)
    {
        Dispatcher.Invoke(() =>
        {
            if (list.Count >= 2)
            {
                DisplacementScaledValue.Value = list[0];
                LoadScaledValue.Value = list[1];
            }

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
            if (list.Count >= 2)
            {
                DisplacementCalibratedValue.Value = list[0];
                LoadCalibratedValue.Value = list[1];
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
    private void SetDisplacementCalibrationData(ImmutableDictionary<double, double> calData)
    {
        foreach (TextBox t in _displacementCalInputs)
        {
            t.Text = "";
        }
        foreach (TextBox t in _displacementCalOutputs)
        {
            t.Text = "";
        }

        int index = 0;
        List<double> ks = calData.Keys.ToList();
        ks.Sort();
        foreach (double k in ks)
        {
            _displacementCalInputs[index].Text = k.ToString();
            _displacementCalOutputs[index].Text = calData[k].ToString();
            index++;
        }
    }
    private void SetLoadCalibrationData(ImmutableDictionary<double, double> calData)
    {
        foreach (TextBox t in _loadCalInputs)
        {
            t.Text = "";
        }
        foreach (TextBox t in _loadCalOutputs)
        {
            t.Text = "";
        }

        int index = 0;
        List<double> ks = calData.Keys.ToList();
        ks.Sort();
        foreach (double k in ks)
        {
            _loadCalInputs[index].Text = k.ToString();
            _loadCalOutputs[index].Text = calData[k].ToString();
            index++;
        }
    }

    private ImmutableDictionary<double, double> ReadDisplacementCalibrationData()
    {
        Dictionary<double, double> dict = new();

        for(int i = 0; i < _displacementCalInputs.Length; i++)
        {
            if (!string.IsNullOrEmpty(_displacementCalInputs[i].Text) &&
                !string.IsNullOrEmpty(_displacementCalOutputs[i].Text) &&
                double.TryParse(_displacementCalInputs[i].Text, out double input) &&
                double.TryParse(_displacementCalOutputs[i].Text, out double output))
            {
                dict.Add(input, output);
            }
        }

        return dict.ToImmutableDictionary();
    }
    private ImmutableDictionary<double, double>  ReadLoadCalibrationData()
    {
        Dictionary<double, double> dict = new();

        for(int i = 0; i < _loadCalInputs.Length; i++)
        {
            if (!string.IsNullOrEmpty(_loadCalInputs[i].Text) &&
                !string.IsNullOrEmpty(_loadCalOutputs[i].Text) &&
                double.TryParse(_loadCalInputs[i].Text, out double input) &&
                double.TryParse(_loadCalOutputs[i].Text, out double output))
            {
                dict.Add(input, output);
            }
        }

        return dict.ToImmutableDictionary();
    }
    private void CalibrationUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender == DisplacementCalibrationUpdateButton)
        {
            SetDisplacementCalibrationData(
                _modbusCalibration.SetCalibrationValues(0, ReadDisplacementCalibrationData()));
        }
        else if (sender == LoadCalibrationUpdateButton)
        {
            SetLoadCalibrationData(
                _modbusCalibration.SetCalibrationValues(1, ReadLoadCalibrationData()));
        }
    }
    #endregion
}
