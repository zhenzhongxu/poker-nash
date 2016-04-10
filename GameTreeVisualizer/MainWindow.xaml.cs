using GraphSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphSharp.Controls;
using QuickGraph;

namespace GameTreeVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            GraphLayout layout = new GraphLayout();
            
            var graph = new CompoundGraph<object, IEdge<object>>();

                graph.AddVertex("1");

            layout.Graph = graph;
            layout.UpdateLayout();

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GraphLayout layout = new GraphLayout();

            var graph = new CompoundGraph<object, IEdge<object>>();

            graph.AddVertex("1");

            layout.Graph = graph;
            layout.UpdateLayout();
        }

        private void Canvas_Initialized(object sender, EventArgs e)
        {
            
            GraphLayout layout = new GraphLayout();

            var graph = new CompoundGraph<object, IEdge<object>>();

            graph.AddVertex("1");

            
            layout.Graph = graph;
            layout.UpdateLayout();
            this.AddVisualChild(layout);
            this.UpdateLayout();
        }
    }
}
