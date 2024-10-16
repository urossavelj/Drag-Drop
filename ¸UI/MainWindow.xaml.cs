using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _UI
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private const string PositionsFile = "positions.json";
        private HashSet<string> addedFields = new HashSet<string>();
        private Process _process;

        public MainWindow()
        {
            InitializeComponent();
            LoadPositions();
        }

        private void TextBlock_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock != null && !addedFields.Contains(textBlock.Name))
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DragDrop.DoDragDrop(textBlock, textBlock, DragDropEffects.Move);
                    }));
                }
            }
        }

        private void DropCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TextBlock)))
            {
                TextBlock textBlock = e.Data.GetData(typeof(TextBlock)) as TextBlock;
                if (textBlock != null && !addedFields.Contains(textBlock.Name))
                {
                    Point dropPosition = e.GetPosition(DropCanvas);
                    Grid grid = CreateRemovableEditableTextBlock(textBlock.Text, textBlock.Name);
                    Canvas.SetLeft(grid, dropPosition.X);
                    Canvas.SetTop(grid, dropPosition.Y);
                    DropCanvas.Children.Add(grid);
                    addedFields.Add(textBlock.Name);
                }
            }
        }

        private Grid CreateRemovableEditableTextBlock(string text, string name)
        {
            Grid grid = new Grid
            {
                Width = 200,
                Height = 60, // Adjusted height to accommodate larger TextBox
                Background = Brushes.White
            };

            TextBlock nameLabel = new TextBlock
            {
                Text = name,
                Width = 170,
                Height = 30, // Increased height for the label
                Margin = new Thickness(0, 0, 30, 0),
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14, // Increased font size for the label
                FontWeight = FontWeights.Bold
            };

            TextBox textBox = new TextBox
            {
                Text = text,
                Width = 170,
                Height = 30, // Increased height for the TextBox
                Margin = new Thickness(0, 30, 30, 0),
                VerticalAlignment = VerticalAlignment.Bottom
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
            removeButton.Click += (sender, e) => RemoveButton_Click(sender, e, name);

            grid.Children.Add(nameLabel);
            grid.Children.Add(textBox);
            grid.Children.Add(removeButton);

            grid.PreviewMouseLeftButtonDown += Grid_PreviewMouseLeftButtonDown;
            grid.PreviewMouseMove += Grid_PreviewMouseMove;
            grid.MouseEnter += Grid_MouseEnter;
            grid.MouseLeave += Grid_MouseLeave;

            return grid;
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(DropCanvas);
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
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

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Hand;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Arrow;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e, string name)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Grid grid = button.Parent as Grid;
                if (grid != null)
                {
                    DropCanvas.Children.Remove(grid);
                    addedFields.Remove(name);
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
                    TextBox textBox = grid.Children[1] as TextBox;
                    TextBlock nameLabel = grid.Children[0] as TextBlock;
                    positions.Add(new GridPosition
                    {
                        Left = Canvas.GetLeft(grid),
                        Top = Canvas.GetTop(grid),
                        Text = textBox.Text,
                        Name = nameLabel.Text
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
                    Grid grid = CreateRemovableEditableTextBlock(position.Text, position.Name);
                    Canvas.SetLeft(grid, position.Left);
                    Canvas.SetTop(grid, position.Top);
                    DropCanvas.Children.Add(grid);
                    addedFields.Add(position.Name);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SavePositions();
            base.OnClosing(e);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Server server = new Server();
            Client client = new Client();
            AdditionalFields additionalFields = new AdditionalFields();

            foreach (UIElement element in DropCanvas.Children)
            {
                if (element is Grid grid)
                {
                    TextBox textBox = grid.Children[1] as TextBox;
                    TextBlock nameLabel = grid.Children[0] as TextBlock;

                    switch (nameLabel.Text)
                    {
                        case "InboundAddress":
                            server.InboundAddress = textBox.Text;
                            break;
                        case "InboundPort":
                            server.InboundPort = textBox.Text;
                            break;
                        case "OutboundAddress":
                            client.OutboundAddress = textBox.Text;
                            break;
                        case "OutboundPort":
                            client.OutboundPort = textBox.Text;
                            break;
                        case "Body":
                            additionalFields.Body = textBox.Text;
                            break;
                        case "HTTPParams":
                            additionalFields.HTTPParams = textBox.Text;
                            break;
                        case "IncomingRequests":
                            additionalFields.IncomingRequests = textBox.Text;
                            break;
                    }
                }
            }

            // Display the values for demonstration purposes
            MessageBox.Show($"Server:\nInbound Address: {server.InboundAddress}\nInbound Port: {server.InboundPort}\n\n" +
                            $"Client:\nOutbound Address: {client.OutboundAddress}\nOutbound Port: {client.OutboundPort}\n\n" +
                            $"Additional Fields:\nBody: {additionalFields.Body}\nHTTP params: {additionalFields.HTTPParams}\nIncoming requests: {additionalFields.IncomingRequests}");

            var workingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent;

            _process = Process.Start(Path.Combine(workingDirectory.ToString(), @"API\bin\Debug\net8.0\API.exe"));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _process.Kill();
        }
    }
}