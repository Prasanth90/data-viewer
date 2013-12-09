using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Atmel.Studio.Services;
using C1.WPF.C1Chart;
using Fdk.Ui.ViewModelUtils;

namespace Company.DataViewer.ViewModel
{
    public class GraphViewModel : ViewModelBase<GraphViewModel>
    {
        public IDataBreakpoint BreakPoint { get; set; }
        private readonly Action<GraphViewModel> _removeAction;

        private bool _pauseButtonEnable;
        private bool _runButtonEnable;
        private string _selectedGraphType;
        private ObservableCollection<string> _addresses;
        private string _selectedAddress;
        private bool _isControlEnabled;
        private ChartData _chartData = new ChartData();
        private ChartType _lineSymbols;
        private ChartView _chartView = new ChartView();
        public GraphViewModel( IDataBreakpoint breakPoint, Action<GraphViewModel> removeAction)
        {        
            BreakPoint = breakPoint;
            IsBound = (BreakPoint.State == DataBreakpointState.Bound);
            UpdateBits();
            _removeAction = removeAction;
            PauseButtonEnable = false;
            RunButtonEnable = true;
            GraphTypes = new ObservableCollection<string>() {"Linear", "Digital"};
            Addresses = GetAddresses();
            PauseButtonClick = new RelayCommand<EventArgs>(PauseButtonClickHandler);
            RunButtonClick = new RelayCommand<EventArgs>(RunButtonClickHandler);
            CloseButtonClick = new RelayCommand<EventArgs>(CloseButtonClickHandler);
            Results = new ObservableCollection<Result>();
            LinearDataSeries = GetLineardataSeries();
            DigitalDataSeries = GetDigitalDataSeries();
            SelectedAddress = Addresses.FirstOrDefault();
            SelectedGraphType = GraphTypes.FirstOrDefault();
            Settings.GraphSettings.OptionsChanged += new EventHandler(settings_OptionsChanged);
        }

        public void UpdateBits()
        {
             Bits = new ObservableCollection<BitData>();
            for (int i = 0; i < 8; i++)
            {
                Bits.Add(new BitData(Changed)
                {
                    BitName = "Bit " + i.ToString()
                });
            }
        }

        private void Changed()
        {
            DigitalDataSeries = GetDigitalDataSeries();
            SelectedGraphType = _selectedGraphType;
        }

        private void settings_OptionsChanged(object sender, EventArgs e)
        {
            if (SelectedGraphType != null && Addresses != null && Results != null)
            {
                LinearDataSeries = GetLineardataSeries();
                DigitalDataSeries = GetDigitalDataSeries();
                SelectedGraphType = _selectedGraphType;
            }
        }

        private ObservableCollection<string> GetAddresses()
        {
            var addresses = new ObservableCollection<string>();
            if (!string.IsNullOrEmpty(BreakPoint.Address))
            {
                var address = BreakPoint.Address.Replace("0x", "");
                ulong startAddress = ulong.Parse(address, NumberStyles.HexNumber);
                addresses.Add(startAddress.ToString());
                for (int i = 1; i < BreakPoint.Config.ByteCount; i++)
                {
                    addresses.Add(startAddress.ToString());
                    startAddress = startAddress + 1;
                }
            }
            return addresses;
        }

        public bool IsBound
        {
            get { return _isBound; }
            set { _isBound = value; }
        }

        private ObservableCollection<XYDataSeries> GetDigitalDataSeries()
        {
            var digitalDataSeriesSet = new ObservableCollection<XYDataSeries>();
            for (int i = 0; i <= 7;i++ )
            {
                if (Bits[i].IsChecked)
                {
                    var dataseries = new XYDataSeries()
                        {
                            SymbolSize = new Size(6, 6),
                            SymbolFill = SymbolFill,
                            SymbolMarker = SymbolMarker,
                            ConnectionFill = ConnectionFill,
                            RenderMode = RenderMode.Default,
                            Label = string.Format("Bit {0}", i.ToString()),
                            ItemsSource = Results,
                            XValueBinding = new Binding()
                                {
                                    Path =
                                        new PropertyPath(string.Format("Bit{0}[{1}].Key", i.ToString(),
                                                                       SelectedAddressIndex)),
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                },
                            ValueBinding = new Binding()
                                {
                                    Path =
                                        new PropertyPath(string.Format("Bit{0}[{1}].Value", i.ToString(),
                                                                       SelectedAddressIndex)),
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                },
                        };
                    dataseries.PlotElementLoaded += new EventHandler(DataSeries_OnPlotElementLoaded);

                    digitalDataSeriesSet.Add(dataseries);
                }
            }

            return digitalDataSeriesSet;
        }

