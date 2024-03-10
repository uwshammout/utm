using CronBlocks.UTM.Settings;
using CronBlocks.Helpers.Extensions;
using CronBlocks.SerialPortInterface.Entities;
using CronBlocks.SerialPortInterface.Extensions;
using CronBlocks.SerialPortInterface.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CronBlocks.UTM.Windows;

public partial class DeviceConnectionWindow : Window
{
    private readonly ISerialPortsDiscoveryService _ports;
    private readonly ISerialOptionsService _options;
    private readonly ISerialModbusClientService _modbus;

    private SerialModbusClientSettings _settings;

    private List<FrameworkElement> _allSelectionElements;

    private readonly string _connectButtonInitialText;
    private readonly string _connectButtonConnectedText = "Disconnect";

    private readonly Brush _connectButtonBgBrush;
    private readonly Brush _deviceAddressInputBgBrush;
    private readonly Brush _registersStartAddressInputBgBrush;

    public DeviceConnectionWindow(
        ISerialPortsDiscoveryService serialPortsDiscovery,
        ISerialOptionsService serialOptions,
        ISerialModbusClientService serialModbusClient)
    {
        InitializeComponent();

        _allSelectionElements = new List<FrameworkElement>()
        {
            PortHeading,
            BaudRateHeading,
            DataBitsHeading,
            ParityHeading,
            StopBitsHeading,
            DeviceAddressHeading,
            RegistersStartAddressHeading,
            PortInput,
            BaudRateInput,
            DataBitsInput,
            ParityInput,
            StopBitsInput,
            DeviceAddressInput,
            RegistersStartAddressInput,
        };

        _connectButtonInitialText = (string)ConnectButton.Content;

        _connectButtonBgBrush = ConnectButton.Background;
        _deviceAddressInputBgBrush = DeviceAddressInput.Background;
        _registersStartAddressInputBgBrush = RegistersStartAddressHeading.Background;

        _ports = serialPortsDiscovery;
        _options = serialOptions;
        _modbus = serialModbusClient;

        _settings = _modbus.GetComSettings();

        _modbus.OperationStateChanged += HandleModbusStatus;
        HandleModbusStatus(_modbus.OperationState);
    }

    protected override void OnClosed(EventArgs e)
    {
        _modbus.OperationStateChanged -= HandleModbusStatus;

        _ports.NewPortFound -= OnComPortFound;
        _ports.ExistingPortRemoved -= OnComPortRemoved;
        _ports.StopPortsDiscovery();

        base.OnClosed(e);
    }

    private void HandleModbusStatus(OperationState status)
    {
        Dispatcher.Invoke(() =>
        {
            PortInput.Items.Clear();
            BaudRateInput.Items.Clear();
            DataBitsInput.Items.Clear();
            ParityInput.Items.Clear();
            StopBitsInput.Items.Clear();
            DeviceAddressInput.Text = "";
            RegistersStartAddressInput.Text = "";

            switch (status)
            {
                case OperationState.Running:
                    _ports.NewPortFound -= OnComPortFound;
                    _ports.ExistingPortRemoved -= OnComPortRemoved;
                    _ports.StopPortsDiscovery();

                    EnableSelectionControls(false);

                    _settings = _modbus.GetComSettings();

                    PortInput.Items.Add(_settings.ComPort);
                    BaudRateInput.Items.Add(_settings.BaudRate.ToDisplayString());
                    DataBitsInput.Items.Add(_settings.DataBits.ToDisplayString());
                    ParityInput.Items.Add(_settings.Parity.ToDisplayString());
                    StopBitsInput.Items.Add(_settings.StopBits.ToDisplayString());
                    DeviceAddressInput.Text = _settings.DeviceAddress.ToString();
                    RegistersStartAddressInput.Text = _settings.RegistersStartAddressHexStr;

                    PortInput.SelectedIndex = 0;
                    BaudRateInput.SelectedIndex = 0;
                    DataBitsInput.SelectedIndex = 0;
                    ParityInput.SelectedIndex = 0;
                    StopBitsInput.SelectedIndex = 0;

                    break;

                case OperationState.Stopped:
                    EnableSelectionControls(true);

                    _settings = _modbus.GetComSettings();

                    int index;

                    index = 0;
                    foreach (string option in _options.GetAllOptions<BaudRate>())
                    {
                        BaudRateInput.Items.Add(option);
                        if (option == _settings.BaudRate.ToDisplayString())
                        {
                            BaudRateInput.SelectedIndex = index;
                        }
                        index++;
                    }

                    index = 0;
                    foreach (string option in _options.GetAllOptions<DataBits>())
                    {
                        DataBitsInput.Items.Add(option);
                        if (option == _settings.DataBits.ToDisplayString())
                        {
                            DataBitsInput.SelectedIndex = index;
                        }
                        index++;
                    }

                    index = 0;
                    foreach (string option in _options.GetAllOptions<Parity>())
                    {
                        ParityInput.Items.Add(option);
                        if (option == _settings.Parity.ToDisplayString())
                        {
                            ParityInput.SelectedIndex = index;
                        }
                        index++;
                    }

                    index = 0;
                    foreach (string option in _options.GetAllOptions<StopBits>())
                    {
                        StopBitsInput.Items.Add(option);
                        if (option == _settings.StopBits.ToDisplayString())
                        {
                            StopBitsInput.SelectedIndex = index;
                        }
                        index++;
                    }

                    DeviceAddressInput.Text = _settings.DeviceAddress.ToString();
                    RegistersStartAddressInput.Text = _settings.RegistersStartAddressHexStr;

                    _ports.NewPortFound += OnComPortFound;
                    _ports.ExistingPortRemoved += OnComPortRemoved;
                    _ports.StartPortsDiscovery();

                    break;
            }

            UpdateConnectButton();
        });
    }

