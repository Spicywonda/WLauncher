using Avalonia;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WLauncher
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppId);

        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        public static int Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    SetCurrentProcessExplicitAppUserModelID("Spicywonda.WLauncher");
                }
                catch { }
            }

            try
            {
                if (args.Length > 0 && args[0].StartsWith("-"))
                {
                    if (OperatingSystem.IsWindows())
                    {
                        if (AttachConsole(ATTACH_PARENT_PROCESS))
                        {
                            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
                        }
                    }

                    int exitCode = RunCLI(args);

                    if (OperatingSystem.IsWindows())
                    {
                        FreeConsole();
                    }

                    return exitCode;
                }

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                return 0;
            }
            catch (Exception ex)
            {
                if (OperatingSystem.IsWindows())
                {
                    MessageBox(IntPtr.Zero, $"Critical crash in Main: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "WLauncher Critical Crash", 0x10);
                }
                else
                {
                    Console.Error.WriteLine($"Critical Crash: {ex.Message}");
                }
                return 1;
            }
        }

        private static int RunCLI(string[] args)
        {
            try
            {
                var cliHandler = new CLIHandler();
                return cliHandler.Execute(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (OperatingSystem.IsWindows())
                {
                    MessageBox(IntPtr.Zero, $"RunCLI Exception: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "WLauncher RunCLI Exception", 0x10);
                }
                else
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
                return 1;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}