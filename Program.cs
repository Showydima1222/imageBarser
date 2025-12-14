
using System.IO;

if (args.Length < 2)
{
    Console.WriteLine("Usage: imagebarser <input> <output> \notpional:\n-t Transparency (by default = 2, maybe 1 and more)");
} else
{
    string inputPath = args[0];
    string outputPath = args[1];
    int transp = 2;
    if (!File.Exists(inputPath))
    {
        Console.WriteLine("Input file isn't exists");
        Environment.Exit(1);
    } else {
        string? dir = Path.GetDirectoryName(outputPath);
        if (dir == null || !Directory.Exists(dir))
        {
            Console.WriteLine("output path invalid");
            Environment.Exit(1);
        } else
        {
            // Args parser
            for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-t" && i + 1 < args.Length)
                    {
                        string transpRaw = args[i+1];
                        if (!int.TryParse(transpRaw, out transp))
                        {
                            Console.WriteLine("-t Argument must be nubmer.");
                            Environment.Exit(1);
                        }
                        Console.WriteLine($"Transparency: {transp}");
                        i += 1; // пропускаем значения
                    }
                }
            // Main program here
            using (Image<Rgba32> original = Image.Load<Rgba32>(inputPath))
            {
                Image<Rgba32> image = new (original.Size.Width, original.Size.Height);
                for (int vertLineIndex = 0; vertLineIndex < original.Size.Width; vertLineIndex++)
                {
                    for (int pixel = 0; pixel < original.Size.Height; pixel++)
                    {
                        if (vertLineIndex % transp == 0)
                        {
                            image[vertLineIndex, pixel] = Color.Transparent;
                        } else
                        {
                            image[vertLineIndex, pixel] = original[vertLineIndex, pixel];
                        }
                    }
                }
                image.Save(outputPath);
            }
        }
    }

}
