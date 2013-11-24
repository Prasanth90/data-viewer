using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell;

namespace Company.DataViewer.Options
{

    [Guid("D2AE3A81-79E5-4D1D-B6DC-29CA310A8792")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class DataViewerOptionsPage : DialogPage
    {
        private bool _syncDataBreakPoints;
        private string _markerSymbol;
        private string _xAxisLimit;
        private string _lineColour;
        private string _markerColour;

        [Category(@"DataViewer")]
        [DisplayName(@"Marker Colour")]
        [Description(@"Specify the colour of the marker, which is used to denote a point in the graph")]
        public string MarkerColour
        {
            get { return _markerColour; }
            set { _markerColour = value; }
        }

        [Category(@"DataViewer")]
        [DisplayName(@"Line Colour")]
        [Description(@"Specify the colour of the line, which is used to connect the points in the graph")]
        public string LineColour
        {
            get { return _lineColour; }
            set { _lineColour = value; }
        }


        [Category(@"DataViewer")]
        [DisplayName(@"X Axis Limit")]
        [Description(@"Specify the maximum limit of the XAxis in the X Axis")]
        public string XAxisLimit
        {
            get { return _xAxisLimit; }
            set { _xAxisLimit = value; }
        }

        [Category(@"DataViewer")]
        [DisplayName(@"Marker Symbol")]
        [Description(@"Specify the symbol of the marker, which is used to denote a point in the graph")]
        public string MarkerSymbol
        {
            get { return _markerSymbol; }
            set { _markerSymbol = value; }
        }

        [Category(@"DataViewer")]
        [DisplayName(@"SyncDataBreakPoints")]
        [Description(@"Enabling this option will load the graph based on the data breakpoints added in the project. Adding a databreakpoint will create a graph to track its value")]
        public bool SyncDataBreakPoints
        {
            get { return _syncDataBreakPoints; }
            set { _syncDataBreakPoints = value; }
        }


        public override void SaveSettingsToStorage()
        {
            GraphSettings.ApplySettings(this);
            base.SaveSettingsToStorage();
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();
        }

        protected override void OnApply(DialogPage.PageApplyEventArgs e)
        {
            SaveSettingsToStorage();
            base.OnApply(e);
        }

    }

    public delegate void DataViewerOptionsChangedHandler(object sender, DataViewerOptionsChangedArgs e);

   

    public class DataViewerOptionsChangedArgs : EventArgs
    {
        public DataViewerOptionsPage DataViewerOptions;

        public DataViewerOptionsChangedArgs(DataViewerOptionsPage options)
        {
            DataViewerOptions = options;
        }
    }
}


