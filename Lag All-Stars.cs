using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

public class MoveConsole
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    const int MONITOR_DEFAULTTOPRIMARY = 1;

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        public static MONITORINFO Default
        {
            get { var inst= new MONITORINFO(); inst.cbSize = (uint)Marshal.SizeOf(inst); return inst; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x, y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    const uint SW_RESTORE= 9;

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWPLACEMENT
    {
        public uint Length;
        public uint Flags;
        public uint ShowCmd;
        public POINT MinPosition;
        public POINT MaxPosition;
        public RECT NormalPosition;
        public static WINDOWPLACEMENT Default
        {
            get
            {
                var instance = new WINDOWPLACEMENT();
                instance.Length = (uint) Marshal.SizeOf(instance);
                return instance;
            }
        }
    }

    public void move()
    {
        IntPtr hWnd = GetConsoleWindow();
        var mi = MONITORINFO.Default;
        GetMonitorInfo(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);

        var wp = WINDOWPLACEMENT.Default;
        GetWindowPlacement(hWnd, ref wp);

        wp.NormalPosition = new RECT() {
            Left = 2265,
            Top = 578,
            Right = 2830,
            Bottom = 750
        };

        SetWindowPlacement(hWnd, ref wp);
    }
}

public class Program
{
    public int[] time = {0, 0, 0};

    public void startTime()
    {
        while (3 < 4)
        {
            if (time[0] == 59)
            {
                time[0] = -1;
                time[1]++;
            }

            if (time[1] == 59)
            {
                time[1] = 0;
                time[2]++;
            }

            time[0]++;
            Thread.Sleep(1000);
        }
    }

    static void Main(String[] args)
    {
        //Setup
        MoveConsole window = new MoveConsole();
        window.move();

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        Console.WindowHeight = 8;
        Console.BufferHeight = 8;
        Console.WindowWidth = 68;
        Console.BufferWidth = 68;
        Console.Title = "Lag All-Stars";

        Program app = new Program();
        var tiempo = new Thread(app.startTime);

        tiempo.Start();
        
        List<long> query = new List<long>();
        List<long> lastMin = new List<long>();

        int[] stat = {0, 0, 0, 0};
        long step = 0;
        long sum = 0;
        long prom = 0;

        int signal = 60;
        String[] loading = {"▀ ", " ▀", " ▄", "▄ "};
        int loadCount = 0;
        
        char txt = '0';
        ConsoleColor color = ConsoleColor.White;

        //Asignar IP
        Ping pingSender = new Ping();
        string host;

        if (args.Length == 0)
            host = "8.8.8.8";
        
        else
            host = args[0];

        //App
        while (3 < 4)
        {
            PingReply reply = pingSender.Send(host);

            //Promedio
            sum = 0;          
            if (step < 60)
                step++;

            else
                lastMin.RemoveAt(0);

            lastMin.Add(reply.RoundtripTime);

            foreach (long data in lastMin)
            {
                sum += data;
                
                if (data < 100)
                    stat[0]++;

                if (data >= 100 && data < 200)
                    stat[1]++;

                if (data > 200)
                    stat[2]++;

                if (data == 0)
                    stat[3]++;
            }

            prom = sum / step;

            if (prom < 100)
                color = ConsoleColor.Green;

            if (prom >= 100 && prom < 200)
                color = ConsoleColor.DarkYellow;

            if (prom > 200)
                color = ConsoleColor.Red;
                
            Console.ForegroundColor = color;
            Console.SetCursorPosition(0,0);
            Console.Write(String.Format("{0,4}ms", prom));
        
            //Tiempo Transcurrido
            Console.ResetColor();
            Console.Write(String.Format("│ {0,2}:{1,2}:{2,2} │", app.time[2], app.time[1], app.time[0]));

            //Estadisticas
            Console.SetCursorPosition(17,0);
            Console.Write(String.Format("│ {0,6} │ {1,6} │ {2,6} │ {3,6} ", stat[0], stat[1], stat[2], stat[3]));

            Console.SetCursorPosition(19,0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("▓▓");
            Console.ResetColor();
            Console.Write(": ");

            Console.SetCursorPosition(28,0);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("▒▒");
            Console.ResetColor();
            Console.Write(": ");

            Console.SetCursorPosition(37,0);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("░░");
            Console.ResetColor();
            Console.Write(": ");

            Console.SetCursorPosition(46,0);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("▬ ");
            Console.ResetColor();
            Console.Write(": ");

            //Signal Level
            Console.SetCursorPosition(53,0);
            Console.ResetColor();
            Console.Write("│ ╒");

            if (step < 60)
            {
                Console.Write(" {0} ", loading[loadCount]);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(String.Format("{0,2}%", (step*100)/60));

                Console.ResetColor();
                Console.Write("|");

                loadCount++;

                if (loadCount == 4)
                    loadCount = 0;
            }

            else
            {
                signal -= Convert.ToInt32(stat[1]*0.5);
                signal -= stat[2];
                signal -= stat[3]*2;

                if (signal >= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("_");

                    if (signal >= 20)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("▄");
                        
                        if (signal >= 40)
                        {
                            if (signal >= 50)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("█");
                            }
                            
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write("▌");
                            }
                        }

                        else 
                            Console.Write(" ");
                    }
                    
                    else 
                        Console.Write("  ");
                }

                else
                    Console.Write("   ");

                Console.ResetColor();
                Console.Write(String.Format("{0,3}%│", (signal*100)/60));
            }

            stat[0] = 0;
            stat[1] = 0;
            stat[2] = 0;
            stat[3] = 0;
            signal = 60;

            Console.SetCursorPosition(0,1);
            Console.WriteLine("──────┼──────────┴────────┴────────┴────────┴────────┴─────────┘");
            
            //Pings
            if (reply.Status == IPStatus.Success)
                query.Add(reply.RoundtripTime);
            
            else
                query.Add(-1);

            //Limpiar buffer
            for (int k = 0; k < 5; k++)
            {
                Console.SetCursorPosition(7,2+k);
                Console.Write("                                                          ");
            }

            //Imprimir
            Console.SetCursorPosition(0,2);
            foreach (long data in query)
            {
                if (data != -1)
                {
                    if (data < 100)
                    {
                        color = ConsoleColor.Green;
                        txt = '▓';
                    }

                    if (data >= 100 && data < 200)
                    {
                        color = ConsoleColor.DarkYellow;
                        txt = '▒';
                    }

                    if (data > 200)
                    {
                        color = ConsoleColor.Red;
                        txt = '░';
                    }
                    
                    Console.ForegroundColor = color;
                    Console.Write(String.Format("{0,4}ms", data));
                    
                    Console.ResetColor();
                    Console.Write("│");

                    Console.ForegroundColor = color;
                    for (int k = 0; k < data / 10; k++)
                    {
                        Console.Write(txt);

                        if (k > 54)
                            break;
                    }
                    
                    Console.WriteLine();
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(String.Format("{0,6}", "▬ "));

                    Console.ResetColor();
                    Console.Write("│ ");

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Error.");
                }
            }

            if (step > 4)
                query.RemoveAt(0);

            if (reply.RoundtripTime < 1000 && reply.Status == IPStatus.Success)
                Thread.Sleep(Convert.ToInt32(1000-reply.RoundtripTime));
        }
    }
}