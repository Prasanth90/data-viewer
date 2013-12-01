using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Atmel.Studio.Services;
using Atmel.Studio.Services.Device;
using Atmel.VsIde.AvrStudio.Services.TargetService;
using Atmel.VsIde.AvrStudio.Services.TargetService.TCF.Internal.Services.Remote;
using Atmel.VsIde.AvrStudio.Services.TargetService.TCF.Services;
using Company.DataViewer.ExpressionEvaluator;
using Fdk.Ui.ViewModelUtils;
using Microsoft.VisualStudio.Shell;

namespace Company.DataViewer.ViewModel
{
    public class GraphsViewModel : ViewModelBase<GraphsViewModel>
    {
        private ObservableCollection<GraphViewModel> _graphItems;
        private IDataBreakpointService _service;
        private ITargetService2 _targetService2;
        private ITargetService _targetService;
        private List<IDataBreakpoint> _breakpoints;
        private ITarget2 _target;
        private ExpressionEvaluator.ExpressionEvaluationWrapper _expressionEvaluationWrapper;
        private EnvDTE.DTE _dte;

        public GraphsViewModel()
        {
            Settings.GraphSettings.OptionsChanged += new EventHandler(settings_OptionsChanged);
            GraphItems = new ObservableCollection<GraphViewModel>();
            _breakpoints = new List<IDataBreakpoint>();
            _dte = Package.GetGlobalService(typeof (EnvDTE.DTE)) as EnvDTE.DTE;
            _expressionEvaluationWrapper = new ExpressionEvaluationWrapper();
            Subscribe();
        }

        void settings_OptionsChanged(object sender, EventArgs e)
        {
            foreach (var graphViewModel in GraphItems)
            {
                graphViewModel.SymbolFill = Settings.GraphSettings.MarkerColour;
                graphViewModel.SymbolMarker = Settings.GraphSettings.MarkerSymbol;
                graphViewModel.ConnectionFill = Settings.GraphSettings.LineColour;
            }
        }
        public ObservableCollection<GraphViewModel> GraphItems
        {
            get
            {
                return _graphItems;
            }
            set
            {
                _graphItems = value;
                this.OnPropertyChanged("GraphItems");
            }
        }
        
        public void Remove(string breakPointId)
        {
            GraphViewModel graphViewModelTemp = null;
            foreach (var graphViewModel in _graphItems)
            {
                if (graphViewModel.BreakPoint.Id.Equals(breakPointId))
                {
                    graphViewModelTemp = graphViewModel;
                    break;
                }
            }
            if (graphViewModelTemp != null)
            {
                Remove(graphViewModelTemp);
            }
        }

        public void Remove(GraphViewModel graphViewModel)
        {
            GraphItems.Remove(graphViewModel);
        }

        public void Add(GraphViewModel graphViewModel)
        {
            GraphItems.Add(graphViewModel);
        }

