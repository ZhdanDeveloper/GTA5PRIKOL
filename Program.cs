using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using Tesseract;
using System.Text;

public class KeyPresser
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);


    private const int INPUT_MOUSE = 0;
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

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

    public void ClickMouse(int x, int y)
    {
        // Move the mouse to the specified coordinates
        SetCursorPos(x, y);

        // Simulate mouse click
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
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
    static bool isDay = true;
    static int isHunter = 880;
    static bool take = true;
    int selectedIndex = 0;
    static bool firstTime = true;
    static KeyPresser presser = new KeyPresser();

    static void Main()
    {
        KeyPresser keyPresser = new KeyPresser();
        string[] tine_of_day = { "День", "Ночь" };
        string[] kind_of_fish = { "Хищная", "не хищная" };
        string[] take_or_leave = { "Забирать", "отпускать" };
        int selectedIndex = 0;
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        ConsoleKey key;

        if (firstTime == true)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Время суток :");

                DisplayMenu(tine_of_day, selectedIndex);

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? tine_of_day.Length - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == tine_of_day.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        ExecuteOptionTime(selectedIndex);
                        break;
                }

            } while (key != ConsoleKey.Enter);

            Console.Clear();
            selectedIndex = 0;
            do
            {
                Console.Clear();
                Console.WriteLine("Тип рыбы :");

                DisplayMenu(kind_of_fish, selectedIndex);

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? kind_of_fish.Length - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == kind_of_fish.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        ExecuteOptionKindOFish(selectedIndex);
                        break;
                }

            } while (key != ConsoleKey.Enter);

            selectedIndex = 0;
            do
            {
                Console.Clear();
                Console.WriteLine("Забирать или отпускать :");

                DisplayMenu(take_or_leave, selectedIndex);

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? kind_of_fish.Length - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == kind_of_fish.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        TakeOrLeave(selectedIndex);
                        break;
                }

            } while (key != ConsoleKey.Enter);

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Примечание: бот ловит рыбу 85 секунд, если он не успевает то прекращает тянуть рыбу и ждет нового заброса, если рыба поймана просто ждите и удочка закинеться автоматически");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Нажмите любую кнопку чтоб начать...");
            Console.ReadKey();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Бот работает, откройте ГТА и забросьте удочку, для появления шкалы с зеленой меткой");
            Console.ForegroundColor = ConsoleColor.White;

        }


        firstTime = false;

        


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

        y = isHunter;

        while (true)
        {
            using (Bitmap currentScreenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(currentScreenshot))
                {
                    g.CopyFromScreen(x - width / 2, y - height / 2, 0, 0, currentScreenshot.Size, CopyPixelOperation.SourceCopy);
                }
                if (isHunter != 990)
                {
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
                        previousScreenshot.Dispose();
                        previousScreenshot = new Bitmap(currentScreenshot); // Сохраняем текущее состояние как предыдущее
                    }
                    else
                    {
                        Console.WriteLine("No changes detected.");
                    }
                }
                else
                {
                    if (previousScreenshot == null)
                    {
                        // Инициализация: сохраняем первый скриншот и пропускаем его из сравнения
                        previousScreenshot = new Bitmap(currentScreenshot);
                        Console.WriteLine("First screenshot taken, will not be compared.");
                    }
                    else if (ContainsRedColor(currentScreenshot))
                    {
                        Console.WriteLine("Pixel color change detected.");

                        presser.PressSpace();
                        Console.Beep();
                        traaack();
                        previousScreenshot.Dispose();
                        previousScreenshot = new Bitmap(currentScreenshot); // Сохраняем текущее состояние как предыдущее
                    }
                    else
                    {
                        Console.WriteLine("No changes detected.");
                    }
                }

                Thread.Sleep(1000); // Задержка 1 секунда между проверками
            }
        }
    }

    private static bool ContainsRedColor(Bitmap bmp)
    {
        for (int i = 0; i < bmp.Width; i++)
        {
            for (int j = 0; j < bmp.Height; j++)
            {
                Color pixelColor = bmp.GetPixel(i, j);
                if (pixelColor.R > 150 && pixelColor.G < 100 && pixelColor.B < 100)
                {
                    return true; // Если найден красный цвет
                }
            }
        }
        return false; // Красный цвет не найден
    }

    private static void SaveScreenshot(Bitmap bmp, string filename)
    {
        string filepath = Path.Combine(Directory.GetCurrentDirectory(), filename);
        bmp.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"Screenshot saved: {filepath}");
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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (true)
        {
            // Захват текущего экрана
            Bitmap currentScreenshot = CaptureScreen(screenArea);

            // Преобразуем в формат Mat для OpenCV
            Mat currentFrame = BitmapConverter.ToMat(currentScreenshot);

            if (previousTRACKScreenshot != null)
            {
                // Преобразуем предыдущий кадр в формат Mat
                Mat previousFrame = BitmapConverter.ToMat(previousTRACKScreenshot);

                // Вычисляем разницу между кадрами
                Mat diff = new Mat();
                Cv2.Absdiff(previousFrame, currentFrame, diff);

                // Анализируем смещение центра

                System.Drawing.Point movement;

                if (isDay == true)
                {
                    movement = CalculateMovement(previousFrame, currentFrame);
                }
                else
                {
                    movement = CalculateMovementNight(previousFrame, currentFrame);
                }

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

                if (stopwatch.Elapsed.TotalSeconds >= 40)
                {
                    checkEND();
                }

                if (stopwatch.Elapsed.TotalSeconds >= 85)
                {
                    Main();
                }
            }

            // Сохраняем текущий кадр как предыдущий
            if (previousTRACKScreenshot != null)
            {
                previousTRACKScreenshot.Dispose();
            }
            previousTRACKScreenshot = currentScreenshot;
        }
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

    static void checkEND()
    {
        Bitmap screenshotBmp = CaptureFINISHScreenshot();

        // Поиск текста на скриншоте
        string recognizedText = RecognizeText(screenshotBmp);

        if (take)
        {
            if (recognizedText.Contains("Забрать себе"))
            {
                Console.WriteLine("found");
                Main();
            }
        }
        else
        {
            if (recognizedText.Contains("Отпустить"))
            {
                Console.WriteLine("found");
                presser.ClickMouse(977, 725);
                presser.ClickMouse(1224, 725);
                Thread.Sleep(500);
                Main();
            }
        }
       
        // Проверка наличия текста "Забрать себе"


        if (recognizedText.Contains("Рыба сорвалась"))
        {
            Console.WriteLine("found");
            Main();
        }
        else
        {
            Console.WriteLine("not found");
        }

        // Освобождение ресурсов
        screenshotBmp.Dispose();
    }

    static Bitmap CaptureFINISHScreenshot()
    {
        int screenWidth = 1920;  // Ваше разрешение экрана
        int screenHeight = 1080; // Ваше разрешение экрана
        Bitmap bitmap = new Bitmap(screenWidth, screenHeight);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        }
        return bitmap;
    }

    static string RecognizeText(Bitmap image)
    {
        string tessDataPath = @".\testdata"; // Убедитесь, что путь к tessdata указан правильно
        using var engine = new TesseractEngine(tessDataPath, "rus", EngineMode.Default);

        // Конвертация Bitmap в Pix
        using var img = ConvertBitmapToPix(image);
        using var page = engine.Process(img);
        return page.GetText();
    }

    static Pix ConvertBitmapToPix(Bitmap bitmap)
    {
        // Сохранение Bitmap в поток памяти как изображение BMP
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
        stream.Position = 0;

        // Конвертация потока в Pix с помощью Tesseract
        return Pix.LoadFromMemory(stream.ToArray());
    }


    // night
    static System.Drawing.Point CalculateMovement(Mat previousFrame, Mat currentFrame)
    {
        // Преобразуем в серый цвет для обоих кадров
        Mat grayPrev = new Mat();
        Mat grayCurr = new Mat();
        Cv2.CvtColor(previousFrame, grayPrev, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(currentFrame, grayCurr, ColorConversionCodes.BGR2GRAY);

        // Применяем выравнивание гистограммы для улучшения контраста
        Cv2.EqualizeHist(grayPrev, grayPrev);
        Cv2.EqualizeHist(grayCurr, grayCurr);

        // Применяем фильтр Гаусса для сглаживания и уменьшения шума
        Cv2.GaussianBlur(grayPrev, grayPrev, new OpenCvSharp.Size(3, 3), 0);
        Cv2.GaussianBlur(grayCurr, grayCurr, new OpenCvSharp.Size(3, 3), 0);

        // Настройка параметров для усиления детекции
        Mat flow = new Mat();
        Cv2.CalcOpticalFlowFarneback(
            grayPrev,
            grayCurr,
            flow,
            0.3,  // уменьшаем масштаб пирамиды для повышения чувствительности
            5,    // увеличиваем количество уровней пирамиды
            15,   // уменьшаем размер окна
            3,    // количество итераций алгоритма для каждого уровня пирамиды
            7,    // увеличиваем количество пикселей для усреднения
            1.5,  // вес усреднения
            0     // флаги
        );

        // Анализируем смещение по оси X (влево/вправо)
        System.Drawing.Point totalMovement = new System.Drawing.Point(0, 0);
        for (int y = 0; y < flow.Rows; y++)
        {
            for (int x = 0; x < flow.Cols; x++)
            {
                Vec2f flowAtPoint = flow.At<Vec2f>(y, x);
                totalMovement.X += (int)flowAtPoint[0];
                totalMovement.Y += (int)flowAtPoint[1];
            }
        }

        // Усредняем движение
        totalMovement.X /= (flow.Rows * flow.Cols);
        totalMovement.Y /= (flow.Rows * flow.Cols);

        // Добавляем порог, чтобы реагировать только на значительные изменения
        const int movementThreshold = 1;  // чувствительность
        if (Math.Abs(totalMovement.X) < movementThreshold) totalMovement.X = 0;
        if (Math.Abs(totalMovement.Y) < movementThreshold) totalMovement.Y = 0;

        return totalMovement;
    }


    //static System.Drawing.Point CalculateMovement(Mat previousFrame, Mat currentFrame)
    //{
    //    // Простой пример: сравнение разницы пикселей по оси X
    //    Mat grayPrev = new Mat();
    //    Mat grayCurr = new Mat();

    //    Cv2.CvtColor(previousFrame, grayPrev, ColorConversionCodes.BGR2GRAY);
    //    Cv2.CvtColor(currentFrame, grayCurr, ColorConversionCodes.BGR2GRAY);

    //    // Используем функцию для поиска смещения
    //    Mat flow = new Mat();
    //    Cv2.CalcOpticalFlowFarneback(grayPrev, grayCurr, flow, 0.5, 3, 15, 3, 5, 1.2, 0);

    //    // Усредняем смещение по всему изображению
    //    System.Drawing.Point movement = new System.Drawing.Point(0, 0);
    //    for (int y = 0; y < flow.Rows; y++)
    //    {
    //        for (int x = 0; x < flow.Cols; x++)
    //        {
    //            Vec2f flowAtPoint = flow.At<Vec2f>(y, x);
    //            movement.X += (int)flowAtPoint[0];
    //            movement.Y += (int)flowAtPoint[1];
    //        }
    //    }

    //    // Нормализуем движение
    //    movement.X /= flow.Rows * flow.Cols;
    //    movement.Y /= flow.Rows * flow.Cols;

    //    return movement;
    //}


    //dat
    static System.Drawing.Point CalculateMovementNight(Mat previousFrame, Mat currentFrame)
    {
        // Преобразуем в серый цвет
        Mat grayPrev = new Mat();
        Mat grayCurr = new Mat();
        Cv2.CvtColor(previousFrame, grayPrev, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(currentFrame, grayCurr, ColorConversionCodes.BGR2GRAY);

        // Применяем выравнивание гистограммы
        Cv2.EqualizeHist(grayPrev, grayPrev);
        Cv2.EqualizeHist(grayCurr, grayCurr);

        // Применяем фильтр Гаусса для сглаживания
        Cv2.GaussianBlur(grayPrev, grayPrev, new OpenCvSharp.Size(5, 5), 0);
        Cv2.GaussianBlur(grayCurr, grayCurr, new OpenCvSharp.Size(5, 5), 0);

        // Вычисляем оптический поток
        Mat flow = new Mat();
        Cv2.CalcOpticalFlowFarneback(grayPrev, grayCurr, flow, 0.5, 3, 15, 3, 7, 1.5, 0);

        // Анализируем смещение центра
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
    static void SaveImage(Mat image, string filename)
    {
        image.SaveImage(filename);
        Console.WriteLine($"saved: {filename}");
    }
    static void DisplayMenu(string[] options, int selectedIndex)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"> {options[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {options[i]}");
            }
        }
    }

    static void ExecuteOptionTime(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                isDay = true;
                break;
            case 1:
                isDay = false;
                break;
            case 2:
                break;
        }
    }

    static void ExecuteOptionKindOFish(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                isHunter = 990;
                break;
            case 1:
                isHunter = 880;
                break;
            case 2:
                break;
        }
    }

    static void TakeOrLeave(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                take = true;
                break;
            case 1:
                take = false;
                break;
            case 2:
                break;
        }
    }

}
