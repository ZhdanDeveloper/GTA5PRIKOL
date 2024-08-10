using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;




public class KeyPresser
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    private const int INPUT_KEYBOARD = 1;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const ushort VK_A = 0x41; // Virtual-Key code for 'A'
    private const ushort VK_D = 0x44; // Virtual-Key code for 'D'
    private const ushort VK_SPACE = 0x20; // Virtual-Key code for 'Space'

    public void PressA()
    {
        INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_A,
                        dwFlags = 0 // Key down
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void ReleaseA()
    {
        INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_A,
                        dwFlags = KEYEVENTF_KEYUP // Key up
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void PressSpace()
    {
        INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_SPACE,
                        dwFlags = 0 // Key down
                    }
                }
            },
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_SPACE,
                        dwFlags = KEYEVENTF_KEYUP // Key up
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void PressD()
    {
        INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_D,
                        dwFlags = 0 // Key down
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void ReleaseD()
    {
        INPUT[] inputs = new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = VK_D,
                        dwFlags = KEYEVENTF_KEYUP // Key up
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void PressKeyWithScanCode(ushort scanCode)
    {
        INPUT[] inputs = new INPUT[]
        {
        new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wScan = scanCode,
                    dwFlags = 0x0008 // KEYEVENTF_SCANCODE
                }
            }
        }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public void ReleaseKeyWithScanCode(ushort scanCode)
    {
        INPUT[] inputs = new INPUT[]
        {
        new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wScan = scanCode,
                    dwFlags = 0x0008 | 0x0002 // KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP
                }
            }
        }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}

