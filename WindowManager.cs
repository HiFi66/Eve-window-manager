using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace EveLayoutManager
{
    public class WindowManager
    {
        private const string WindowTitlePrefix = "EVE - ";
        private readonly string _filePath;
        private Dictionary<string, Layout> _layouts = new Dictionary<string, Layout>();

        public WindowManager(string fileName)
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            LoadLayouts();
        }

        private void LoadLayouts()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _layouts = JsonSerializer.Deserialize<Dictionary<string, Layout>>(json) ?? new Dictionary<string, Layout>();
            }
            else
            {
                _layouts = new Dictionary<string, Layout>();
            }
        }

        public void SaveLayouts()
        {
            var windows = FindEveWindows();
            foreach (var item in windows)
            {
                var title = item.Key;
                var name = title.Substring(WindowTitlePrefix.Length);
                var rect = item.Value.Rect;
                _layouts[name] = new Layout
                {
                    X = rect.Left,
                    Y = rect.Top,
                    Width = rect.Right - rect.Left,
                    Height = rect.Bottom - rect.Top
                };
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_layouts, options));
        }

        public void CheckAndRestoreWindows()
        {
            var windows = FindEveWindows();
            foreach (var item in windows)
            {
                var title = item.Key;
                var name = title.Substring(WindowTitlePrefix.Length);
                if (_layouts.TryGetValue(name, out var layout))
                {
                    SetWindowPosition(item.Value.HWnd, layout);
                }
            }
        }

        private Dictionary<string, WindowInfo> FindEveWindows()
        {
            var result = new Dictionary<string, WindowInfo>();
            EnumWindows((hWnd, lParam) =>
            {
                var sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                if (!string.IsNullOrEmpty(title) && title.StartsWith(WindowTitlePrefix, StringComparison.Ordinal))
                {
                    GetWindowRect(hWnd, out RECT rect);
                    result[title] = new WindowInfo { HWnd = hWnd, Rect = rect };
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        private void SetWindowPosition(IntPtr hWnd, Layout layout)
        {
            SetWindowPos(hWnd, IntPtr.Zero, layout.X, layout.Y, layout.Width, layout.Height, SWP_NOZORDER);
        }

        #region P/Invoke Declarations
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        #endregion
    }

    public class WindowInfo
    {
        public IntPtr HWnd { get; set; }
        public RECT Rect { get; set; }
    }

    public class Layout
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
