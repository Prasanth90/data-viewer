﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private Brush _symbolFill;
        private Marker _symbolMarker;
        private Brush _connectionFill;
        private string _selectedGraphType;
        private ObservableCollection<string> _addresses;
        private string _selectedAddress;
        private ObservableCollection<string> _bits = new ObservableCollection<string>(){"0","1","2","3","4","5","6","7"};
        private bool _isControlEnabled;
        private ChartData _chartData = new ChartData();
        private ChartType _lineSymbols;
        private ChartView _chartView = new ChartView();

        public GraphViewModel( IDataBreakpoint breakPoint, Action<GraphViewModel> removeAction)
        {
            BreakPoint = breakPoint;
            _removeAction = removeAction;
            PauseButtonEnable = false;
            RunButtonEnable = true;
            GraphTypes = new ObservableCollection<string>() {"Linear", "Digital"};
            Addresses = new ObservableCollection<string>() {breakPoint.Address};
            SymbolFill = Settings.GraphSettings.MarkerColour;
            SymbolMarker = Settings.GraphSettings.MarkerSymbol;
            ConnectionFill = Settings.GraphSettings.LineColour;
            PauseButtonClick = new RelayCommand<EventArgs>(PauseButtonClickHandler);
            RunButtonClick = new RelayCommand<EventArgs>(RunButtonClickHandler);
            CloseButtonClick = new RelayCommand<EventArgs>(CloseButtonClickHandler);
            Results = new ObservableCollection<Result>();
            LinearDataSeries = GetLineardataSeries();
            DigitalDataSeries = GetDigitalDataSeries();
            SelectedAddress = Addresses.FirstOrDefault();
            SelectedGraphType = GraphTypes.FirstOrDefault();
        }

        private ObservableCollection<XYDataSeries> GetDigitalDataSeries()
        {
            var digitalDataSeriesSet = new ObservableCollection<XYDataSeries>();
            for (int i = 0; i <= 7;i++ )
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
                        Path = new PropertyPath(string.Format("Bit{0}.Key",i.ToString())),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    ValueBinding = new Binding()
                    {
                        Path = new PropertyPath(string.Format("Bit{0}.Value", i.ToString())),
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    
                   
                };
                dataseries.PlotElementLoaded+=new EventHandler(DataSeries_OnPlotElementLoaded);
                
                digitalDataSeriesSet.Add(dataseries);
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

        public ObservableCollection<string> Bits
        {
            get { return _bits; }
            set
            {
                _bits = value;
            }
        }

        public Brush SymbolFill
        {
            get { return _symbolFill; }
            set
            {
                _symbolFill = value;
                this.OnPropertyChanged("SymbolFill");
            }
        }

        public Brush ConnectionFill
        {
            get { return _connectionFill; }
            set
            {
                _connectionFill = value;
                this.OnPropertyChanged("ConnectionFill");
            }
        }

        public Marker SymbolMarker
        {
            get { return _symbolMarker; }
            set
            {
                _symbolMarker = value;
                this.OnPropertyChanged("SymbolMarker");
            }
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
            byte valAtFirstAddress = digitalDictionary.First().Value;
            Bit0 = new KeyValuePair<int, double>(serialNo, 0 +  (GetBit(valAtFirstAddress, 0) ? 5 : 0));
            Bit1 = new KeyValuePair<int, double>(serialNo, 10 + (GetBit(valAtFirstAddress, 1) ? 5 : 0));
            Bit2 = new KeyValuePair<int, double>(serialNo, 20 + (GetBit(valAtFirstAddress, 2) ? 5 : 0));
            Bit3 = new KeyValuePair<int, double>(serialNo, 30 + (GetBit(valAtFirstAddress, 3) ? 5 : 0));
            Bit4 = new KeyValuePair<int, double>(serialNo, 40 + (GetBit(valAtFirstAddress, 4) ? 5 : 0));
            Bit5 = new KeyValuePair<int, double>(serialNo, 50 + (GetBit(valAtFirstAddress, 5) ? 5 : 0));
            Bit6 = new KeyValuePair<int, double>(serialNo, 60 + (GetBit(valAtFirstAddress, 6) ? 5 : 0));
            Bit7 = new KeyValuePair<int, double>(serialNo, 70 + (GetBit(valAtFirstAddress, 7) ? 5 : 0));
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
        
        public KeyValuePair<int, double> Bit0
        {
            get;
            set;
        }

        public KeyValuePair<int, double> Bit1
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit2
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit3
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit4
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit5
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit6
        {
            get;
            set;
        }
        public KeyValuePair<int, double> Bit7
        {
            get;
            set;
        }
    }
}
