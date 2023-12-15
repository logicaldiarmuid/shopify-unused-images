using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter the path to the Shopify theme folder:");
        string themePath = Console.ReadLine();

        Console.WriteLine("Enter the path to the Images folder:");
        string imagesFolderPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(themePath) || string.IsNullOrWhiteSpace(imagesFolderPath))
        {
            Console.WriteLine("Please provide the theme path and images folder path.");
            return;
        }

        try
        {
            List<string> liquidFileContents = await ReadLiquidFiles(themePath);
            List<string> jsonFileContents = ReadJsonFiles(themePath);
            List<string> allThemeContents = liquidFileContents.Concat(jsonFileContents).ToList();

            List<string> unusedImages = GetUnusedImages(imagesFolderPath, allThemeContents);

            MoveUnusedImages(imagesFolderPath, "unused", unusedImages);

            Console.WriteLine("Unused images moved to the 'unused' subfolder.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static async Task<List<string>> ReadLiquidFiles(string themePath)
    {
        var liquidFiles = Directory.GetFiles(themePath, "*.liquid", SearchOption.AllDirectories);

        var liquidContents = new List<string>();

        foreach (var file in liquidFiles)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                liquidContents.Add(await reader.ReadToEndAsync());
            }
        }

        return liquidContents;
    }

    private static List<string> ReadJsonFiles(string themePath)
    {
        var jsonFiles = Directory.GetFiles(themePath, "*.json", SearchOption.AllDirectories);

        var jsonContents = new List<string>();

        foreach (var file in jsonFiles)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                jsonContents.Add(reader.ReadToEnd());
            }
        }

        return jsonContents;
    }

    private static List<string> GetUnusedImages(string imagesFolderPath, List<string> themeContents)
    {
        List<string> imageFiles = Directory.GetFiles(imagesFolderPath, "*.jpg", SearchOption.AllDirectories)
                                            .Union(Directory.GetFiles(imagesFolderPath, "*.png", SearchOption.AllDirectories))
                                            .Union(Directory.GetFiles(imagesFolderPath, "*.gif", SearchOption.AllDirectories))
                                            .ToList();

        List<string> usedImages = new List<string>();

        foreach (var image in imageFiles)
        {
            string filename = Path.GetFileName(image);
            bool isUsed = themeContents.Any(content => ContainsFilename(content, filename));
            if (isUsed)
            {
                usedImages.Add(image);
            }
        }

        List<string> unusedImages = imageFiles.Except(usedImages).ToList();

        return unusedImages;
    }

    private static void MoveUnusedImages(string imagesFolderPath, string unusedSubfolderName, List<string> unusedImages)
    {
        string unusedFolderPath = Path.Combine(imagesFolderPath, unusedSubfolderName);

        if (!Directory.Exists(unusedFolderPath))
        {
            Directory.CreateDirectory(unusedFolderPath);
        }

        foreach (var unusedImage in unusedImages)
        {
            string destinationPath = Path.Combine(unusedFolderPath, Path.GetFileName(unusedImage));
            File.Move(unusedImage, destinationPath);
        }
    }

    private static bool ContainsFilename(string code, string filename)
    {
        // Use a case-insensitive search for the filename in the code content
        return code.IndexOf(filename, StringComparison.OrdinalIgnoreCase) != -1;
    }
}
