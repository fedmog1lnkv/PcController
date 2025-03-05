using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;

namespace Application.Services
{
    public class VolumeService
    {
        public void SetVolume(int volume)
        {
            if (volume < 0 || volume > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(volume), "Громкость должна быть от 0 до 100.");
            }
            if (OperatingSystem.IsWindows())
            {
                SetVolumeWindows(volume);
            }
            else if (OperatingSystem.IsLinux())
            {
                SetVolumeLinux(volume);
            }
            else
            {
                throw new NotSupportedException("Операционная система не поддерживается.");
            }
        }
        
        public int GetCurrentVolume()
        {
            if (OperatingSystem.IsWindows())
            {
                return GetCurrentVolumeWindows();
            }
            
            if (OperatingSystem.IsLinux())
            {
                return GetCurrentVolumeLinux();
            }
            throw new NotSupportedException("Операционная система не поддерживается.");
        }
        
        private int GetCurrentVolumeWindows()
        {
            var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return (int)Math.Round(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }
        
        private int GetCurrentVolumeLinux()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pactl",
                    Arguments = "get-sink-volume @DEFAULT_SINK@",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var match = System.Text.RegularExpressions.Regex.Match(output, @"\b(\d{1,3})%\b");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
            {
                return volume;
            }

            throw new Exception("Не удалось получить уровень громкости.");
        }

        private void SetVolumeWindows(int volume)
        {
            var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
        }

        private void SetVolumeLinux(int volume)
        {
            throw new NotImplementedException();
            Process.Start("amixer", $"sset 'Master' {volume}%");
        }

    }
}
