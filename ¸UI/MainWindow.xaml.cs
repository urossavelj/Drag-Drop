using _UI.Helpers;
using _UI.Models;
using API.Dtos;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
        private Process? _process;
        private static readonly HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            LoadPositions();
        }

        /// <summary>
        /// On closing override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _process?.Kill();
            base.OnClosing(e);
        }

        /// <summary>
        /// On mouse move event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Drop event for canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Logic for creating grid element with label and text box
        /// </summary>
        /// <param name="text"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Grid CreateRemovableEditableTextBlock(string text, string name)
        {
            Grid grid = new Grid
            {
                Width = 200,
                Height = 60,
                Background = Brushes.White,
                Name = name + "Grid"
            };

            TextBlock nameLabel = new TextBlock
            {
                Text = name,
                Width = 170,
                Height = 30,
                Margin = new Thickness(0, 0, 30, 0),
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };

            TextBox textBox = new TextBox
            {
                Text = text,
                Width = 170,
                Height = 30,
                Margin = new Thickness(0, 30, 30, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Name = name + "TextBox"
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

            grid.PreviewMouseLeftButtonDown += Grid_MouseLeftButtonDown;
            grid.PreviewMouseMove += Grid_MouseMove;
            grid.MouseEnter += Grid_MouseEnter;
            grid.MouseLeave += Grid_MouseLeave;

            return grid;
        }

        /// <summary>
        /// On left mouse button pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(DropCanvas);
        }

        /// <summary>
        /// On grid mouse move event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseMove(object sender, MouseEventArgs e)
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

        /// <summary>
        /// Grid mouse entered event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Grid mouse leave event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// On remove grid button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="name"></param>
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

        /// <summary>
        /// Save canvas settings to file
        /// </summary>
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

        /// <summary>
        /// Save server settings to db
        /// </summary>
        private async void SaveServerSettings()
        {
            Server server;
            Client client;
            AdditionalFields additionalFields;
            GetValues(out server, out client, out additionalFields);

            if (string.IsNullOrEmpty(client.OutboundAddress) || string.IsNullOrEmpty(client.OutboundPort))
                return;

            var url = client.OutboundAddress + ":" + client.OutboundPort + "/Settings";

            CheckAndBuildUrl(ref url);

            var serverOptions = new ServerOptions
            {
                InboundAddress = server.InboundAddress,
                InboundPort = server.InboundPort
            };

            var json = JsonSerializer.Serialize(serverOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string responseBody = string.Empty;

            try
            {
                HttpResponseMessage response = await httpClient.PutAsync(url, content);

                var statusCode = response.StatusCode;

                if (statusCode == HttpStatusCode.OK)
                {
                    responseBody = "Request succeeded! ";
                }
                else
                {
                    responseBody = $"Request failed with status code: {statusCode} ";
                }
                if (response.Headers.TryGetValues("Date", out var dateValues))
                {
                    var date = DateTime.Parse(dateValues.First());
                    responseBody += date;
                }
            }
            catch (Exception ex)
            {
                responseBody = ex.Message;
            }

            IncomingRequestsTextBox.Text = responseBody;
        }

        /// <summary>
        /// Load positions from file
        /// </summary>
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

        /// <summary>
        /// Start API service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Server server;
            Client client;
            AdditionalFields additionalFields;
            GetValues(out server, out client, out additionalFields);

            var workingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent;

            _process = Process.Start(Path.Combine(workingDirectory.ToString(), @"API\bin\Debug\net8.0\API.exe"));
        }

        /// <summary>
        /// Get values from canvas fields
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="additionalFields"></param>
        private void GetValues(out Server server, out Client client, out AdditionalFields additionalFields)
        {
            server = new Server();
            client = new Client();
            additionalFields = new AdditionalFields();

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
                    }
                }
            }
            additionalFields.Body = BodyTextBox.Text;
            additionalFields.HTTPParams = HTTPParamsTextBox.Text;
        }

        /// <summary>
        /// Kill API service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _process?.Kill();
        }

        /// <summary>
        /// Save button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SavePositions();
            SaveServerSettings();
        }

        /// <summary>
        /// Send button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Server server;
            Client client;
            AdditionalFields additionalFields;
            GetValues(out server, out client, out additionalFields);

            var httpParams = Parsers.ParseStringToDictionary(additionalFields.HTTPParams);

            if (string.IsNullOrEmpty(client.OutboundAddress) || string.IsNullOrEmpty(client.OutboundPort))
            {
                MessageBox.Show("Outbound address or outbound port is empty");
                return;
            }

            SendMessageAsync(client.OutboundAddress + ":" + client.OutboundPort + "/Settings", httpParams, additionalFields.Body);
        }

        /// <summary>
        /// Sends message to API service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        private async void SendMessageAsync(string url, Dictionary<string, string> headers, string body)
        {
            CheckAndBuildUrl(ref url);

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            string responseBody = string.Empty;

            try
            {
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var statusCode = response.StatusCode;

                if (statusCode == HttpStatusCode.OK)
                {
                    responseBody = "Request succeeded! ";
                }
                else
                {
                    responseBody = $"Request failed with status code: {statusCode} ";
                }
                if (response.Headers.TryGetValues("Date", out var dateValues))
                {
                    var date = DateTime.Parse(dateValues.First());
                    responseBody += date;
                }
            }
            catch (Exception ex)
            {
                responseBody = ex.Message;
            }

            IncomingRequestsTextBox.Text = responseBody;
        }

        /// <summary>
        /// Checks and builds url
        /// </summary>
        /// <param name="url"></param>
        private static void CheckAndBuildUrl(ref string url)
        {
            try
            {
                var builder = new UriBuilder(url);
                url = builder.ToString();
            }
            catch
            {
                MessageBox.Show($"Url invalid");
                return;
            }
        }
    }
}