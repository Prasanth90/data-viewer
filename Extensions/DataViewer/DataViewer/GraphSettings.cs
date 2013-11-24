using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Company.DataViewer.Options;

namespace Company.DataViewer
{
    public static class GraphSettings
    {
        public static int MaxXValue { get; set; }
        public static int MaxYValue { get; set; }
        public static string MarkerColour { get; set; }
        public static string MarkerSymbol { get; set; }
        public static string LineColour { get; set; }
        public static bool SyncDataBreakPointSettings { get; set; }

        public static void ApplySettings(DataViewerOptionsPage dataViewerOptionsPage)
        {
            Update(dataViewerOptionsPage);
        }

        private static void Update(DataViewerOptionsPage dataViewerOptionsPage)
        {
            MaxXValue = Convert.ToInt32(dataViewerOptionsPage.XAxisLimit);
            MarkerColour = dataViewerOptionsPage.MarkerColour;
            MarkerSymbol = dataViewerOptionsPage.MarkerSymbol;
            LineColour = dataViewerOptionsPage.LineColour;
            SyncDataBreakPointSettings = dataViewerOptionsPage.SyncDataBreakPoints;
        }
    }
}
