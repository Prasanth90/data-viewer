using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Atmel.Studio.Services;
using Atmel.Studio.Services.Device;
using EnvDTE;
using Fdk.Ui.ViewModelUtils;
using Microsoft.VisualStudio.Shell;

namespace Company.DataViewer.ViewModel
{
    public class GraphsViewModel : ViewModelBase<GraphsViewModel>
    {
        private ObservableCollection<GraphViewModel> _graphItems;
        private DebuggerEvents _debuggerEvents;
        private IDataBreakpointService _service;
        private ITargetService2 _targetService2;
        private DTE _dte;
        private List<IDataBreakpoint> _breakpoints;
        private ITarget2 _target;

        public GraphsViewModel()
        {
            GraphItems = new ObservableCollection<GraphViewModel>();
            _breakpoints = new List<IDataBreakpoint>();
            Subscribe();
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
                if (graphViewModel.BreakPointId.Equals(breakPointId))
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
            _service = ATServiceProvider.DataBreakpointService;
            if (_service != null)
            {
                _breakpoints = new List<IDataBreakpoint>(_service.DataBreakpoints);
                _service.BreakpointsAdded += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsAdded);
                _service.BreakpointsRemoved += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsRemoved);
                _service.BreakpointsChanged += new EventHandler<BreakpointsChangedEventArgs>(_service_BreakpointsChanged);
                _service.EnterDataBreakpointBreakMode +=
                    new EventHandler<DataBreakpointBreakEventArgs>(_service_EnterDataBreakpointBreakMode);
                _service.EnterDataBreakpointTriggerMode += new EventHandler<DataBreakpointTriggerEventArgs>(_service_EnterDataBreakpointTriggerMode);
                _dte = Package.GetGlobalService(typeof (EnvDTE.DTE)) as EnvDTE.DTE;
                if (_dte != null)
                {
                    this._debuggerEvents = _dte.Events.DebuggerEvents;
                    if (this._debuggerEvents != null)
                    {
                        this._debuggerEvents.OnEnterBreakMode += debuggerEvents_OnEnterBreakMode;
                    }
                }
                if (_breakpoints.Count > 0)
                {
                    foreach (var dataBreakpoint in _breakpoints)
                    {
                        var graphViewModel = new GraphViewModel(dataBreakpoint.Id, Remove);
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
            _service.ChangeWhenHitInformation(e.GraphViewModel.BreakPointId, breakPointInfo);
        }

        private void graphViewModel_PauseClicked(object o, GraphEventArgs e)
        {
            var breakPointInfo = new DataBreakpointTriggerInfo { ContinueExecution = false, IsEnabled = false,Message = ""};
            _service.ChangeWhenHitInformation(e.GraphViewModel.BreakPointId, breakPointInfo );
        }

        void _service_EnterDataBreakpointTriggerMode(object sender, DataBreakpointTriggerEventArgs e)
        {
            _target = _targetService2.GetLaunchedTarget();
            //IAddressSpace addressSpace = _target.Device.GetAddressSpace(MemoryTypes.Data);
            if (e.DataBreakpointInfo.State == DataBreakpointState.Bound)
            {
                var str = e.DataBreakpointInfo.Address;
                str = str.Replace("0x", "");
                ulong t = ulong.Parse(str, NumberStyles.HexNumber);
                MemoryErrorRange[] memoryErrorRange;
                var value = _target.GetMemory(_target.GetAddressSpaceName(MemoryTypes.Data), t, 1, 1, 0,
                                              out memoryErrorRange);
                foreach (var graphViewModel in GraphItems)
                {
                    if (graphViewModel.BreakPointId.Equals(e.DataBreakpointInfo.Id))
                    {
                        graphViewModel.AddPoint(value[0]);
                    }
                }
            }
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
                        var graphViewModel = new GraphViewModel(breakpointId, Remove);
                        graphViewModel.PauseClicked += new GraphViewModel.ClickEventHandler(graphViewModel_PauseClicked);
                        graphViewModel.RunClicked += new GraphViewModel.ClickEventHandler(graphViewModel_RunClicked);
                        Add(graphViewModel);
                    }
        }

        void _service_EnterDataBreakpointBreakMode(object sender, DataBreakpointBreakEventArgs e)
        {
                    _target = _targetService2.GetLaunchedTarget();
                    //IAddressSpace addressSpace = _target.Device.GetAddressSpace(MemoryTypes.Data);
                    if (e.DataBreakpointInfo.State == DataBreakpointState.Bound)
                    {
                        var str = e.DataBreakpointInfo.Address;
                        str = str.Replace("0x", "");
                        ulong t = ulong.Parse(str, NumberStyles.HexNumber);
                        MemoryErrorRange[] memoryErrorRange;
                        var value = _target.GetMemory(_target.GetAddressSpaceName(MemoryTypes.Data), t, 1, 1, 0,
                                                      out memoryErrorRange);
                        foreach (var graphViewModel in GraphItems)
                        {
                            if (graphViewModel.BreakPointId.Equals(e.DataBreakpointInfo.Id))
                            {
                                graphViewModel.AddPoint(value[0]);
                            }
                        }
                    }
        }

        void debuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
        }
    }
}
