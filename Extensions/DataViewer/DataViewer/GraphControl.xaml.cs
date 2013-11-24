using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.C1Chart;
using Company.DataViewer.ViewModel;

namespace Company.DataViewer
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class GraphControl : UserControl
    {
        public GraphControl()
        {
            InitializeComponent();
            this.DataContext = new GraphsViewModel();
            
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            C1Chart c1Chart = sender as C1Chart;
            
            
            if (c1Chart != null)
            {
                foreach (var child in c1Chart.Data.Children)
                {
                    child.SymbolSize = new Size(6, 6);
                    child.SymbolFill = Brushes.Yellow;
                    child.SymbolMarker = Marker.Dot;
                    child.ConnectionFill = Brushes.Crimson;
                }

                if (c1Chart.Actions.Count == 0)
                {
                    c1Chart.Actions.Add(new ScaleAction()
                        {
                            MouseWheelDirection = MouseWheelDirection.XY,
                            Modifiers = ModifierKeys.Control

                        });
                    c1Chart.Actions.Add(new ZoomAction()
                        {
                            MouseButton = MouseButton.Left,
                            Modifiers = ModifierKeys.Shift,
                            Stroke = Brushes.Black
                        });
                }
                c1Chart.View.AxisX.Title = new TextBlock()
                    {
                        Text = "nth BreakPoint Hit",
                        TextAlignment = TextAlignment.Center,
                        Foreground = Brushes.DarkBlue,
                        FontWeight = FontWeights.Bold,
                    };
                c1Chart.View.AxisY.Title = new TextBlock()
                    {
                        Text = "Value",
                        TextAlignment = TextAlignment.Center,
                        Foreground = Brushes.DarkBlue,
                        FontWeight = FontWeights.Bold,
                    };
                c1Chart.View.AxisX.Min = 0;         
                c1Chart.View.AxisX.Max = GraphSettings.MaxXValue;
                c1Chart.View.AxisX.MinScale = 0.00001;
                c1Chart.View.AxisY.MinScale = 0.00001;
                //c1Chart.View.AxisY.Min = 0;
                //c1Chart.View.AxisY.Max = 255;
                //c1Chart.View.AxisY.MajorUnit = 30;
            }
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
                if (pointindex != -1)
                {
                    SetToolTipText(string.Format("{0},{1}", (plotelement.DataPoint.PointIndex+1).ToString(),plotelement.DataPoint.Value.ToString()));
                    Show_PopupToolTip(sender, e);
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
}