        public XYDataSeries LinearDataSeries { get; set; }

        public ObservableCollection<XYDataSeries> DigitalDataSeries { get; set; }

        public ChartData ChartData
        {
            get { return _chartData; }
            set
            {
                _chartData = value;
                this.OnPropertyChanged("ChartData");
            }
        }


        public ChartView ChartView
        {
            get { return _chartView; }
            set
            {
                _chartView = value;
             
                this.OnPropertyChanged("ChartView");
            }
        }

        private XYDataSeries GetLineardataSeries()
        {
            var dataseries = new XYDataSeries()
                {
                   SymbolSize =  new Size(6,6),
                    SymbolFill = SymbolFill,
                    SymbolMarker = SymbolMarker,
                    ConnectionFill = ConnectionFill,
                    RenderMode = RenderMode.Default,
                    Label = "Plot",
                    
                    ItemsSource =  Results,
                    XValueBinding = new Binding()
                        {
                            Path = new PropertyPath("KeyValuePair.Key"),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        },
                    ValueBinding = new Binding()
                        {
                            Path = new PropertyPath("KeyValuePair.Value"),
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        }
                };
            dataseries.PlotElementLoaded+=new EventHandler(DataSeries_OnPlotElementLoaded);
            return dataseries;
        }

        public bool IsControlEnabled
        {
            get { return _isControlEnabled; }
            set
            {
                _isControlEnabled = value;
                this.OnPropertyChanged("IsControlEnabled");
            }
        }

        public ObservableCollection<string> GraphTypes { get; set; }

        public ChartType LineSymbols
        {
            get { return _lineSymbols; }
            set
            {
                _lineSymbols = value;
                this.OnPropertyChanged("LineSymbols");
            }
        }

        public string SelectedGraphType
        {
            get { return _selectedGraphType; }
            set
            {
                _selectedGraphType = value;
                if (_selectedGraphType.Equals("Digital"))
                {
                    ChartData.Children.Clear();
                    foreach (var digitalDataSeries in DigitalDataSeries)
                    {
                        ChartData.Children.Add(digitalDataSeries);    
                    }
                    IsControlEnabled = true;
                    LineSymbols = ChartType.Step;
                    ChartView.AxisY.MajorUnit = 10000.0;
                }
                else
                {
                    ChartData.Children.Clear();
                    ChartData.Children.Add(LinearDataSeries);
                    IsControlEnabled = false;
                    LineSymbols = ChartType.LineSymbols;
                    ChartView.AxisY.MajorUnit = double.NaN;
                }
                this.OnPropertyChanged("SelectedGraphType");
            }
        }

        public int SelectedAddressIndex
        {
            get { return _selectedAddressIndex; }
            set
            {
                _selectedAddressIndex = value;
                this.OnPropertyChanged("SelectedAddressIndex");
                DigitalDataSeries = GetDigitalDataSeries();
                SelectedGraphType = _selectedGraphType;
            }
        }

