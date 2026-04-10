using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

static public class WinOSController
{
    #region disable keyboard mouse
    static private bool IsInputDisabled => keyboardHook != IntPtr.Zero && mouseHook != IntPtr.Zero;
    static private IntPtr keyboardHook;
    static private IntPtr mouseHook;

    [DllImport("user32.dll")] static private extern IntPtr SetWindowsHookEx(int idHook, Func<int, IntPtr, IntPtr, IntPtr> lpfn, IntPtr hMod, ushort dwThreadId);
    [DllImport("user32.dll")] static private extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")] static private extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// Intercept input messages sent to the OS to disable keyboard (and mouse optionally) system-globally.
    /// </summary>
    /// <param name="disableMouse">Disable mouse input or not.</param>
    static public void DisableInput(bool disableMouse = false)
    {
        if (keyboardHook == IntPtr.Zero)
            keyboardHook = SetWindowsHookEx(13, (int _, IntPtr _, IntPtr _) => (IntPtr)1, IntPtr.Zero, 0);
        if (disableMouse && mouseHook == IntPtr.Zero)
            mouseHook = SetWindowsHookEx(14, (int _, IntPtr _, IntPtr _) => (IntPtr)1, IntPtr.Zero, 0);
    }

    /// <summary>
    /// Recover keyboard and mouse input from DisableInput()
    /// </summary>
    static public void EnableInput()
    {
        if (keyboardHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(keyboardHook);
            keyboardHook = IntPtr.Zero;
        }
        if (mouseHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(mouseHook);
            mouseHook = IntPtr.Zero;
        }
    }
#endregion

    #region kill Task Manager
    static public void KillTaskmgr()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-command Get-Process -Name taskmgr | Stop-Process",
            CreateNoWindow = true,
            UseShellExecute = false,
        });
    }
    #endregion

    #region make computer sleep
    /// <summary>
    /// Suspend the OS after calling EnableInput() and UnlockVolume()
    /// </summary>
    static public void Sleep()
    {
        EnableInput();
        UnlockVolume();
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments =
            "[Void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms');" +
            "[System.Windows.Forms.Application]::SetSuspendState('Suspend', $true, $true);",
            CreateNoWindow = true,
            UseShellExecute = false,
        });
    }
    #endregion

    #region lock system volume
    static public float LockedVolume { get; private set; }
    static private string systemSpeakerID;
    static private Thread volumeLock;
    static private MMDeviceEnumerator deviceEnumerator;

    static public void LockVolume(float targetVolumeLevelScalar)
    {
        if (null != deviceEnumerator)
            return;
        deviceEnumerator = new MMDeviceEnumerator();

        volumeLock = new Thread(() =>
        {
            while (null != deviceEnumerator)
            {
                var defalutSpeaker = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                var v = defalutSpeaker.AudioEndpointVolume;
                if (systemSpeakerID != defalutSpeaker.ID)
                {
                    var vs = v.MasterVolumeLevelScalar;
                    LockedVolume = vs < targetVolumeLevelScalar || v.Mute ? targetVolumeLevelScalar : vs;
                    systemSpeakerID = defalutSpeaker.ID;
                }
                v.MasterVolumeLevelScalar = LockedVolume;
                v.Mute = false;
            }
        });
        volumeLock.Start();
    }

    static public void UnlockVolume()
    {
        deviceEnumerator = null;
        volumeLock?.Join();
        systemSpeakerID = null;
        LockedVolume = 0f;
    }
    #endregion
}
