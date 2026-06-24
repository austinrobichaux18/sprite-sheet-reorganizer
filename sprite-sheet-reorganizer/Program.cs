using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    static readonly string[] TargetOrder = new[]
    {
        "n", "ne", "e", "se", "s", "sw", "w", "nw"
    };

    // TODO units are 162, worms are 128
    const int FrameWidth = 162;
    const int FrameHeight = FrameWidth;
    const int LineLength = 3;

    static void Main()
    {
        var baseDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        var inputDir = Path.Combine(baseDir, "Inputs");
        var outputDir = Path.Combine(baseDir, "Outputs");

        Directory.CreateDirectory(inputDir);
        Directory.CreateDirectory(outputDir);

        var inputFile = Directory.GetFiles(inputDir, "*.png").FirstOrDefault();

        if (inputFile == null)
        {
            Console.WriteLine("No PNG found in Input folder.");
            return;
        }

        Console.WriteLine($"Processing: {Path.GetFileName(inputFile)}");

        using var bitmap = new Bitmap(inputFile);

        var frames = SplitSpriteSheet(bitmap);

        Console.WriteLine("\nEnter CURRENT order of frames (space):");
        Console.WriteLine("Example: n e s w ne nw se sw");
        Console.Write("> ");

        var input = Console.ReadLine();
        var currentOrder = input!
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLower())
            .ToList();

        if (currentOrder.Count != 8)
        {
            Console.WriteLine("You must provide exactly 8 directions.");
            return;
        }

        var map = new Dictionary<string, Bitmap>();
        for (int i = 0; i < 8; i++)
        {
            map[currentOrder[i]] = frames[i];
        }

        var reordered = TargetOrder.Select(dir => map[dir]).ToList();

        var outputPath = Path.Combine(
            outputDir,
            Path.GetFileNameWithoutExtension(inputFile) + ".png"
        );

        using var result = BuildSpriteSheet(reordered);

        result.Save(outputPath, ImageFormat.Png);

        Console.WriteLine($"Done! Saved to: {outputPath}");
    }

    static List<Bitmap> SplitSpriteSheet(Bitmap sheet)
    {
        var frames = new List<Bitmap>();

        for (int i = 0; i < 8; i++)
        {
            int row = i / LineLength;
            int col = i % LineLength;

            int x = col * FrameWidth;
            int y = row * FrameHeight;

            var rect = new Rectangle(x, y, FrameWidth, FrameHeight);
            var clone = sheet.Clone(rect, PixelFormat.Format32bppArgb);

            frames.Add(clone);
        }

        return frames;
    }

    static Bitmap BuildSpriteSheet(List<Bitmap> frames)
    {
        int cols = 3;
        int rows = 3;

        var output = new Bitmap(cols * FrameWidth, rows * FrameHeight);

        using var g = Graphics.FromImage(output);
        g.Clear(Color.Transparent);

        for (int i = 0; i < frames.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            g.DrawImage(
                frames[i],
                col * FrameWidth,
                row * FrameHeight,
                FrameWidth,
                FrameHeight
            );
        }

        return output;
    }
}