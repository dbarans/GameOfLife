using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RLEParser
{
    public class PatternData
    {
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public string FileName { get; set; } = "";
        public string RleData { get; set; } = "";
    }

    public class RLEPatterns
    {
        public List<PatternData> Patterns { get; set; } = new List<PatternData>();
        public int TotalPatterns { get; set; }
        public string SourceFolder { get; set; } = "";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== RLE Parser for Game of Life ===");
            
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: RLEParser.exe <path_to_folder_with_RLE_files> [output_json_file]");
                Console.WriteLine("Example: RLEParser.exe C:\\Patterns\\RLE patterns.json");
                return;
            }

            string folderPath = args[0];
            string outputFile = args.Length > 1 ? args[1] : "rle_patterns.json";
            
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Error: Folder '{folderPath}' does not exist!");
                return;
            }

            // Find all .rle files in the folder
            string[] rleFiles = Directory.GetFiles(folderPath, "*.rle");
            
            if (rleFiles.Length == 0)
            {
                Console.WriteLine($"No .rle files found in folder '{folderPath}'");
                return;
            }

            Console.WriteLine($"Found {rleFiles.Length} RLE files, converting to JSON...");

            // Start writing JSON file incrementally
            int processedCount = 0;
            try
            {
                using (var writer = new StreamWriter(outputFile))
                {
                    // Write JSON header
                    writer.WriteLine("{");
                    writer.WriteLine($"  \"SourceFolder\": \"{folderPath.Replace("\\", "\\\\")}\",");
                    writer.WriteLine($"  \"TotalPatterns\": {rleFiles.Length},");
                    writer.WriteLine("  \"Patterns\": [");

                    bool isFirstPattern = true;

                    foreach (string filePath in rleFiles)
                    {
                        try
                        {
                            var pattern = ParseRLEFile(filePath);
                            if (pattern != null)
                            {
                                if (!isFirstPattern)
                                    writer.WriteLine(",");
                                
                                // Write pattern directly to file
                                string patternJson = JsonConvert.SerializeObject(pattern, Formatting.Indented);
                                // Indent the pattern JSON
                                string indentedPattern = string.Join("\n", patternJson.Split('\n').Select(line => "    " + line));
                                writer.Write(indentedPattern);
                                
                                isFirstPattern = false;
                                processedCount++;
                                Console.WriteLine($"✓ Parsed: {Path.GetFileName(filePath)} ({processedCount}/{rleFiles.Length})");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Create metadata-only entry for failed patterns
                            try
                            {
                                var failedPattern = CreateMetadataOnlyPattern(filePath, ex.Message);
                                if (failedPattern != null)
                                {
                                    if (!isFirstPattern)
                                        writer.WriteLine(",");
                                    
                                    // Write metadata-only pattern
                                    string patternJson = JsonConvert.SerializeObject(failedPattern, Formatting.Indented);
                                    string indentedPattern = string.Join("\n", patternJson.Split('\n').Select(line => "    " + line));
                                    writer.Write(indentedPattern);
                                    
                                    isFirstPattern = false;
                                    processedCount++;
                                    Console.WriteLine($"⚠ Metadata only: {Path.GetFileName(filePath)} - {ex.Message}");
                                }
                            }
                            catch (Exception metaEx)
                            {
                                Console.WriteLine($"✗ Complete failure: {Path.GetFileName(filePath)} - {metaEx.Message}");
                            }
                        }
                    }

                    // Write JSON footer
                    writer.WriteLine();
                    writer.WriteLine("  ]");
                    writer.WriteLine("}");
                }

                Console.WriteLine($"\n✓ Successfully saved {processedCount} patterns to '{outputFile}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error saving JSON file: {ex.Message}");
            }
        }

        static PatternData? ParseRLEFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            var pattern = new List<string>();
            int width = 0;
            int height = 0;
            string name = "";
            string author = "";
            string description = "";

            // Parse header
            foreach (string line in lines)
            {
                if (line.StartsWith("#N"))
                    name = line.Substring(2).Trim();
                else if (line.StartsWith("#O"))
                    author = line.Substring(2).Trim();
                else if (line.StartsWith("#C"))
                    description = line.Substring(2).Trim();
                else if (line.StartsWith("x =") && line.Contains("y ="))
                {
                    // Parse dimensions
                    var parts = line.Split(',');
                    foreach (var part in parts)
                    {
                        if (part.Contains("x ="))
                            int.TryParse(part.Split('=')[1].Trim(), out width);
                        if (part.Contains("y ="))
                            int.TryParse(part.Split('=')[1].Trim(), out height);
                    }
                }
                else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    pattern.Add(line);
                }
            }

            // Store original RLE data instead of converting to 2D array
            if (pattern.Count > 0)
            {
                var patternData = new PatternData
                {
                    Name = name,
                    Author = author,
                    Description = description,
                    Width = width,
                    Height = height,
                    FileName = Path.GetFileName(filePath),
                    RleData = string.Join("", pattern) // Store original RLE string
                };
                return patternData;
            }

            return null;
        }


        static PatternData? CreateMetadataOnlyPattern(string filePath, string errorMessage)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string name = "";
                string author = "";
                string description = "";
                int width = 0;
                int height = 0;
                string rleData = "";

                // Parse only header metadata and collect RLE data
                foreach (string line in lines)
                {
                    if (line.StartsWith("#N"))
                        name = line.Substring(2).Trim();
                    else if (line.StartsWith("#O"))
                        author = line.Substring(2).Trim();
                    else if (line.StartsWith("#C"))
                        description = line.Substring(2).Trim();
                    else if (line.StartsWith("x =") && line.Contains("y ="))
                    {
                        // Parse dimensions
                        var parts = line.Split(',');
                        foreach (var part in parts)
                        {
                            if (part.Contains("x ="))
                                int.TryParse(part.Split('=')[1].Trim(), out width);
                            if (part.Contains("y ="))
                                int.TryParse(part.Split('=')[1].Trim(), out height);
                        }
                    }
                    else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        rleData += line;
                    }
                }

                return new PatternData
                {
                    Name = name,
                    Author = author,
                    Description = description + $" [ERROR: {errorMessage}]",
                    Width = width,
                    Height = height,
                    FileName = Path.GetFileName(filePath),
                    RleData = rleData // Store original RLE data even for failed patterns
                };
            }
            catch
            {
                // If even metadata parsing fails, create minimal entry
                return new PatternData
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    Author = "",
                    Description = $"Failed to parse: {errorMessage}",
                    Width = 0,
                    Height = 0,
                    FileName = Path.GetFileName(filePath),
                    RleData = ""
                };
            }
        }
    }
}
