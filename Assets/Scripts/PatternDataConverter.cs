using System.Collections.Generic;
using UnityEngine;
using System;

public class PatternDataConverter
{
    public HashSet<Vector3Int> ConvertPattern(PatternData patternData)
    {
        if (patternData == null || string.IsNullOrEmpty(patternData.RleData))
        {
            Debug.LogWarning("Pattern data is null or RLE data is empty");
            return new HashSet<Vector3Int>();
        }
        
        var result = new HashSet<Vector3Int>();
        
        try
        {
            // Use dimensions from PatternData
            int width = patternData.Width;
            int height = patternData.Height;
            string rleContent = patternData.RleData.Trim();
            
            // Parse RLE content
            int x = 0, y = 0;
            int currentNumber = 0;
            
            for (int i = 0; i < rleContent.Length; i++)
            {
                char c = rleContent[i];
                
                if (char.IsDigit(c))
                {
                    currentNumber = currentNumber * 10 + (c - '0');
                }
                else
                {
                    if (currentNumber == 0) currentNumber = 1;
                    
                    switch (c)
                    {
                        case 'b':
                        case '.':
                            x += currentNumber;
                            break;
                        case 'o':
                            for (int j = 0; j < currentNumber; j++)
                            {
                                result.Add(new Vector3Int(x + j, height - 1 - y, 0));
                            }
                            x += currentNumber;
                            break;
                        case '$':
                            x = 0;
                            y += currentNumber;
                            break;
                        case '!':
                            // End of pattern
                            break;
                    }
                    
                    currentNumber = 0;
                }
            }
            
            Debug.Log($"Converted pattern '{patternData.Name}': {result.Count} cells, dimensions: {width}x{height}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error converting pattern '{patternData.Name}': {e.Message}");
        }
        
        return result;
    }
}
