using System.Runtime.InteropServices;

namespace Application.Services;

public class MediaService
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int keybd_event(
        byte bVk,
        byte bScan,
        uint dwFlags,
        int dwExtraInfo);

    public void PlayPause()
    {
        const byte playPauseKey = 0xB3;
        keybd_event(playPauseKey, 0, 0, 0); // Нажатие клавиши
        keybd_event(playPauseKey, 0, 2, 0); // Отпускание клавиши
    }

    public void Next()
    {
        const byte nextKey = 0xB0;     // Код клавиши Next
        keybd_event(nextKey, 0, 0, 0); // Нажатие клавиши
        keybd_event(nextKey, 0, 2, 0); // Отпускание клавиши
    }

    public void Previous()
    {
        const byte previousKey = 0xB1;     // Код клавиши Previous
        keybd_event(previousKey, 0, 0, 0); // Нажатие клавиши
        keybd_event(previousKey, 0, 2, 0); // Отпускание клавиши
    }

    public void Stop()
    {
        const byte stopKey = 0xB2;     // Код клавиши Stop
        keybd_event(stopKey, 0, 0, 0); // Нажатие клавиши
        keybd_event(stopKey, 0, 2, 0); // Отпускание клавиши
    }
}