        private void Subscribe()
        {
            _targetService2 = ATServiceProvider.TargetService2;
            _targetService = ATServiceProvider.TargetService2 as ITargetService;
            _service = ATServiceProvider.DataBreakpointService;
            if (_service != null)
            {
                _breakpoints = new List<IDataBreakpoint>(_service.DataBreakpoints);
                _service.BreakpointsAdded += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsAdded);
                _service.BreakpointsRemoved += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsRemoved);
                _service.BreakpointsChanged += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsChanged);
                _service.EnterDataBreakpointBreakMode +=new EventHandler<DataBreakpointBreakEventArgs>(_service_EnterDataBreakpointBreakMode);
                _service.EnterDataBreakpointTriggerMode += new EventHandler<DataBreakpointTriggerEventArgs>(_service_EnterDataBreakpointTriggerMode);
                if (_breakpoints.Count > 0)
                {
                    foreach (var dataBreakpoint in _breakpoints)
                    {
                        var graphViewModel = new GraphViewModel(dataBreakpoint, Remove);
                        graphViewModel.PauseClicked += new GraphViewModel.ClickEventHandler(graphViewModel_PauseClicked);
                        graphViewModel.RunClicked += new GraphViewModel.ClickEventHandler(graphViewModel_RunClicked);
                        Add(graphViewModel);
                    }
                }
            }
        }

        private void graphViewModel_RunClicked(object o, GraphEventArgs e)
        {
            var breakPointInfo = new DataBreakpointTriggerInfo { ContinueExecution = true ,IsEnabled = true,Message = ""};
            _service.ChangeWhenHitInformation(e.GraphViewModel.BreakPoint.Id, breakPointInfo);
        }

        private void graphViewModel_PauseClicked(object o, GraphEventArgs e)
        {
            var breakPointInfo = new DataBreakpointTriggerInfo { ContinueExecution = false, IsEnabled = false,Message = ""};
            _service.ChangeWhenHitInformation(e.GraphViewModel.BreakPoint.Id, breakPointInfo );
        }

        void _service_EnterDataBreakpointTriggerMode(object sender, DataBreakpointTriggerEventArgs e)
        {

            UpdateGraphs();
        }

        private void UpdateGraphs()
        {
            _target = _targetService2.GetLaunchedTarget();
            //IAddressSpace addressSpace = _target.Device.GetAddressSpace(MemoryTypes.Data);
            foreach (var graphViewModel in GraphItems)
            {
                try
                {
                    var breakpointInfo = _service.GetDataBreakpoint(graphViewModel.BreakPoint.Id);
                    if (breakpointInfo.State == DataBreakpointState.Bound)
                    {
                        Dictionary<ulong ,byte> digitalValues = new Dictionary<ulong, byte>(); 
                        double linearValue = 0;
                        byte[] bytes;
                        var str = breakpointInfo.Address;
                        var byteCount = Convert.ToInt32(breakpointInfo.Config.ByteCount);
                        str = str.Replace("0x", "");
                        ulong startAddress = ulong.Parse(str, NumberStyles.HexNumber);
                        var location = breakpointInfo.Config.Location;
                        if (location.Contains("&") && IsValidVariable(location) && startAddress != 0 & location!=breakpointInfo.Address)
                        {
                            var variable = breakpointInfo.Config.Location.Replace("&", "");
                            var stringval = _expressionEvaluationWrapper.GetVaraibleValue(variable);
                            linearValue = Convert.ToDouble(stringval);
                            bytes = _expressionEvaluationWrapper.GetValueAtAddress(startAddress, byteCount);        
                        }
                        else
                        {
                             bytes = _expressionEvaluationWrapper.GetValueAtAddress(startAddress, byteCount);
                            linearValue = bytes[0];
                        }
                        foreach (byte b in bytes)
                        {
                            digitalValues.Add(startAddress, b);
                            startAddress = startAddress + 1;
                        }
                        graphViewModel.AddPoint(linearValue,digitalValues);
                    }
                }
                catch
                {
                    // TODO:  Log messages somewhere and continue
                    continue;
                }
               
            }
        }

        private bool IsValidVariable(string location)
        {
            var list = new List<string>()
                {
                    "+",
                    "-",
                    "*",
                    "/",
                    "<",
                    ">",
                    "%",
                    "!",
                    "?",
                    "|",
                };

            var nos = new List<string>() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
            foreach (var no in nos)
            {
                if (location.StartsWith(no))
                {
                    return false;
                }
            }
            foreach (var VARIABLE in list)
            {
                if (location.Contains(VARIABLE))
                {
                    return false;
                }
            }
            return true;
        }
        

        void _service_BreakpointsChanged(object sender, BreakpointsChangedEventArgs e)
        {
            _breakpoints = new List<IDataBreakpoint>(_service.DataBreakpoints);
        }

        void _service_BreakpointsRemoved(object sender, BreakpointsChangedEventArgs e)
        {
                    _breakpoints = new List<IDataBreakpoint>(_service.DataBreakpoints);
                    foreach (var breakpointId in e.BreakpointIds)
                    {
                        Remove(breakpointId);
                    }
        }

        void _service_BreakpointsAdded(object sender, BreakpointsChangedEventArgs e)
        {
                    _breakpoints = new List<IDataBreakpoint>(_service.DataBreakpoints);
                    foreach (var breakpointId in e.BreakpointIds)
                    {
                        IDataBreakpoint breakPointInfo = _service.GetDataBreakpoint(breakpointId);
                        var graphViewModel = new GraphViewModel(breakPointInfo, Remove);
                        graphViewModel.PauseClicked += new GraphViewModel.ClickEventHandler(graphViewModel_PauseClicked);
                        graphViewModel.RunClicked += new GraphViewModel.ClickEventHandler(graphViewModel_RunClicked);
                        Add(graphViewModel);
                    }
        }

        void _service_EnterDataBreakpointBreakMode(object sender, DataBreakpointBreakEventArgs e)
        {
            UpdateGraphs();
        }
    }
}
