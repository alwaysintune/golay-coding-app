using CodingApp.Auxiliary.Internal;
using CodingApp.Auxiliary.UI.Validation;
using CodingApp.Channels.BinaryChannels;
using CodingApp.Codes.Golay;
using CodingApp.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodingApp {
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private BinaryMatrix _inputMatrix;
        private BinaryMatrix _outputMatrix;
        private readonly BinarySymmetricChannel _channel;
        private readonly Encoding _encoding;
        private BitmapImage _image;
        private readonly BitmapImage _nothingImage;
        private readonly BitmapImage _inProgressImage;
        private readonly BitmapImage _doneImage;
        private readonly List<Rectangle> _rectangles;
        private readonly SolidColorBrush _match;
        private readonly SolidColorBrush _error;

        private string _vectorFieldValue;
        public string VectorFieldValue {
            get => _vectorFieldValue;
            set => SetField(ref _vectorFieldValue, value);
        }

        private string _probabilityFieldValue;
        public string ProbabilityFieldValue {
            get => _probabilityFieldValue;
            set => SetField(ref _probabilityFieldValue, value);
        }

        private string _textFieldValue;
        public string TextFieldValue {
            get => _textFieldValue;
            set => SetField(ref _textFieldValue, value);
        }

        private bool _isMatrixCollapsed = true;
        public bool IsMatrixCollapsed {
            get => _isMatrixCollapsed;
            set => SetField(ref _isMatrixCollapsed, value);
        }

        private bool _isInitElementsCollapsed = false;
        public bool IsInitElementsCollapsed {
            get => _isInitElementsCollapsed;
            set => SetField(ref _isInitElementsCollapsed, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value,
            [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public MainWindow() {
            _channel = new BinarySymmetricChannel(new Golay());
            _encoding = Encoding.UTF8;
            _rectangles = new List<Rectangle>();
            _nothingImage = new BitmapImage(new Uri("pack://application:,,,/Resources/" + "Nothing.png"));
            _inProgressImage = new BitmapImage(new Uri("pack://application:,,,/Resources/" + "InProgress.png"));
            _doneImage = new BitmapImage(new Uri("pack://application:,,,/Resources/" + "Done.png"));

            InitializeComponent();

            _match = (SolidColorBrush)FindResource("Match");
            _error = (SolidColorBrush)FindResource("Error");

            VectorMatchBar.Columns = _channel.BinaryCode.GeneratorMatrix.N;
            VectorMatchBar.MaxWidth = Input_dataGrid.MinColumnWidth * VectorMatchBar.Columns;
            for (int i = 0; i < VectorMatchBar.Columns; i++) {
                var rectangle = new Rectangle {
                    Uid = i.ToString(),
                    Fill = _match
                };
                _rectangles.Add(rectangle);
                VectorMatchBar.Children.Add(rectangle);
            }
            Output_dataGrid.CellEditEnding += CheckForErrors;
            DataContext = this;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
            var column = e.Column as DataGridTextColumn;
            column.CellStyle = new Style {
                TargetType = typeof(DataGridCell),
                Setters = {
                    new Setter(MarginProperty, new Thickness(1, 1, 1, 1)),
                }
            };

            column.ElementStyle = new Style {
                TargetType = typeof(TextBlock),
                Setters = {
                    new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center),
                    new Setter(VerticalAlignmentProperty, VerticalAlignment.Center),
                }
            };

            column.EditingElementStyle = FindResource("EditingElementStyle") as Style;

            var binding = column.Binding as Binding;
            binding.Path = new PropertyPath(binding.Path.Path + ".Value");
            binding.ValidatesOnDataErrors = true;
            binding.NotifyOnValidationError = true;
            binding.ValidationRules.Add(new MatrixValidation());
        }

        private bool IsValid(DependencyObject obj) {
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsValid);
        }

        private void Button_Confirm1(object sender, RoutedEventArgs e) {
            if (!IsValid(this) ||
                string.IsNullOrEmpty(VectorFieldValue) ||
                string.IsNullOrEmpty(ProbabilityFieldValue))
                return;

            try {
                _inputMatrix = _channel.EncodeInput(VectorFieldValue, _channel.VectorLength);
            }
            catch (Exception ex) {
                ShowMessageBox(ex.Message);
                return;
            }

            if (IsMatrixCollapsed)
                IsMatrixCollapsed = !IsMatrixCollapsed;

            Input_dataGrid.ItemsSource = BindingHelper.GetBindable2DArray(_inputMatrix);
            Input_dataGrid.Width = Input_dataGrid.MinColumnWidth * _inputMatrix.N + 2;

            _outputMatrix = _channel.SendThroughChannel(_inputMatrix, Parse(ProbabilityFieldValue));
            Output_dataGrid.ItemsSource = BindingHelper.GetBindable2DArray(_outputMatrix);
            Output_dataGrid.Width = Output_dataGrid.MinColumnWidth * _channel.BinaryCode.GeneratorMatrix.N + 2;

            int errorCount = HighlightErrors();
            ErrorCountLabel.Content = Regex.Replace(ErrorCountLabel.Content as string,
                                                    @"Errors: \d+",
                                                    "Errors: " + errorCount);
            Output.Text = null;
        }

        private void CheckForErrors(object sender, DataGridCellEditEndingEventArgs e) {
            if (Output_dataGrid.SelectedItem != null) {
                var dataGrid = sender as DataGrid;
                dataGrid.CellEditEnding -= CheckForErrors;
                dataGrid.CommitEdit(); // Shenanigans
                dataGrid.CommitEdit(); // of WPF
                dataGrid.CellEditEnding += CheckForErrors;
            }
            else return;

            int errorCount = HighlightErrors();
            ErrorCountLabel.Content = Regex.Replace(ErrorCountLabel.Content as string,
                                                    @"Errors: \d+",
                                                    "Errors: " + errorCount);
        }

        private int HighlightErrors() {
            int errorCount = 0;
            for (int i = 0; i < _inputMatrix.M; i++) {
                for (int j = 0; j < _inputMatrix.N; j++) {
                    if (_inputMatrix[i, j] != _outputMatrix[i, j]) {
                        _rectangles[j].Fill = _error;
                        errorCount++;
                    }
                    else {
                        _rectangles[j].Fill = _match;
                    }
                }
            }

            return errorCount;
        }

        private void SimulateClick(Button button) {
            var peer = new ButtonAutomationPeer(button);
            var invokeProvider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProvider?.Invoke();
        }

        private void Button_Collapse(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var textBlock = (button.Content as Border).Child as TextBlock;
            textBlock.Text = textBlock.Text == "\u25b2" ? "\u25bc" : "\u25b2";
            IsInitElementsCollapsed = !IsInitElementsCollapsed;
        }

        private void Button_Decode(object sender, RoutedEventArgs e) {
            try {
                Output.Text = _channel.BinaryCode.Decode(_outputMatrix).ToString();
            }
            catch (Exception ex) {
                ShowMessageBox(ex.Message);
                SimulateClick(ConfirmButton1);
                return;
            }
        }

        private async void Button_Confirm2(object sender, RoutedEventArgs e) {
            if (!IsValid(this) ||
                string.IsNullOrEmpty(TextFieldValue) ||
                string.IsNullOrEmpty(ProbabilityFieldValue))
                return;

            string binaryInput = DataConverter.StringToBinary(TextFieldValue, _encoding);
            int modulus = binaryInput.Length % _channel.VectorLength;
            _channel.PaddingCount = _channel.VectorLength - (modulus == 0 ? _channel.VectorLength : modulus);
            if (_channel.PaddingCount > 0)
                binaryInput += new string('0', _channel.PaddingCount);

            double probability = Parse(ProbabilityFieldValue);
            IEnumerable<string> textChunks = DataConverter.Split(binaryInput, _channel.VectorLength);
            int textChunksLength = binaryInput.Length / _channel.VectorLength;

            string result = await _channel.SendWithoutEncoding(textChunks, textChunksLength, probability);
            WithoutGolay.Text = DataConverter.BinaryToString(result, _encoding);

            try {
                result = await _channel.SendWithEncoding(textChunks, textChunksLength, probability);
            }
            catch (Exception ex) {
                ShowMessageBox(ex.Message);
                return;
            }

            WithGolay.Text = DataConverter.BinaryToString(result, _encoding);
        }

        private void ShowMessageBox(string msg) {
            MessageBox.Show(msg, "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private double Parse(string str) {
            return double.Parse(
                str.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture
            );
        }

        private void Button_Browse(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog {
                Title = "Picture Upload",
                Filter = "Bitmap Files (*.bmp;*.dib)|*.bmp;*.dib|" +
                         "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "PNG (*.png)|*.png"
            };

            if (dlg.ShowDialog() == false)
                return;

            ImageNameLabel.Content = dlg.FileName;
            _image = new BitmapImage(new Uri(dlg.FileName));
            OriginalImage.Source = _image;
        }

        private async void Button_Confirm3(object sender, RoutedEventArgs e) {
            if (!IsValid(this) ||
                string.IsNullOrEmpty(ProbabilityFieldValue) ||
                _image == null)
                return;

            ConfirmButton3.IsEnabled = false;
            ClearButton.IsEnabled = false;
            BrowseButton.IsEnabled = false;
            WithoutGolayLabelImage.Source = _inProgressImage;
            WithGolayLabelImage.Source = _inProgressImage;

            byte[] pixels = ImageProcessing.BitmapSourceToPixelData(_image);
            int nearestMultiple = _channel.VectorLength * (int)Math.Round(pixels.Length * 8 / (double)_channel.VectorLength,
                                                                       MidpointRounding.AwayFromZero);
            int bytesCount = (nearestMultiple + 8 - 1) / 8;
            _channel.PaddingCount = 0;
            if (bytesCount != pixels.Length) {
                byte[] temp = new byte[bytesCount];
                _channel.PaddingCount = temp.Length - pixels.Length;
                Buffer.BlockCopy(pixels, 0, temp, 0, pixels.Length);
                pixels = temp;
            }

            double probability = Parse(ProbabilityFieldValue);

            SetImageSource(WithoutGolayImage, WithoutGolayLabelImage,
                await _channel.SendWithoutEncoding(pixels.ToArray(), nearestMultiple, probability));

            try {
                SetImageSource(WithGolayImage, WithGolayLabelImage,
                    await _channel.SendWithEncoding(pixels.ToArray(), nearestMultiple, probability));
            }
            catch (Exception ex) {
                ShowMessageBox(ex.Message);
                return;
            }

            ConfirmButton3.IsEnabled = true;
            ClearButton.IsEnabled = true;
            BrowseButton.IsEnabled = true;
        }

        private void SetImageSource(Image image, Image statusImage, byte[] pixels) {
            var receivedPixels = new byte[pixels.Length - _channel.PaddingCount];
            Buffer.BlockCopy(pixels, 0, receivedPixels, 0, pixels.Length - _channel.PaddingCount);
            image.Source = ImageProcessing.PixelDataToBitmapSource(receivedPixels, _image);
            statusImage.Source = _doneImage;
        }

        private void Button_Clear(object sender, RoutedEventArgs e) {
            _image = null;
            OriginalImage.Source = null;
            WithGolayImage.Source = null;
            WithoutGolayImage.Source = null;
            GC.Collect();

            ImageNameLabel.Content = "Img...";
            WithGolayLabelImage.Source = _nothingImage;
            WithoutGolayLabelImage.Source = _nothingImage;
        }
    }
}
