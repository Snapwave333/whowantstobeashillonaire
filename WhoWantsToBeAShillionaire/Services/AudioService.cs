using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace WhoWantsToBeAShillionaire.Services
{
    public class AudioService : IDisposable
    {
        private readonly Dictionary<string, MediaPlayer> _soundPlayers;
        private readonly Dictionary<string, string> _soundFiles;
        private bool _soundEnabled = true;

        public AudioService()
        {
            _soundPlayers = new Dictionary<string, MediaPlayer>();
            _soundFiles = new Dictionary<string, string>();
            InitializeSoundFiles();
        }

        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => _soundEnabled = value;
        }

        private void InitializeSoundFiles()
        {
            var soundsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
            
            // Define sound file mappings
            _soundFiles["button_click"] = Path.Combine(soundsPath, "button_click.wav");
            _soundFiles["game_start"] = Path.Combine(soundsPath, "game_start.wav");
            _soundFiles["answer_select"] = Path.Combine(soundsPath, "answer_select.wav");
            _soundFiles["final_answer"] = Path.Combine(soundsPath, "final_answer.wav");
            _soundFiles["correct_answer"] = Path.Combine(soundsPath, "correct_answer.wav");
            _soundFiles["wrong_answer"] = Path.Combine(soundsPath, "wrong_answer.wav");
            _soundFiles["lifeline_use"] = Path.Combine(soundsPath, "lifeline_use.wav");
            _soundFiles["walk_away"] = Path.Combine(soundsPath, "walk_away.wav");
            _soundFiles["game_over"] = Path.Combine(soundsPath, "game_over.wav");
            _soundFiles["victory"] = Path.Combine(soundsPath, "victory.wav");

            // Create default sound files if they don't exist
            CreateDefaultSoundFiles();
        }

        private void CreateDefaultSoundFiles()
        {
            var soundsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
            Directory.CreateDirectory(soundsPath);

            // Create or generate sound for each action if files don't exist
            foreach (var soundFile in _soundFiles)
            {
                if (!File.Exists(soundFile.Value))
                {
                    // Try to generate with AudioLDM CLI first
                    var generated = TryGenerateSoundWithAudioLDM(soundFile.Key, soundFile.Value);
                    if (!generated)
                    {
                        // Fallback: procedurally generated beep
                        CreateBeepSound(soundFile.Value, GetFrequencyForSound(soundFile.Key));
                    }
                }
            }
        }

        private string BuildAudioPromptForSound(string soundName)
        {
            return soundName switch
            {
                "button_click" => "UI click sound, short, soft, clean, modern, minimal reverb",
                "game_start" => "Game start sting, energetic, short orchestral/synth hit, uplifting",
                "answer_select" => "UI selection tone, subtle, positive, short blip",
                "final_answer" => "Tension tone, low hum building, cinematic, short",
                "correct_answer" => "Correct answer bell/chime, bright, single note, short tail",
                "wrong_answer" => "Wrong answer buzzer, short, muted, not harsh",
                "lifeline_use" => "Whoosh effect, gentle, quick, airy",
                "walk_away" => "Soft fade-out tone, gentle down-glide",
                "game_over" => "Game over sting, minor key, short, dramatic",
                "victory" => "Victory fanfare, short triumphant brass/synth, uplifting",
                _ => "UI click sound, short, soft, clean, modern"
            };
        }

        private bool TryGenerateSoundWithAudioLDM(string soundName, string targetFilePath)
        {
            try
            {
                var prompt = BuildAudioPromptForSound(soundName);
                var workDir = AppDomain.CurrentDomain.BaseDirectory;

                // Attempt to run the audioldm CLI
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "audioldm",
                    Arguments = $"-t \"{prompt}\"",
                    WorkingDirectory = workDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var proc = new System.Diagnostics.Process { StartInfo = psi };
                if (!proc.Start())
                    return false;

                // Wait up to 10 seconds for generation
                if (!proc.WaitForExit(10000))
                {
                    try { proc.Kill(); } catch { }
                    return false;
                }

                // AudioLDM saves results under ./output/generation
                var outputGenDir = Path.Combine(workDir, "output", "generation");
                if (!Directory.Exists(outputGenDir))
                    return false;

                // Find the newest wav file
                var wavs = Directory.GetFiles(outputGenDir, "*.wav", SearchOption.AllDirectories);
                if (wavs.Length == 0)
                    return false;

                string newest = null;
                DateTime newestTime = DateTime.MinValue;
                foreach (var w in wavs)
                {
                    var t = File.GetLastWriteTimeUtc(w);
                    if (t > newestTime)
                    {
                        newestTime = t;
                        newest = w;
                    }
                }

                if (newest == null)
                    return false;

                // Ensure destination directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                File.Copy(newest, targetFilePath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                // Log and fallback to beep
                Console.WriteLine($"AudioLDM generation failed for {soundName}: {ex.Message}");
                return false;
            }
        }

        private int GetFrequencyForSound(string soundName)
        {
            return soundName switch
            {
                "button_click" => 800,
                "game_start" => 1000,
                "answer_select" => 600,
                "final_answer" => 1200,
                "correct_answer" => 1500,
                "wrong_answer" => 300,
                "lifeline_use" => 900,
                "walk_away" => 700,
                "game_over" => 400,
                "victory" => 2000,
                _ => 800
            };
        }

        private void CreateBeepSound(string filePath, int frequency)
        {
            try
            {
                // Create a simple WAV file with a beep sound
                var duration = 500; // milliseconds
                var sampleRate = 44100;
                var samples = duration * sampleRate / 1000;
                var amplitude = 0.3;

                using var fileStream = new FileStream(filePath, FileMode.Create);
                using var writer = new BinaryWriter(fileStream);

                // WAV header
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples * 2);

                // Generate sine wave
                for (int i = 0; i < samples; i++)
                {
                    var sample = (short)(amplitude * short.MaxValue * Math.Sin(2 * Math.PI * frequency * i / sampleRate));
                    writer.Write(sample);
                }
            }
            catch (Exception ex)
            {
                // If we can't create the sound file, just continue without it
                Console.WriteLine($"Could not create sound file {filePath}: {ex.Message}");
            }
        }

        public void PlaySound(string soundName)
        {
            if (!_soundEnabled || !_soundFiles.ContainsKey(soundName))
                return;

            try
            {
                var soundFile = _soundFiles[soundName];
                if (!File.Exists(soundFile))
                    return;

                // Dispose existing player if it exists
                if (_soundPlayers.ContainsKey(soundName))
                {
                    _soundPlayers[soundName].Stop();
                    _soundPlayers[soundName].Close();
                    _soundPlayers.Remove(soundName);
                }

                // Create new player
                var player = new MediaPlayer();
                player.Open(new Uri(soundFile));
                player.Volume = 0.5;
                player.Play();

                _soundPlayers[soundName] = player;

                // Clean up after playing
                player.MediaEnded += (s, e) =>
                {
                    player.Close();
                    if (_soundPlayers.ContainsKey(soundName))
                        _soundPlayers.Remove(soundName);
                };
            }
            catch (Exception ex)
            {
                // Silently handle audio errors
                Console.WriteLine($"Error playing sound {soundName}: {ex.Message}");
            }
        }

        public void StopAllSounds()
        {
            foreach (var player in _soundPlayers.Values)
            {
                try
                {
                    player.Stop();
                    player.Close();
                }
                catch
                {
                    // Ignore errors when stopping
                }
            }
            _soundPlayers.Clear();
        }

        public void SetVolume(double volume)
        {
            volume = Math.Max(0, Math.Min(1, volume)); // Clamp between 0 and 1
            
            foreach (var player in _soundPlayers.Values)
            {
                try
                {
                    player.Volume = volume;
                }
                catch
                {
                    // Ignore errors when setting volume
                }
            }
        }

        public void Dispose()
        {
            StopAllSounds();
        }
    }
}