using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Fdk.Ui.ViewModelUtils;

namespace Company.DataViewer.ViewModel
{
    public class GraphViewModel : ViewModelBase<GraphViewModel>
    {
        public string BreakPointId { get; set; }
        private readonly Action<GraphViewModel> _removeAction;

        private bool _pauseButtonEnable;
        private bool _runButtonEnable;

        public GraphViewModel( string breakPointId, Action<GraphViewModel> removeAction)
        {
            BreakPointId = breakPointId;
            _removeAction = removeAction;
            PauseButtonEnable = false;
            RunButtonEnable = true;
            PauseButtonClick = new RelayCommand<EventArgs>(PauseButtonClickHandler);
            RunButtonClick = new RelayCommand<EventArgs>(RunButtonClickHandler);
            CloseButtonClick = new RelayCommand<EventArgs>(CloseButtonClickHandler);
            Results = new ObservableCollection<Result>();
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

        public void AddPoint(int value)
        {
            if(Results.Count <=GraphSettings.MaxXValue-1)
            Results.Add(new Result( new KeyValuePair<int, int>(Results.Count()+1,value)));
            else
            {
                Shift(value);
            }
        }

        private void Shift(int val)
        {
            var list = Results.Skip(1).Select(result=>result.KeyValuePair.Value).ToList();
            Results.Clear();
            foreach (var value in list)
            {
                Results.Add(new Result(new KeyValuePair<int, int>(Results.Count() + 1, value)));
            }
            Results.Add(new Result(new KeyValuePair<int, int>(Results.Count() + 1, val)));
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
        public Result(KeyValuePair<int, int> keyValuePair)
        {
            KeyValuePair = keyValuePair;
        }

        public KeyValuePair<int, int> KeyValuePair { get; set; }
    }
}
