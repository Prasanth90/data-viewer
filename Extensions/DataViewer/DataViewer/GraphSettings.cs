using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using C1.WPF.C1Chart;
using Company.DataViewer.Options;

namespace Company.DataViewer
{
    public class GraphSettings
    {
        public event EventHandler OptionsChanged;

        protected virtual void OnOptionsChanged()
        {
            EventHandler handler = OptionsChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public  int MaxXValue = 30;
        public  int MaxYValue { get; set; }
        public Brush MarkerColour = Brushes.CornflowerBlue;
        public Marker MarkerSymbol = Marker.Dot;
        public Brush LineColour = Brushes.Crimson;

        public void ApplySettings(DataViewerOptionsPage dataViewerOptionsPage)
        {
            Update(dataViewerOptionsPage);
            OnOptionsChanged();
        }

        private  void Update(DataViewerOptionsPage dataViewerOptionsPage)
        {
            if (!string.IsNullOrEmpty(dataViewerOptionsPage.XAxisLimit))
            {
                try
                {
                    MaxXValue = Convert.ToInt32(dataViewerOptionsPage.XAxisLimit);
                }
                catch
                {
                    MaxXValue = 20;
                }
                
            }
            MarkerColour = GetBrush(dataViewerOptionsPage.MarkerColour);
            MarkerSymbol = dataViewerOptionsPage.MarkerSymbol;
            LineColour = GetBrush(dataViewerOptionsPage.LineColour);
        }

        private  Brush GetBrush(DataViewerOptionsPage.Colours colour)
        {
            if (colour ==DataViewerOptionsPage.Colours.Black)
            {
                return Brushes.Black;
            }
            if (colour == DataViewerOptionsPage.Colours.Blue)
            {
                return Brushes.Blue;
            }
            if (colour == DataViewerOptionsPage.Colours.Crimson)
            {
                return Brushes.Crimson;
            }
            if (colour == DataViewerOptionsPage.Colours.Gray)
            {
                return Brushes.Gray;
            }
            if (colour == DataViewerOptionsPage.Colours.Green)
            {
                return Brushes.Green;
            }
            if (colour == DataViewerOptionsPage.Colours.Orange)
            {
                return Brushes.Orange;
            }
            if (colour == DataViewerOptionsPage.Colours.Red)
            {
                return Brushes.Red;
            }
            if (colour == DataViewerOptionsPage.Colours.Yellow)
            {
                return Brushes.Yellow;
            }
            return Brushes.Crimson;
        }
    }

    public static class Settings
    {
        public static GraphSettings GraphSettings { get; set; }
    }
}
