using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public delegate bool WNDENUMPROC(IntPtr hwnd, uint lParam);
public delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref MONITORRECT pRect, int dwData);

[StructLayout(LayoutKind.Sequential)]
public struct MONITORRECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
public class WindowSetting
{

    //private openglwin5v2 oglWin;
    [DllImport("user32.dll")]
    public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "EnumWindows", SetLastError = true)]
    public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, uint lParam);

    [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

    [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint uFlag);

    [DllImport("user32")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);


    public const int SW_MAXIMIZE = 3;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;

    public const int SWP_SHOWWINDOW = 0x0040;

    public const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;

    public struct MonitorInfo
    {
        /// <summary>
        /// 显示器的矩形区域，可以得出显示器的大小等信息。
        /// </summary>
        public MONITORRECT rect;
        /// <summary>
        /// 显示器是否是主显示器。
        /// </summary>
        public bool isPrimary;
        /// <summary>
        /// 显示器的句柄。
        /// </summary>
        public IntPtr monPtr;
    }

    /// <summary>
    /// 要获取的进程主窗口
    /// </summary>
    private static IntPtr hMainWnd;

    /// <summary>
    /// 主显示器的句柄
    /// </summary>
    private static IntPtr primaryPtr;

	private static List<MonitorInfo> m_monitorInfos;
    public static List<MonitorInfo> monitorInfos
	{
		get{
			if(m_monitorInfos == null || m_monitorInfos.Count <=0)
			{
				GetMonitors();

			}
			return m_monitorInfos;
		}
	}

	/// <summary>
	/// 获取到所有的显示器信息
	/// </summary>
	private static void GetMonitors()
	{
		if (primaryPtr == IntPtr.Zero)
			primaryPtr = MonitorFromWindow(IntPtr.Zero, MONITOR_DEFAULTTOPRIMARY);
		
		if (m_monitorInfos == null || m_monitorInfos.Count == 0)
		{
			MonitorEnumProc callback = MonitorEnumProcCallBack;
			if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0))
				UnityEngine.Debug.Log(string.Format("You have {0} monitors", monitorInfos.Count.ToString()));
			else
				UnityEngine.Debug.Log("An error occured while enumerating monitors");
		}
	}

    /// <summary>
    /// 设置进程窗口最大化
    /// </summary>
    /// <param name="processID"></param>
    public static void SetProcessWndMaximize(int processID)
    {
        hMainWnd = IntPtr.Zero;
        bool bResult = EnumWindows(new WNDENUMPROC(EnumWindowsProc), (uint)processID);
        //说明已经找到主窗口。
        if (!bResult && hMainWnd != IntPtr.Zero)
        {
            SetWindowToMonitor(hMainWnd);            
        }
    }

    /// <summary>
    /// 调用戴弦dll,设置裸眼3D/偏振投影窗口最小化
    /// </summary>
    public static void SetMinimize()
    {
        IntPtr hWin = openglwinDll.gethwnd();
        if(hWin != IntPtr.Zero)
            ShowWindow(hWin, SW_MINIMIZE);
    }

    /// <summary>
    /// 调用戴弦dll,设置裸眼3D/偏振投影窗口最大化
    /// </summary>
    public static void SetMaximize()
    {
        IntPtr hWin = openglwinDll.gethwnd();
        if (hWin != IntPtr.Zero)
            ShowWindow(hWin, SW_RESTORE);
    }

    /// <summary>
    /// 设置窗口到副屏上，并且最大化。
    /// 如果只有一个屏，则只在主屏上显示。
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    public static void SetWindowToMonitor(IntPtr hWnd)
    {
        if (monitorInfos == null || monitorInfos.Count == 0)
            return;

        MONITORRECT monRect = new MONITORRECT();
        if (monitorInfos.Count == 1)
            monRect = monitorInfos[0].rect;
        else
        {
            foreach (var item in monitorInfos)
            {
                if (!item.isPrimary)
                {
                    monRect = item.rect;
                    break;
                }
            }
        }

        //UnityEngine.Debug.Log(string.Format("left:{0},right:{1},top:{2},bottom:{3}", monRect.left, monRect.right, monRect.top, monRect.bottom));
        SetWindowPos(hWnd, 0, monRect.left, monRect.top, (int)(monRect.right- monRect.left), (int)(monRect.bottom - monRect.top), SWP_SHOWWINDOW);
        ShowWindow(hWnd, SW_MAXIMIZE);
    }

    /// <summary>
    /// WinAPI遍历所有的窗口获取到与我们指定的句柄相同的窗口
    /// </summary>
    private static bool EnumWindowsProc(IntPtr hwnd, uint lParam)
    {
        uint uiPid = 0;

        if (GetParent(hwnd) == IntPtr.Zero)
        {
            GetWindowThreadProcessId(hwnd, ref uiPid);
            if (uiPid == lParam)    // 找到进程对应的主窗口句柄
            {
                //processWnd[uiPid] = hwnd;   // 把句柄缓存起来
                //SetLastError(0);    // 设置无错误

                hMainWnd = hwnd;

                return false;   // 返回 false 以终止枚举窗口
            }
        }
        return true;
    }
    

    

    private static bool MonitorEnumProcCallBack(IntPtr hDesktop, IntPtr hdc, ref MONITORRECT pRect, int dwData)
    {
		if (m_monitorInfos == null)
			m_monitorInfos = new List<MonitorInfo>();

        MonitorInfo info = new MonitorInfo();
        info.rect = pRect;
        info.monPtr = hDesktop;

        info.isPrimary = (hDesktop == primaryPtr);
		m_monitorInfos.Add(info);

        return true;
    }
}
