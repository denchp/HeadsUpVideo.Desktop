using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Windows.Input;
using Windows.UI.Input.Inking;
using Windows.UI;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Xml.Serialization;
using HeadsUpVideo.Desktop.Models;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeadsUpVideo.Desktop
{
    public sealed partial class MainPage : Page
    {
        InkDrawingAttributes _inkDA;
        TimeSpan lastVideoPosition;

        public MainPage()
        {
            this.InitializeComponent();

            _inkDA = new InkDrawingAttributes()
            {
                Color = Colors.Blue,
                FitToCurve = true,
                Size = new Size(10, 10)
            };

            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(_inkDA);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
            inkCanvas.Tapped += InkCanvas_Tapped;
            btnClear.Tapped += BtnClear_Tapped;
            btnOpen.Tapped += BtnOpen_Tapped;
            btnPlay.Tapped += BtnPlay_Tapped;
            
            btnMore.Tapped += BtnMore_Tapped;
            btnMoreControls.Tapped += BtnMoreControls_Tapped;
            btnFullIce.Tapped += BtnFullIce_Tapped;
            btnHalfIce.Tapped += BtnHalfIce_Tapped;
            btnCurrentToQuick.Tapped += BtnCurrentToQuick_Tapped;
            btnClearQuickPens.Tapped += BtnClearQuickPens_Tapped;
            BtnRecentFiles.Tapped += BtnRecentFiles_Tapped;
            Scrubber.ValueChanged += Scrubber_ValueChanged;

            foreach (var file in LoadRecentFileList())
            {
                var newLink = new HyperlinkButton();
                var shortLink = new HyperlinkButton();
                newLink.Content = file.Path;
                newLink.DataContext = file;

                shortLink.Content = file.Name;
                shortLink.DataContext = file;
                newLink.Tapped += OpenFile_HyperlinkButton_Handler;
                shortLink.Tapped += OpenFile_HyperlinkButton_Handler;

                lstRecentFiles.Children.Add(newLink);
                pnlRecentDropDown.Children.Add(shortLink);
            }
            RefreshQuickPens();

            this.Loaded += MainPage_Loaded;
        }

        private void BtnRecentFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlRecentDropDown.Visibility = Visibility.Visible;
        }

        private void RefreshQuickPens()
        {
            QuickPens.Children.Clear();

            foreach (var pen in LoadQuickPens())
            {
                var quickPen = new AppBarButton();
                quickPen.Foreground = new SolidColorBrush(pen.Color);

                quickPen.Icon = new SymbolIcon(Symbol.Edit);
                if (pen.DrawAsHighlighter)
                    quickPen.Icon = new SymbolIcon(Symbol.Highlight);

                quickPen.BorderThickness = new Thickness(pen.Size.Height, pen.Size.Height, pen.Size.Height, pen.Size.Height);

                quickPen.Tapped += QuickPen_Tapped;
                QuickPens.Children.Add(quickPen);
            }

        }

        private void QuickPen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pen = sender as AppBarButton;

            _inkDA.Color = ((SolidColorBrush)pen.Foreground).Color;
            _inkDA.DrawAsHighlighter = false;
            _inkDA.Size = new Size(pen.BorderThickness.Top, pen.BorderThickness.Top);

            if (((SymbolIcon)pen.Icon).Symbol == Symbol.Highlight)
                _inkDA.DrawAsHighlighter = true;

            UpdateInk(_inkDA);
        }

        private List<InkDrawingAttributes> LoadQuickPens()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new XmlSerializer(typeof(List<QuickPenModel>));
            try
            {
                List<QuickPenModel> quickPens;
                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Open))
                {
                    quickPens = serializer.Deserialize(fileStream) as List<QuickPenModel>;
                }

                if (quickPens == null)
                    return new List<InkDrawingAttributes>();

                List<InkDrawingAttributes> inkDAs = new List<InkDrawingAttributes>();
                foreach (var pen in quickPens)
                {
                    inkDAs.Add(new InkDrawingAttributes()
                    {
                        Color = pen.Color,
                        Size = new Size(pen.Size, pen.Size),
                        DrawAsHighlighter = pen.IsHighlighter
                    });

                }

                return inkDAs;
            }
            catch
            {
                return new List<InkDrawingAttributes>();
            }
        }

        private void BtnClearQuickPens_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SaveQuickPens(new List<InkDrawingAttributes>());
        }

        private void BtnCurrentToQuick_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var currentPens = LoadQuickPens();

            currentPens.Add(_inkDA);
            SaveQuickPens(currentPens);
        }

        private async void SaveQuickPens(List<InkDrawingAttributes> currentPens)
        {
            List<QuickPenModel> quickPens = new List<QuickPenModel>();

            foreach (var pen in currentPens)
            {
                quickPens.Add(new QuickPenModel()
                {
                    Color = pen.Color,
                    Size = pen.Size.Height,
                    IsHighlighter = pen.DrawAsHighlighter
                });
            }

            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new XmlSerializer(typeof(List<QuickPenModel>));

                using (var fileStream = new FileStream(folder.Path + "\\quickPens.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, quickPens);
                }

                RefreshQuickPens();
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Error saving quick pens list.  If this problem continues please contact support.");
                await dialog.ShowAsync();
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var thumb = MyFindSliderChildOfType<Thumb>(Scrubber);

            thumb.DragCompleted += Thumb_DragCompleted;

            pnlControls.Visibility = Visibility.Collapsed;

            var view = ApplicationView.GetForCurrentView();
            if (!view.TryEnterFullScreenMode())
            {
                var dialog = new MessageDialog("Unable to enter the full-screen mode.");
                await dialog.ShowAsync();
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Scrubber.Value = 50;
            lastVideoPosition = videoPlayer.Position;
        }

        public static T MyFindSliderChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }

        private void Scrubber_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var scrubber = sender as Slider;

            if (scrubber.Value != 50)
            {
                if (videoPlayer.CurrentState == MediaElementState.Playing)
                    ToggleVideoPlayPause();

                inkCanvas.InkPresenter.StrokeContainer.Clear();
                videoPlayer.Position = lastVideoPosition.Add(new TimeSpan(0, 0, 0, 0, (int)(scrubber.Value - 50) * 75));
            }
        }

        private async void OpenFile_HyperlinkButton_Handler(object sender, TappedRoutedEventArgs path)
        {
            var link = sender as HyperlinkButton;

            if (link == null)
                return;

            StorageFile file = await StorageFile.GetFileFromPathAsync(((FileModel)link.DataContext).Path);
            OpenFile(file, false);
        }

        private async void OpenFile(StorageFile file, bool addToRecentList)
        {
            if (file == null)
            {
                var dialog = new MessageDialog("There was an error opening the specified file.", "Error opening file");
                await dialog.ShowAsync();
                return;
            }

            if (addToRecentList)
            {
                var fileList = LoadRecentFileList();
                fileList.Add(new FileModel() { Path = file.Path, Name = file.Name });
                SaveRecentFileList(fileList);
            }

            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            HideMorePanels();
            pnlControls.Visibility = Visibility.Visible;
            videoPlayer.SetSource(stream, file.ContentType);
        }

        private List<FileModel> LoadRecentFileList()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var serializer = new XmlSerializer(typeof(List<FileModel>));
            try
            {
                List<FileModel> recentFiles;
                using (var fileStream = new FileStream(folder.Path + "\\recent.xml", FileMode.Open))
                {
                    recentFiles = serializer.Deserialize(fileStream) as List<FileModel>;
                }

                if (recentFiles == null)
                    return new List<FileModel>();

                return recentFiles;
            }
            catch
            {
                return new List<FileModel>();
            }
        }

        private async void SaveRecentFileList(List<FileModel> files)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var serializer = new XmlSerializer(typeof(List<FileModel>));

                using (var fileStream = new FileStream(folder.Path + "\\recent.xml", FileMode.Create))
                {
                    serializer.Serialize(fileStream, files);
                }
            }
            catch
            {
                var dialog = new MessageDialog("Error saving recent files list.  If this problem continues please contact support.");
                await dialog.ShowAsync();
            }
        }

        private void HideRinks()
        {
            HalfRink.Visibility = Visibility.Collapsed;
            FullRink.Visibility = Visibility.Collapsed;
            Scrubber.Visibility = Visibility.Visible;
        }

        private void HideMorePanels()
        {
            pnlMoreControls.Visibility = Visibility.Collapsed;
            pnlMoreOptions.Visibility = Visibility.Collapsed;
            pnlRecentDropDown.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Collapsed;
        }

        private void BtnHalfIce_Tapped(object sender, TappedRoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            HideRinks();
            HideMorePanels();
            videoPlayer.Visibility = Visibility.Collapsed;
            HalfRink.Visibility = Visibility.Visible;
            Scrubber.Visibility = Visibility.Collapsed;
        }

        private void BtnFullIce_Tapped(object sender, TappedRoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            HideRinks();
            HideMorePanels();
            Scrubber.Visibility = Visibility.Collapsed;
            videoPlayer.Visibility = Visibility.Collapsed;
            FullRink.Visibility = Visibility.Visible;
        }

        private void BtnMoreControls_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlMoreControls.Visibility = Visibility.Visible;
        }

        private void BtnClear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        private void BtnPlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleVideoPlayPause();
        }

        private void ToggleVideoPlayPause()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            switch (videoPlayer.CurrentState)
            {
                case MediaElementState.Stopped:
                case MediaElementState.Playing:
                    videoPlayer.Pause();
                    btnPlay.Icon = new SymbolIcon(Symbol.Play);
                    btnPlay.Label = "Play";
                    lastVideoPosition = videoPlayer.Position;
                    break;
                case MediaElementState.Paused:
                    HideRinks();
                    HideMorePanels();
                    videoPlayer.Visibility = Visibility.Visible;
                    videoPlayer.Play();
                    Scrubber.Value = 50;
                    btnPlay.Icon = new SymbolIcon(Symbol.Pause);
                    btnPlay.Label = "Pause";
                    break;
            }
        }

        private void HideAppBar()
        {
            pnlControls.Visibility = Visibility.Collapsed;
        }

        private async void BtnOpen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".avi");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
                OpenFile(file, true);
        }

        private void ColorButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Color = ((SolidColorBrush)((AppBarButton)sender).Background).Color;
            UpdateInk(_inkDA);
        }

        private void HideOptionsBar()
        {
            HideMorePanels();
        }

        private void BtnMore_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlMoreOptions.Visibility = Visibility.Visible;
        }

        private void btnSmall_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(5, 5);
            UpdateInk(_inkDA);
        }

        private void btnMed_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(10, 10);
            UpdateInk(_inkDA);
        }

        private void btnLarge_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _inkDA.Size = new Size(20, 20);
            UpdateInk(_inkDA);
        }

        private void UpdateInk(InkDrawingAttributes inkDA)
        {
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(inkDA);
            HideOptionsBar();
        }

        private void btnHighlight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var btn = sender as AppBarButton;

            _inkDA.DrawAsHighlighter = !_inkDA.DrawAsHighlighter;
            UpdateInk(_inkDA);

            if (_inkDA.DrawAsHighlighter)
                btn.Background = new SolidColorBrush(Color.FromArgb(44, 9, 00, 00));
            else
                btn.Background = new SolidColorBrush(Color.FromArgb(0, 9, 00, 00));

            HideOptionsBar();
        }
    }
}