        public ObservableCollection<string> Addresses
        {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public string SelectedAddress
        {
            get { return _selectedAddress; }
            set
            {
                _selectedAddress = value;
                this.OnPropertyChanged("SelectedAddress");
            }
        }

        private ObservableCollection<BitData> _bits;

        public ObservableCollection<BitData> Bits
        {
            get { return _bits; }
            set
            {
                _bits = value;
                this.OnPropertyChanged("Bits");
            }
        }

        public Brush SymbolFill
        {
            get { return Settings.GraphSettings.MarkerColour; }
        }

        public Brush ConnectionFill
        {
            get { return Settings.GraphSettings.LineColour; }
        }

        public Marker SymbolMarker
        {
            get { return Settings.GraphSettings.MarkerSymbol; }
        }

        public event ClickEventHandler PauseClicked;

        public delegate void ClickEventHandler(object o, GraphEventArgs e); 

        protected virtual void OnPauseClicked()
        {
            ClickEventHandler handler = PauseClicked;
            if (handler != null) handler(this, new GraphEventArgs(this));
        }

        public event ClickEventHandler RunClicked;

        protected virtual void OnRunClicked()
        {
            ClickEventHandler handler = RunClicked;
            if (handler != null) handler(this, new GraphEventArgs(this));
        }


        public ObservableCollection<Result> Results { get; set; }

        private void CloseButtonClickHandler(EventArgs obj)
        {
            _removeAction(this);
        }

        private void RunButtonClickHandler(EventArgs obj)
        {
            PauseButtonEnable = true;
            RunButtonEnable = false;
            OnRunClicked();
        }

        private void PauseButtonClickHandler(EventArgs obj)
        {

            PauseButtonEnable = false;
            RunButtonEnable = true;
            OnPauseClicked();
        }

        public ICommand PauseButtonClick { get; set; }
        public ICommand RunButtonClick { get; set; }
        public ICommand CloseButtonClick { get; set; }
        

        public bool PauseButtonEnable
        {
            get { return _pauseButtonEnable; }
            set
            {
                _pauseButtonEnable = value;
                this.OnPropertyChanged("PauseButtonEnable");
            }
        }

        public bool RunButtonEnable
        {
            get { return _runButtonEnable; }
            set
            {
                _runButtonEnable = value;
                this.OnPropertyChanged("RunButtonEnable");
            }
        }

        public void AddPoint(double value,Dictionary<ulong,byte> digitalVaues )
        {
            if (Results.Count <= Settings.GraphSettings.MaxXValue - 1)
            {
                Results.Add(new Result(new KeyValuePair<int, double>(Results.Count() + 1, value),digitalVaues));
            }
            else
            {
                Shift(value,digitalVaues);
            }
        }

        private void Shift(double val, Dictionary<ulong, byte> digitalVaues)
        {
            List<Result> skippedResults = Results.Skip(1).ToList();
            Results.Clear();
            foreach (Result skippedResult in skippedResults)
            {
                Results.Add(new Result(new KeyValuePair<int, double>(Results.Count +1,skippedResult.KeyValuePair.Value), skippedResult.DigitalDictionary));
            }
            Results.Add(new Result(new KeyValuePair<int, double>(Results.Count() + 1, val), digitalVaues));
        }


         private void DataSeries_OnPlotElementLoaded(object sender, EventArgs e)
        {
            var plotelemnt = sender as PlotElement;
            if (plotelemnt != null)
            {
                plotelemnt.MouseEnter += new MouseEventHandler(plotelemnt_MouseMove);
                plotelemnt.MouseLeave += new MouseEventHandler(plotelemnt_MouseLeave);
            }
        }

        void plotelemnt_MouseLeave(object sender, MouseEventArgs e)
        {
            Hide_PopupToolTip(sender,e);
        }

        void plotelemnt_MouseMove(object sender, MouseEventArgs e)
        {
            var plotelement = sender as PlotElement;
            if (plotelement != null)
            {
                var pointindex = plotelement.DataPoint.PointIndex;
                if (SelectedGraphType.Equals("Digital"))
                {
                    if (pointindex != -1)
                    {
                        var bitName = (int) plotelement.DataPoint.Value/10;
                        bool bitState = (int)plotelement.DataPoint.Value %10 > 0;
                        SetToolTipText(string.Format("{0},[Bit {1} , {2}]",(plotelement.DataPoint.PointIndex + 1).ToString(), bitName.ToString(),bitState.ToString()));
                        Show_PopupToolTip(sender, e);
                    }
                }
                else
                {
                    if (pointindex != -1)
                    {
                        SetToolTipText(string.Format("{0},{1}", (plotelement.DataPoint.PointIndex + 1).ToString(),
                                                     plotelement.DataPoint.Value.ToString()));
                        Show_PopupToolTip(sender, e);
                    }
                }
            }
        }

        private void Show_PopupToolTip(object sender, MouseEventArgs e)
        {
            ToolTipControl.IsOpen = true;
        }
        private void Hide_PopupToolTip(object sender, MouseEventArgs e)
        {
            ToolTipControl.IsOpen = false;
        }

        private void SetToolTipText(string message)
        {
            var text = ToolTipControl.Child as TextBox;
            text.Text = message;
        }

        private Popup _toolTip;
        private int _selectedAddressIndex;
        private bool _isBound;

        private Popup ToolTipControl
        {
            get
            {
                if (_toolTip == null)
                {
                    TextBox popupText = new TextBox
                    {
                        Text = "Popup Text",
                        Height = 25,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Background = Brushes.LightGray,
                        Foreground = Brushes.Black
                    };
                    _toolTip = new Popup
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Placement = PlacementMode.Mouse,
                        Child = popupText
                    };

                }
                return _toolTip;
            }
            set { _toolTip = value; }
        }
    
    }