    private void EnableSelectionControls(bool enable)
    {
        foreach (var item in _allSelectionElements)
        {
            item.IsEnabled = enable;
        }
    }

    private void OnComPortFound(string port)
    {
        if (_modbus.OperationState == OperationState.Running)
            return;

        Dispatcher.Invoke(() =>
        {
            if (PortInput.Items.Contains(port)) return;

            PortInput.Items.Add(port);

            if (PortInput.Items.Count == 1)
            {
                PortInput.SelectedIndex = 0;
            }
        });
    }

    private void OnComPortRemoved(string port)
    {
        if (_modbus.OperationState == OperationState.Running)
            return;

        Dispatcher.Invoke(() =>
        {
            if (PortInput.Items.Contains(port))
                PortInput.Items.Remove(port);
        });
    }

    private void OnInputSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string name = ((ComboBox)sender).Name;

        if (((ComboBox)sender).SelectedItem != null)
        {
            switch (name)
            {
                case "PortInput":
                    _settings.ComPort = (string)PortInput.SelectedItem;
                    break;

                case "BaudRateInput":
                    _settings.BaudRate = ((string)BaudRateInput.SelectedItem).FromDisplayString<BaudRate>();
                    break;

                case "DataBitsInput":
                    _settings.DataBits = ((string)DataBitsInput.SelectedItem).FromDisplayString<DataBits>();
                    break;

                case "ParityInput":
                    _settings.Parity = ((string)ParityInput.SelectedItem).FromDisplayString<Parity>();
                    break;

                case "StopBitsInput":
                    _settings.StopBits = ((string)StopBitsInput.SelectedItem).FromDisplayString<StopBits>();
                    break;

                default:
                    throw new NotImplementedException(
                    $"ComboBox-'{name}' - {nameof(OnInputSelectionChanged)} is not implemented");
            }

            UpdateConnectButton();
        }
    }

    private void OnInputTextChanged(object sender, TextChangedEventArgs e)
    {
        string name = ((TextBox)sender).Name;

        switch (name)
        {
            case "DeviceAddressInput":
                if (DeviceAddressInput.Text.IsValidDeviceAddress())
                {
                    DeviceAddressInput.Background = _deviceAddressInputBgBrush;
                    _settings.DeviceAddress = int.Parse(DeviceAddressInput.Text);
                }
                else
                {
                    DeviceAddressInput.Background = DisplayColors.ErrorBg;
                }
                break;

            case "RegistersStartAddressInput":
                if (RegistersStartAddressInput.Text.IsValidHex())
                {
                    RegistersStartAddressInput.Background = _registersStartAddressInputBgBrush;
                    _settings.RegistersStartAddressHexStr = RegistersStartAddressInput.Text;
                }
                else
                {
                    RegistersStartAddressInput.Background = DisplayColors.ErrorBg;
                }
                break;

            default:
                throw new NotImplementedException(
                $"TextBox-'{name}' - {nameof(OnInputTextChanged)} is not implemented");
        }

        UpdateConnectButton();
    }

    private void UpdateConnectButton()
    {
        switch (_modbus.OperationState)
        {
            case OperationState.Running:
                ConnectButton.Content = _connectButtonConnectedText;
                ConnectButton.Background = DisplayColors.DisconnectButtonBg;
                break;

            case OperationState.Stopped:
                ConnectButton.Content = _connectButtonInitialText;
                ConnectButton.Background = _connectButtonBgBrush;
                break;
        }

        if (PortInput.SelectedItem != null &&
            DeviceAddressInput.Text.IsValidDeviceAddress() &&
            RegistersStartAddressInput.Text.IsValidHex())
        {
            ConnectButton.IsEnabled = true;
        }
        else
        {
            ConnectButton.IsEnabled = false;
        }
    }

    private void ConnectButtonClicked(object sender, RoutedEventArgs e)
    {
        if ((string)ConnectButton.Content == _connectButtonInitialText)
        {
            //- Connect
            if (_settings.IsValid())
            {
                _modbus.SetComSettings(_settings);
                _modbus.StartAcquisition();
            }
            else
            {
                throw new InvalidOperationException(
                    $"{_settings} object is invalid to start connection with the device");
            }
        }
        else if ((string)ConnectButton.Content == _connectButtonConnectedText)
        {
            //- Disconnect
            _modbus.StopAcquisition();
        }
        else
        {
            throw new NotImplementedException(
                $"Handling of {nameof(ConnectButtonClicked)} is not implemented" +
                $" when its used for '{(string)ConnectButton.Content}' purpose");
        }
    }
}
