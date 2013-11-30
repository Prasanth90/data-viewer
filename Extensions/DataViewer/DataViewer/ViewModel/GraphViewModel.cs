using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.C1Chart;
using Fdk.Ui.ViewModelUtils;

namespace Company.DataViewer.ViewModel
{
    public class GraphViewModel : ViewModelBase<GraphViewModel>
    {
        public string BreakPointId { get; set; }
        private readonly Action<GraphViewModel> _removeAction;

        private bool _pauseButtonEnable;
        private bool _runButtonEnable;
        private Brush _symbolFill;
        private Marker _symbolMarker;
        private Brush _connectionFill;

        public GraphViewModel( string breakPointId, Action<GraphViewModel> removeAction)
        {
            BreakPointId = breakPointId;
            _removeAction = removeAction;
            PauseButtonEnable = false;
            RunButtonEnable = true;
            SymbolFill = Settings.GraphSettings.MarkerColour;
            SymbolMarker = Settings.GraphSettings.MarkerSymbol;
            ConnectionFill = Settings.GraphSettings.LineColour;
            PauseButtonClick = new RelayCommand<EventArgs>(PauseButtonClickHandler);
            RunButtonClick = new RelayCommand<EventArgs>(RunButtonClickHandler);
            CloseButtonClick = new RelayCommand<EventArgs>(CloseButtonClickHandler);
            Results = new ObservableCollection<Result>();
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

        public void AddPoint(double value)
        {
            if (Results.Count <= Settings.GraphSettings.MaxXValue - 1)
            Results.Add(new Result( new KeyValuePair<int, double>(Results.Count()+1,value)));
            else
            {
                Shift(value);
            }
        }

        private void Shift(double val)
        {
            var list = Results.Skip(1).Select(result=>result.KeyValuePair.Value).ToList();
            Results.Clear();
            foreach (var value in list)
            {
                Results.Add(new Result(new KeyValuePair<int, double>(Results.Count() + 1, value)));
            }
            Results.Add(new Result(new KeyValuePair<int, double>(Results.Count() + 1, val)));
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
        public Result(KeyValuePair<int, double> keyValuePair)
        {
            KeyValuePair = keyValuePair;
        }

        public KeyValuePair<int, double> KeyValuePair
        {
            get;
            set;
        }
    }
}
