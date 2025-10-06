using System;
using System.Windows;
using System.Windows.Media;

namespace DominoGame.Helpers
{
    public static class SoundManager
    {
        private static MediaPlayer backgroundPlayer;
        private static MediaPlayer sfxPlayer;

        private static double lastVolume = 0.3; // Store last non-zero volume for mute toggle

        public static void PlayBackgroundMusic(string fileName, double volume = 0.3)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", fileName);

                backgroundPlayer = new MediaPlayer();
                backgroundPlayer.Open(new Uri(fullPath, UriKind.Absolute));
                backgroundPlayer.Volume = volume;
                lastVolume = volume;

                backgroundPlayer.MediaEnded += (s, e) =>
                {
                    backgroundPlayer.Position = TimeSpan.Zero;
                    backgroundPlayer.Play();
                };

                backgroundPlayer.MediaFailed += (s, e) =>
                {
                    MessageBox.Show($"Media Error: {e.ErrorException.Message}\nPath: {fullPath}");
                };

                backgroundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to play sound: {ex.Message}");
            }
        }

        public static void StopBackgroundMusic()
        {
            backgroundPlayer?.Stop();
        }

        public static void PlaySfx(string fileName, double volume = 1.0)
        {
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", fileName);

            sfxPlayer = new MediaPlayer();
            sfxPlayer.Open(new Uri(fullPath, UriKind.Absolute));
            sfxPlayer.Volume = volume;
            sfxPlayer.Play();
        }

        // ✅ NEW: Set volume dynamically
        public static void SetBackgroundVolume(double volume)
        {
            if (backgroundPlayer != null)
            {
                backgroundPlayer.Volume = volume;
                if (volume > 0)
                    lastVolume = volume; // remember last level
            }
        }

        // ✅ NEW: Get current volume
        public static double GetBackgroundVolume()
        {
            return backgroundPlayer?.Volume ?? 0.0;
        }

        // ✅ NEW: Mute toggle
        public static void MuteBackground(bool mute)
        {
            if (mute)
                SetBackgroundVolume(0);
            else
                SetBackgroundVolume(lastVolume);
        }
    }
}
