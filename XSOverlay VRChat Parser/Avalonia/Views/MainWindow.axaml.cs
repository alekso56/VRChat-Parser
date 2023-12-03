using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Xml;

namespace XSOverlay_VRChat_Parser.Avalonia.Views
{
    public class MainWindow : AcrylicWindow
    {
        public static MainWindow MainWindowRef;

        public static ConcurrentQueue<string> MessageQueue = new ConcurrentQueue<string>();
        private readonly TextEditor EventLog;
        private readonly Button GitHubLink;
        private readonly ComboBox voiceBox;

        private DispatcherTimer LogUpdateTimer;
        private static bool ScrollDelayToggle = false;

        public static SpeechSynthesizer synth = RuntimeInformation
         .IsOSPlatform(OSPlatform.Windows) ? new SpeechSynthesizer() : null;

        public static Prompt Prompt { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            GitHubLink = this.FindControl<Button>("GitHubLink");
            EventLog = this.FindControl<TextEditor>("EventLog");

            this.PointerPressed += MainWindow_PointerPressed;
            GitHubLink.Click += GitHubLink_Click;

            XmlReader reader = XmlReader.Create("Resources\\EventLogSH.xshd");
            HighlightingManager.Instance.RegisterHighlighting("EventLogSH", null, HighlightingLoader.Load(reader, HighlightingManager.Instance));

            MainWindowRef = this;

            // This is broken right now. Issue: https://github.com/AvaloniaUI/AvaloniaEdit/issues/133
            EventLog.Options.EnableHyperlinks = true;
            EventLog.Options.RequireControlModifierForHyperlinkClick = false;

            EventLog.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("EventLogSH");

            EventLog.AddHandler(AvaloniaEdit.Rendering.VisualLineLinkText.OpenUriEvent, (s, e) =>
            {
                Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true, RedirectStandardOutput = false });
            });

            LogUpdateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Background,
                new EventHandler(delegate (Object o, EventArgs ea)
                {
                    if (MessageQueue.Count > 0)
                    {
                        string message = string.Empty;
                        while (MessageQueue.TryDequeue(out message))
                        {
                            EventLog.AppendText(message);
                        }

                        ScrollDelayToggle = true;
                    }
                    else if (ScrollDelayToggle)
                    {
                        EventLog.ScrollToEnd();
                        ScrollDelayToggle = false;
                    }

                }));
            voiceBox = this.FindControl<ComboBox>("voiceBox");
            if (synth != null)
            {
                List<string> voices = new List<string>();
                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    voices.Add(info.Name);
                }

                voiceBox.Items = voices;
                if (UIMain.Configuration.Voiceselection != null)
                {
                    int index = 0;
                    foreach (string item in voiceBox.Items)
                    {
                        if (item.Equals(UIMain.Configuration.Voiceselection))
                        {
                            voiceBox.SelectedIndex = index;
                            break;
                        }
                        index++;
                    }
                }
                else
                {
                    voiceBox.SelectedIndex = 0;
                }
                voiceBox.SelectionChanged += VoiceBox_SelectionChanged;
            }
            else
            {
                voiceBox.Items = "Unsupported.";
                voiceBox.SelectedIndex = 0;
                voiceBox.IsEnabled = false;
            }
            LogUpdateTimer.Start();
        }

        //TODO: implement UI based saving
        private void VoiceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIMain.Configuration.Voiceselection = voiceBox.SelectedItem.ToString();
            UIMain.SaveConfigurationDebounced();
        }

        private void GitHubLink_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/nnaaa-vr/XSOverlay-VRChat-Parser", UseShellExecute = true, RedirectStandardOutput = false });
        }

        private void MainWindow_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
        {
            Point mousePosition = e.GetCurrentPoint(MainWindowRef).Position;

            if (mousePosition.Y <= 30 && mousePosition.X >= 475)
                return;

            if (mousePosition.Y <= 90)
                this.BeginMoveDrag(e);
        }

        public static void EventLogAppend(string message)
        {
            MessageQueue.Enqueue(message);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}