    public class GraphEventArgs : EventArgs
    {
        public GraphViewModel GraphViewModel;

        public GraphEventArgs(GraphViewModel graphViewModel)
        {
            GraphViewModel = graphViewModel;
        }
    }

    public class Result 
    {
        public Result(KeyValuePair<int, double> keyValuePair, Dictionary<ulong,byte> digitalDictionary)
        {
            KeyValuePair = keyValuePair;
            DigitalDictionary = digitalDictionary;
            var serialNo = keyValuePair.Key;
            Bit0 = new List<KeyValuePair<int, double>>();
            Bit1 = new List<KeyValuePair<int, double>>();
            Bit2 = new List<KeyValuePair<int, double>>();
            Bit3 = new List<KeyValuePair<int, double>>();
            Bit4 = new List<KeyValuePair<int, double>>();
            Bit5 = new List<KeyValuePair<int, double>>();
            Bit6 = new List<KeyValuePair<int, double>>();
            Bit7 = new List<KeyValuePair<int, double>>();
            foreach (KeyValuePair<ulong, byte> valuePair in digitalDictionary)
            {
                byte value = valuePair.Value;
                Bit0.Add(new KeyValuePair<int, double>(serialNo, 0 + (GetBit(value,  1) ? 5 : 0)));
                Bit1.Add(new KeyValuePair<int, double>(serialNo, 10 + (GetBit(value, 2) ? 5 : 0)));
                Bit2.Add(new KeyValuePair<int, double>(serialNo, 20 + (GetBit(value, 3) ? 5 : 0)));
                Bit3.Add(new KeyValuePair<int, double>(serialNo, 30 + (GetBit(value, 4) ? 5 : 0)));
                Bit4.Add(new KeyValuePair<int, double>(serialNo, 40 + (GetBit(value, 5) ? 5 : 0)));
                Bit5.Add(new KeyValuePair<int, double>(serialNo, 50 + (GetBit(value, 6) ? 5 : 0)));
                Bit6.Add(new KeyValuePair<int, double>(serialNo, 60 + (GetBit(value, 7) ? 5 : 0)));
                Bit7.Add(new KeyValuePair<int, double>(serialNo, 70 + (GetBit(value, 8) ? 5 : 0)));
            }
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            var bit = (b & (1 << bitNumber - 1)) != 0;
            return bit;
        }

        public Dictionary<ulong, byte> DigitalDictionary { get; set; }

        public KeyValuePair<int, double> KeyValuePair
        {
            get;
            set;
        }
        
        public List<KeyValuePair<int, double>> Bit0
        {
            get;
            set;
        }

        public List<KeyValuePair<int, double>> Bit1
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit2
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit3
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit4
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit5
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit6
        {
            get;
            set;
        }
        public List<KeyValuePair<int, double>> Bit7
        {
            get;
            set;
        }
    }
}
