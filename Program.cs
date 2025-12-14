

using System.Diagnostics;

static void PrintHelp()
{
    Console.WriteLine(
@"Usage:
  imagebarser <input> <output> [options]

Options:
  -t <value>   unTransparency step (integer >= 1, default = 2, more value = less transparency)
  -h           Horizontal bars (default is vertical)
  -c           Keep original colors (disable grayscale)

Alternative usage:
  sudo imagebarser install
  sudo imagebarser uninstall
");
}

static void Install()
{
    if (!(OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
    {
        Console.WriteLine("Install command is supported only on macOS and Linux.");
        Environment.Exit(1);
    }

    string targetPath = "/usr/local/bin/imagebarser";
    string selfPath = Environment.ProcessPath
        ?? throw new InvalidOperationException("Cannot determine executable path.");

    if (File.Exists(targetPath))
    {
        Console.WriteLine("Already installed.");
        return;
    }

    try
    {
        File.Copy(selfPath, targetPath);
        Process.Start("chmod", $"+x {targetPath}")?.WaitForExit();

        Console.WriteLine("Installed to /usr/local/bin/imagebarser");
        Console.WriteLine("Restart your terminal.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Permission denied. Try running with sudo.");
        Environment.Exit(1);
    }
}

static void Uninstall()
{
    if (!(OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()))
    {
        Console.WriteLine("Uninstall command is supported only on macOS and Linux.");
        Environment.Exit(1);
    }

    string targetPath = "/usr/local/bin/imagebarser";

    if (!File.Exists(targetPath))
    {
        Console.WriteLine("Not installed.");
        return;
    }

    try
    {
        File.Delete(targetPath);
        Console.WriteLine("Uninstalled.");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Permission denied. Try running with sudo.");
        Environment.Exit(1);
    }
}

static Rgba32 ProcessPixel(Rgba32 src, bool transparent, bool keepColor)
{
    if (transparent)
        return Color.Transparent;

    if (keepColor)
        return src;

    // grayscale
    byte gray = (byte)(
        0.299 * src.R +
        0.587 * src.G +
        0.114 * src.B
    );

    return new Rgba32(gray, gray, gray, src.A);
}

if (args.Length == 1)
{
    if (args[0] == "install")
    {
        Install();
        return;
    }

    if (args[0] == "uninstall")
    {
        Uninstall();
        return;
    }
}

if (args.Length < 2)
{
    PrintHelp();
    Environment.Exit(1);
}

string inputPath = args[0];
string outputPath = args[1];

int transp = 2; // more = less transparent
bool isLinesVertical = true; // by defult bars will be horizontal
bool keepColor = false; // by default it grayscale image


if (!File.Exists(inputPath))
{
    Console.WriteLine("Input file does not exist.");
    Environment.Exit(1);
}
string? outDir = Path.GetDirectoryName(outputPath);
if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
{
    Console.WriteLine("Output directory does not exist.");
    Environment.Exit(1);
}
// parsing args
for (int i = 2; i < args.Length; i++)
{
    if (args[i] == "-t" && i + 1 < args.Length)
    {
        if (!int.TryParse(args[i + 1], out transp))
        {
            Console.WriteLine("-t argument must be a number.");
            Environment.Exit(1);
        }

        i++;
        continue;
    }

    if (args[i] == "-h")
    {
        isLinesVertical = false;
        continue;
    }

    if (args[i] == "-c")
    {
        keepColor = true;
        continue;
    }
}
if (transp < 1)
{
    Console.WriteLine("-t must be >= 1.");
    Environment.Exit(1);
}

//main
int pixelsComputed = 0;
var sw = Stopwatch.StartNew();

using (Image<Rgba32> original = Image.Load<Rgba32>(inputPath))
using (Image<Rgba32> image = new(original.Width, original.Height))
{
    int a = isLinesVertical ? original.Width : original.Height;
    int b = isLinesVertical ? original.Height : original.Width;

    for (int line = 0; line < a; line++)
    {
        for (int pixel = 0; pixel < b; pixel++)
        {
            if (isLinesVertical)
            {
                image[line, pixel] = ProcessPixel(
                    original[line, pixel], (line % transp == 0), keepColor
                );
            }
            else
            {
                image[pixel, line] = ProcessPixel(
                    original[pixel, line], (line % transp == 0), keepColor
                );
            }

            pixelsComputed++;
        }
    }

    sw.Stop();
    image.Save(outputPath);
}

double mpPerSecond =
    (pixelsComputed / 1_000_000.0) / sw.Elapsed.TotalSeconds;

Console.WriteLine(
    $"Executed in {sw.ElapsedMilliseconds} ms, computing speed {mpPerSecond:F2} MP/s"
);

