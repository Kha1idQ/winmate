using System.Runtime.InteropServices;

namespace WinMate.Services;

// Windows' Restart Manager (rstrtmgr.dll) — the supported way to find and close
// whatever holds a file open. Used to free Edge's locked DLLs (held by SearchHost,
// Widgets and WebView hosts) before deleting them, instead of blindly killing
// protected processes.
public static class RestartManager
{
    // Returns the names of the processes that were holding the given files, after
    // asking Restart Manager to shut them down. Windows auto-restarts the ones it
    // registered for restart (Start-menu search, Widgets), so this is safe.
    public static IReadOnlyList<string> FreeFiles(IReadOnlyList<string> files, Action<string> onLine)
    {
        if (files.Count == 0)
            return [];

        var sessionKey = new string('\0', CchSessionKey + 1);
        if (RmStartSession(out var session, 0, sessionKey) != 0)
            return [];

        var closed = new List<string>();
        try
        {
            var array = files.ToArray();
            if (RmRegisterResources(session, (uint)array.Length, array, 0, null, 0, null) != 0)
                return [];

            uint procInfoNeeded = 0, procInfo = 0, rebootReasons = 0;
            var result = RmGetList(session, out procInfoNeeded, ref procInfo, null, ref rebootReasons);

            if (result == ErrorMoreData && procInfoNeeded > 0)
            {
                var infos = new RmProcessInfo[procInfoNeeded];
                procInfo = procInfoNeeded;
                if (RmGetList(session, out procInfoNeeded, ref procInfo, infos, ref rebootReasons) == 0)
                {
                    for (var i = 0; i < procInfo; i++)
                        closed.Add(infos[i].strAppName);

                    if (closed.Count > 0)
                        onLine($"Closing file holders via Restart Manager: {string.Join(", ", closed.Distinct())}");
                }
            }

            // Ask Windows to shut down every registered holder and force it if needed.
            RmShutdown(session, RmForceShutdown, IntPtr.Zero);
        }
        finally
        {
            RmEndSession(session);
        }

        return closed;
    }

    private const int CchSessionKey = 32;
    private const int ErrorMoreData = 234;
    private const uint RmForceShutdown = 0x1;

    [StructLayout(LayoutKind.Sequential)]
    private struct RmUniqueProcess
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private const int AppNameLen = 255;
    private const int SvcNameLen = 63;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RmProcessInfo
    {
        public RmUniqueProcess Process;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = AppNameLen + 1)]
        public string strAppName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SvcNameLen + 1)]
        public string strServiceShortName;
        public int ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmStartSession(out uint sessionHandle, int flags, string sessionKey);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(uint session, uint files, string[] fileNames,
        uint applications, RmUniqueProcess[]? applicationArray, uint services, string[]? serviceNames);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(uint session, out uint procInfoNeeded,
        ref uint procInfo, RmProcessInfo[]? processInfo, ref uint rebootReasons);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmShutdown(uint session, uint flags, IntPtr statusCallback);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint session);
}
