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
        private GraphsViewModel _graphsViewModel;
        public GraphControl()
        {
            InitializeComponent();
             _graphsViewModel =  new GraphsViewModel();
            HostPanel.Children.Clear();
            HostPanel.Children.Add(Message);
             _graphsViewModel.GraphItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GraphItems_CollectionChanged);
            this.DataContext = _graphsViewModel;
        }

        void GraphItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_graphsViewModel.GraphItems.Count > 0)
            {
                if (!HostPanel.Children.Contains(GraphContainer))
                {
                    HostPanel.Children.Clear();
                    HostPanel.Children.Add(GraphContainer);
                }
            }
            else
            {
                HostPanel.Children.Clear();
                HostPanel.Children.Add(Message);
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            C1Chart c1Chart = sender as C1Chart;
            
            
            if (c1Chart != null)
            {
                
                foreach (var child in c1Chart.Data.Children)
                {
                    child.SymbolSize = new Size(6, 6);
            //        child.SymbolFill = GraphSettings.MarkerColour;
              //      child.SymbolMarker = GraphSettings.MarkerSymbol;
                //    child.ConnectionFill = GraphSettings.LineColour;
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
                c1Chart.View.AxisX.Max = Settings.GraphSettings.MaxXValue;
                c1Chart.View.AxisX.MinScale = 0.00001;
                c1Chart.View.AxisY.MinScale = 0.00001;
                //c1Chart.View.AxisY.Min = 0;
                //c1Chart.View.AxisY.Max = 255;
                //c1Chart.View.AxisY.MajorUnit = 30;
            }
        }

        private void Clicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