class Program
{
    static Bitmap previoustrackScreenshot;
    static readonly int screenWidth = 1920;
    static readonly int screenHeight = 1080;
    static readonly int centerRegionSize = 10; // Size of the area around the center for comparison
    static readonly int moveThreshold = 1; // Minimum movement threshold for detecting direction
    static bool isAHeld = false;
    static bool isDHeld = false;
    static void Main()
    {


        KeyPresser keyPresser = new KeyPresser();
        // Примерные координаты и размеры области шкалы
        int x = 650;  // Начальная координата x (примерно 650 пикселей от левого края)
        int y = 896;  // Начальная координата y (примерно 960 пикселей от верхнего края)
        int width = 600;  // Ширина области (примерно 600 пикселей)
        int height = 40;  // Высота области (примерно 40 пикселей)

        int frameCount = 0;
        int previousGreenZoneWidth = 0;

        while (true)
        {
            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                }

                bool greenZoneFound = false;
                Rectangle greenZoneRect = new Rectangle();

                // Ищем зеленую зону
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (IsInGreenZone(bmp, i, j))
                        {
                            if (!greenZoneFound)
                            {
                                greenZoneFound = true;
                                greenZoneRect.X = i;
                                greenZoneRect.Y = j;
                            }

                            // Расширяем прямоугольник, включающий все зеленые пиксели
                            greenZoneRect.Width = Math.Max(greenZoneRect.Width, i - greenZoneRect.X);
                            greenZoneRect.Height = Math.Max(greenZoneRect.Height, j - greenZoneRect.Y);
                        }
                    }
                }

                if (greenZoneFound)
                {
                    int currentGreenZoneWidth = greenZoneRect.Width;

                    // Проверка на уменьшение зеленой зоны
                    if (previousGreenZoneWidth > 0 && currentGreenZoneWidth < previousGreenZoneWidth)
                    {        

                        Console.WriteLine($"detect");
                        gotoinf(keyPresser);
                        frameCount++;

                    }

                    // Обновляем ширину зеленой зоны
                    previousGreenZoneWidth = currentGreenZoneWidth;
                }
                else
                {
                    //Console.WriteLine("Зеленая зона не найдена.");
                }

            }
        }
    }
    static void gotoinf(KeyPresser x)
    {
        Console.Beep();
        x.PressSpace();
        Thread.Sleep(9000);
        CaptureScreenshotAroundPoint(1394, 880, 25, 25, x);

        while (true) { }

    }


    public static void CaptureScreenshotAroundPoint(int x, int y, int width, int height, KeyPresser presser)
    {

        Bitmap previousScreenshot = null;

        while (true)
        {
            using (Bitmap currentScreenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(currentScreenshot))
                {
                    g.CopyFromScreen(x - width / 2, y - height / 2, 0, 0, currentScreenshot.Size, CopyPixelOperation.SourceCopy);
                }

                if (previousScreenshot == null)
                {
                    // Инициализация: сохраняем первый скриншот и пропускаем его из сравнения
                    previousScreenshot = new Bitmap(currentScreenshot);
                    Console.WriteLine("First screenshot taken, will not be compared.");
                }
                else if (ImagesAreDifferent(previousScreenshot, currentScreenshot))
                {
                    Console.WriteLine("Pixel color change detected.");
                    presser.PressSpace();
                    Console.Beep();
                    traaack();
                    SaveScreenshot(currentScreenshot, x, y);
                    previousScreenshot.Dispose();
                    previousScreenshot = new Bitmap(currentScreenshot); // Сохраняем текущее состояние как предыдущее
                    while (true) { }
                }
                else
                {
                    Console.WriteLine("No changes detected.");
                }

                Thread.Sleep(1000); // Задержка 1 секунда между проверками
            }
        }

    }
    private static void SaveScreenshot(Bitmap bmp, int x, int y)
    {
        string filename = $"changed_square_{x}_{y}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        bmp.Save(filename, ImageFormat.Png);
        Console.WriteLine($"Скриншот сохранен: {filename}");
    }
    private static bool ImagesAreDifferent(Bitmap bmp1, Bitmap bmp2)
    {
        for (int i = 0; i < bmp1.Width; i++)
        {
            for (int j = 0; j < bmp1.Height; j++)
            {
                if (bmp1.GetPixel(i, j) != bmp2.GetPixel(i, j))
                {
                    return true; // Если хотя бы один пиксель отличается, возвращаем true
                }
            }
        }
        return false;
    }

    static bool IsInGreenZone(Bitmap bmp, int x, int y)
    {
        // Улучшенные условия для определения зеленого цвета
        Color pixelColor = bmp.GetPixel(x, y);
        return pixelColor.G > 150 && pixelColor.R < 100 && pixelColor.B < 100;
    }


    static void traaack()
    {
        KeyPresser keyPresser = new KeyPresser();
        Rectangle screenArea = new Rectangle(0, 0, 1920, 1080); // разрешение экрана
        Bitmap previousTRACKScreenshot = null;

        while (true)
        {
            // Захват текущего экрана
            Bitmap currentScreenshot = CaptureScreen(screenArea);

            // Преобразуем в формат Mat для OpenCV
            Mat currentFrame = BitmapConverter.ToMat(currentScreenshot);

            // Находим центр текущего изображения
            System.Drawing.Point currentCenter = new System.Drawing.Point(currentFrame.Width / 2, currentFrame.Height / 2);

            if (previousTRACKScreenshot != null)
            {
                // Преобразуем предыдущий кадр в формат Mat
                Mat previousFrame = BitmapConverter.ToMat(previousTRACKScreenshot);

                // Вычисляем разницу между кадрами
                Mat diff = new Mat();
                Cv2.Absdiff(previousFrame, currentFrame, diff);

                // Анализируем смещение центра
                System.Drawing.Point movement = CalculateMovement(previousFrame, currentFrame);

                if (movement.X > 0 && !isDHeld)
                {
                    if (isAHeld)
                    {
                        keyPresser.ReleaseKeyWithScanCode(0x1E);
                        isAHeld = false;
                    }

                    keyPresser.PressKeyWithScanCode(0x20);
                    isDHeld = true;
                    Console.WriteLine("left");
                    Console.Beep(1000, 300);
                }
                else if (movement.X < 0 && !isAHeld)
                {
                    if (isDHeld)
                    {
                        keyPresser.ReleaseKeyWithScanCode(0x20); // Отпускание 'D'
                        isDHeld = false;
                    }

                    keyPresser.PressKeyWithScanCode(0x1E);
                    isAHeld = true;
                    Console.WriteLine("right");
                    Console.Beep(500, 500);
                }
            }

            // Сохраняем текущий кадр как предыдущий
            if (previousTRACKScreenshot != null)
            {
                previousTRACKScreenshot.Dispose();
            }
            previousTRACKScreenshot = currentScreenshot;

            // Задержка для обновления кадров (например, 100ms)
            //System.Threading.Thread.Sleep(0);
        }

        static Bitmap CaptureScreen(Rectangle area)
        {
            Bitmap bmp = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(area.Left, area.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }

        static System.Drawing.Point CalculateMovement(Mat previousFrame, Mat currentFrame)
        {
            // Простой пример: сравнение разницы пикселей по оси X
            Mat grayPrev = new Mat();
            Mat grayCurr = new Mat();

            Cv2.CvtColor(previousFrame, grayPrev, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(currentFrame, grayCurr, ColorConversionCodes.BGR2GRAY);

            // Используем функцию для поиска смещения
            Mat flow = new Mat();
            Cv2.CalcOpticalFlowFarneback(grayPrev, grayCurr, flow, 0.5, 3, 15, 3, 5, 1.2, 0);

            // Усредняем смещение по всему изображению
            System.Drawing.Point movement = new System.Drawing.Point(0, 0);
            for (int y = 0; y < flow.Rows; y++)
            {
                for (int x = 0; x < flow.Cols; x++)
                {
                    Vec2f flowAtPoint = flow.At<Vec2f>(y, x);
                    movement.X += (int)flowAtPoint[0];
                    movement.Y += (int)flowAtPoint[1];
                }
            }

            // Нормализуем движение
            movement.X /= flow.Rows * flow.Cols;
            movement.Y /= flow.Rows * flow.Cols;

            return movement;
        }
    }
}

