using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private const string PositionsFile = "positions.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadPositions();
        }

        private void DraggableTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DragDrop.DoDragDrop(textBox, textBox, DragDropEffects.Move);
                    }));
                }
            }
        }

        private void DropCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TextBox)))
            {
                TextBox textBox = e.Data.GetData(typeof(TextBox)) as TextBox;
                if (textBox != null)
                {
                    Point dropPosition = e.GetPosition(DropCanvas);
                    Grid grid = CreateRemovableTextBox(textBox.Text);
                    Canvas.SetLeft(grid, dropPosition.X);
                    Canvas.SetTop(grid, dropPosition.Y);
                    DropCanvas.Children.Add(grid);
                }
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(DropCanvas);
        }

        private Grid CreateRemovableTextBox(string text)
        {
            Grid grid = new Grid
            {
                Width = 200,
                Height = 30,
                Background = Brushes.White
            };

            TextBox textBox = new TextBox
            {
                Text = text,
                Width = 170,
                Height = 30,
                Margin = new Thickness(0, 0, 30, 0)
            };

            Button removeButton = new Button
            {
                Content = "X",
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };
            removeButton.Click += RemoveItem_Click;

            grid.Children.Add(textBox);
            grid.Children.Add(removeButton);

            grid.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            grid.PreviewMouseMove += TextBox_PreviewMouseMove;
            grid.MouseEnter += TextBox_MouseEnter;
            grid.MouseLeave += TextBox_MouseLeave;

            return grid;
        }


        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Cursor = Cursors.Hand;
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Cursor = Cursors.Arrow;
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Grid grid = button.Parent as Grid;
                if (grid != null)
                {
                    DropCanvas.Children.Remove(grid);
                }
            }
        }

        private void TextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Grid grid = sender as Grid;
                if (grid != null)
                {
                    Point currentPosition = e.GetPosition(DropCanvas);
                    double offsetX = currentPosition.X - _startPoint.X;
                    double offsetY = currentPosition.Y - _startPoint.Y;

                    double newLeft = Canvas.GetLeft(grid) + offsetX;
                    double newTop = Canvas.GetTop(grid) + offsetY;

                    Canvas.SetLeft(grid, newLeft);
                    Canvas.SetTop(grid, newTop);

                    _startPoint = currentPosition;
                }
            }
        }

        private void SavePositions()
        {
            List<GridPosition> positions = new List<GridPosition>();
            foreach (UIElement element in DropCanvas.Children)
            {
                if (element is Grid grid)
                {
                    TextBox textBox = grid.Children[0] as TextBox;
                    positions.Add(new GridPosition
                    {
                        Left = Canvas.GetLeft(grid),
                        Top = Canvas.GetTop(grid),
                        Text = textBox.Text
                    });
                }
            }
            string json = JsonSerializer.Serialize(positions);
            File.WriteAllText(PositionsFile, json);
        }

        private void LoadPositions()
        {
            if (File.Exists(PositionsFile))
            {
                string json = File.ReadAllText(PositionsFile);
                List<GridPosition> positions = JsonSerializer.Deserialize<List<GridPosition>>(json);
                foreach (GridPosition position in positions)
                {
                    Grid grid = CreateRemovableTextBox(position.Text);
                    Canvas.SetLeft(grid, position.Left);
                    Canvas.SetTop(grid, position.Top);
                    DropCanvas.Children.Add(grid);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SavePositions();
            base.OnClosing(e);
        }
    }
}