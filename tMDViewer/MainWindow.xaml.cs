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
using Microsoft.Web.WebView2.Core;
using System.Net.Http;
using System.Security.Policy;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Data;

namespace tMDViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            engine = new Engine();
            addressBar.Text = @"https://gist.githubusercontent.com/allysonsilva/85fff14a22bbdf55485be947566cc09e/raw/fa8048a906ebed3c445d08b20c9173afd1b4a1e5/Full-Markdown.md";
            InitializeWebView();
        }

        Engine engine;
        readonly string HtmlFileExt = @".tmd.html";
        List<string> TempFiles = new List<string>();

        protected override void OnClosed(EventArgs e)
        {
            webView.CoreWebView2.Navigate(@"about:blank");
            webView.Stop();

            foreach (var tempfile in TempFiles)
            {
                try
                {
                    if (File.Exists(tempfile))
                        File.Delete(tempfile);
                }
                catch { }
            }

            base.OnClosed(e);
        }

        private void InitializeWebView()
        {
            webView.CoreWebView2InitializationCompleted += delegate (object? sender, CoreWebView2InitializationCompletedEventArgs e)
            {
                if (e.IsSuccess)
                {
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    webView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                    webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                }
            };

            webView.NavigationCompleted += delegate (object? sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                if (webView.CoreWebView2.Source.EndsWith(HtmlFileExt))
                {
                    var path = new Uri(webView.CoreWebView2.Source).LocalPath;
                    if (TempFiles.Contains(path))
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                            TempFiles.Remove(path);
                        }
                    }
                }
            };

            webView.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri != null && e.Uri.EndsWith(HtmlFileExt))
            {
                var path = new Uri(e.Uri).LocalPath;
                if (TempFiles.Contains(path))
                    return;
            }

            if (e.IsUserInitiated)
            {
                e.Cancel = true;

                if (e.Uri != string.Empty)
                {
                    if (Uri.IsWellFormedUriString(e.Uri, UriKind.Absolute))
                    {
                        if (e.Uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            var ps = new ProcessStartInfo(e.Uri)
                            {
                                UseShellExecute = true,
                                Verb = "open"
                            };
                            using var process = Process.Start(ps);
                        }
                    }
                }
            }
            if (e.NavigationKind == CoreWebView2NavigationKind.BackOrForward)
            {
                e.Cancel = true;
            }
        }

        async Task LoadContent()
        {
            try
            {
                string url = addressBar.Text;
                if (string.IsNullOrEmpty(url))
                {
                    webView?.CoreWebView2?.Navigate(@"about:blank");
                    return;
                }

                string? content = await GetContent(url);

                if (content != null)
                {
                    var html = engine.ToHtml(content);

                    string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + HtmlFileExt);
                    File.WriteAllText(tempFilePath, html);
                    File.SetAttributes(tempFilePath, File.GetAttributes(tempFilePath) | FileAttributes.Temporary);
                    TempFiles.Add(tempFilePath);

                    webView.CoreWebView2.Navigate(tempFilePath);
                }
                else
                {
                    MessageBox.Show("Cannot load content");
                }
            }
            catch
            {
                webView?.CoreWebView2?.Navigate(@"about:blank");
            }
        }

        async private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            await LoadContent();
        }

        async private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Markdown files|*.md|All Files|*.*";
            if (fileDialog.ShowDialog() == true)
            {
                addressBar.Text = fileDialog.FileName;
                await LoadContent();
            }
        }

        async Task<string?> DownloadContent(Uri url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }

        async Task<string?> GetContent(string url)
        {
            var uri = new Uri(url);
            if (uri.IsFile)
            {
                if (File.Exists(uri.LocalPath))
                {
                    return File.ReadAllText(uri.LocalPath);
                }
                return null;
            }
            else
            {
                return await DownloadContent(uri);
            }
        }

        async private void ExportHtml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? content = await GetContent(addressBar.Text);

                if (content != null)
                {
                    var txt = engine.ToHtml(content);

                    SaveFileDialog fileDialog = new SaveFileDialog();
                    if (fileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(fileDialog.FileName, txt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async private void ExportTxt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? content = await GetContent(addressBar.Text);

                if (content != null)
                {
                    var txt = engine.ToText(content);

                    SaveFileDialog fileDialog = new SaveFileDialog();
                    if (fileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(fileDialog.FileName, txt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            webView?.CoreWebView2?.ShowPrintUI();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                addressBar.Text = files[0];
            }
            e.Handled = true;
        }

        private void addressBar_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
