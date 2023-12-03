using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using XSOverlay_VRChat_Parser.Avalonia.Views;
using XSOverlay_VRChat_Parser.Avalonia;

namespace XSOverlay_VRChat_Parser.Helpers
{
    internal class Speech
    {
        internal static SpeechSynthesizer synth = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new SpeechSynthesizer() : null;

        internal static Prompt Prompt { get; set; }

        public static void say(string words) {
            if (UIMain.Configuration.UseWindowsTTS && synth != null)
            {
                if (Prompt != null && !Prompt.IsCompleted) synth.SpeakAsyncCancel(Prompt);
                if (UIMain.Configuration.Voiceselection != null)
                {
                    synth.SelectVoice(UIMain.Configuration.Voiceselection);
                }
                synth.Volume = UIMain.Configuration.voiceVolume;
                Prompt = synth.SpeakAsync(words);
            }
        }

        public static List<string> getInstalledVoices() {

            List<string> voices = new List<string>();
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                voices.Add(info.Name);
            }
            return voices;  
        }
    }
}
