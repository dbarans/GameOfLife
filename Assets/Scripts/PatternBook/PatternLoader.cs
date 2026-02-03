using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class PatternCollection
{
    public int TotalPatterns;
    public PatternData[] Patterns;
}

public class PatternLoader : MonoBehaviour
{
    private static PatternCollection patternCollection;
    private static bool isLoaded = false;

    private static bool ValidateRleData(string rleData)
    {
        if (string.IsNullOrWhiteSpace(rleData))
            return false;

        // Valid RLE characters: digits, 'b', '.', 'o', '$', '!'
        // Based on PatternDataConverter.cs parser
        foreach (char c in rleData)
        {
            if (char.IsDigit(c))
                continue;
            
            if (c == 'b' || c == '.' || c == 'o' || c == '$' || c == '!')
                continue;
            
            // Invalid character found
            return false;
        }

        // Must end with '!' to be valid RLE
        if (!rleData.TrimEnd().EndsWith("!"))
            return false;

        return true;
    }

    private static void FilterPatternsWithoutAuthor()
    {
        if (patternCollection != null && patternCollection.Patterns != null)
        {
            List<PatternData> filteredPatterns = new List<PatternData>();
            foreach (var pattern in patternCollection.Patterns)
            {
                // Filter out patterns without author or with dimensions larger than 300x300
                if (!string.IsNullOrWhiteSpace(pattern.Author) && 
                    pattern.Width <= 300 && 
                    pattern.Height <= 300 &&
                    pattern.Width > 2 &&
                    pattern.Height > 2)
                {
                    // Validate RLE data format
                    if (!ValidateRleData(pattern.RleData))
                    {
                        Debug.LogWarning($"Pattern '{pattern.Name}' has invalid RLE data format and will be filtered out");
                        continue;
                    }

                    // Remove .rle extension from Name if present
                    if (!string.IsNullOrEmpty(pattern.Name) && pattern.Name.EndsWith(".rle", StringComparison.OrdinalIgnoreCase))
                    {
                        pattern.Name = pattern.Name.Substring(0, pattern.Name.Length - 4);
                    }
                    
                    filteredPatterns.Add(pattern);
                }
            }
            patternCollection.Patterns = filteredPatterns.ToArray();
            patternCollection.TotalPatterns = filteredPatterns.Count;
        }
    }

    public static PatternCollection LoadPatterns()
    {
        if (isLoaded && patternCollection != null)
        {
            return patternCollection;
        }

        try
        {
            // Try Resources first
            TextAsset jsonFile = Resources.Load<TextAsset>("patterns");
            if (jsonFile != null)
            {
                patternCollection = JsonUtility.FromJson<PatternCollection>(jsonFile.text);
            }
            else
            {
                // Fallback to StreamingAssets - use UnityWebRequest for Android compatibility
                string jsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, "patterns.json");
                
                if (Application.platform == RuntimePlatform.Android)
                {
                    // On Android, use UnityWebRequest
                    UnityWebRequest request = UnityWebRequest.Get(jsonPath);
                    request.SendWebRequest();
                    
                    while (!request.isDone)
                    {
                        // Wait for request to complete
                    }
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string jsonContent = request.downloadHandler.text;
                        patternCollection = JsonUtility.FromJson<PatternCollection>(jsonContent);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load patterns from StreamingAssets: {request.error}");
                        return null;
                    }
                }
                else
                {
                    // On other platforms, use File.ReadAllText
                    if (System.IO.File.Exists(jsonPath))
                    {
                        string jsonContent = System.IO.File.ReadAllText(jsonPath);
                        patternCollection = JsonUtility.FromJson<PatternCollection>(jsonContent);
                    }
                    else
                    {
                        Debug.LogError("Patterns file not found in Resources or StreamingAssets");
                        return null;
                    }
                }
            }

            // Filter out patterns without author
            FilterPatternsWithoutAuthor();

            isLoaded = true;
            Debug.Log($"Loaded {patternCollection.TotalPatterns} patterns (filtered: only patterns with author)");
            return patternCollection;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading patterns: {e.Message}");
            return null;
        }
    }

    public static PatternData GetFirstPattern()
    {
        var patterns = LoadPatterns();
        if (patterns != null && patterns.Patterns != null && patterns.Patterns.Length > 0)
        {
            return patterns.Patterns[0];
        }
        return null;
    }

    public static PatternData GetPatternByName(string name)
    {
        var patterns = LoadPatterns();
        if (patterns != null && patterns.Patterns != null)
        {
            foreach (var pattern in patterns.Patterns)
            {
                if (pattern.Name == name)
                {
                    return pattern;
                }
            }
        }
        return null;
    }

    public static PatternData GetPatternById(int id)
    {
        var patterns = LoadPatterns();
        if (patterns != null && patterns.Patterns != null)
        {
            foreach (var pattern in patterns.Patterns)
            {
                if (pattern.Id == id)
                {
                    return pattern;
                }
            }
        }
        return null;
    }

    public static PatternData[] GetPatternsPage(int pageNumber, int patternsPerPage = 10)
    {
        var patterns = LoadPatterns();
        if (patterns == null || patterns.Patterns == null)
        {
            return new PatternData[0];
        }

        int startIndex = (pageNumber - 1) * patternsPerPage;
        int endIndex = Mathf.Min(startIndex + patternsPerPage, patterns.Patterns.Length);

        if (startIndex >= patterns.Patterns.Length)
        {
            return new PatternData[0];
        }

        PatternData[] pagePatterns = new PatternData[endIndex - startIndex];
        for (int i = startIndex; i < endIndex; i++)
        {
            pagePatterns[i - startIndex] = patterns.Patterns[i];
        }

        return pagePatterns;
    }

    public static int GetTotalPages(int patternsPerPage = 10)
    {
        var patterns = LoadPatterns();
        if (patterns == null || patterns.Patterns == null)
        {
            return 0;
        }

        return Mathf.CeilToInt((float)patterns.Patterns.Length / patternsPerPage);
    }

 
    public static IEnumerator LoadPatternsAsync(System.Action<PatternCollection> onComplete)
    {
        if (isLoaded && patternCollection != null)
        {
            onComplete?.Invoke(patternCollection);
            yield break;
        }

        // Try Resources first
        TextAsset jsonFile = Resources.Load<TextAsset>("patterns");
        if (jsonFile != null)
        {
            patternCollection = JsonUtility.FromJson<PatternCollection>(jsonFile.text);
            FilterPatternsWithoutAuthor();
            isLoaded = true;
            onComplete?.Invoke(patternCollection);
            yield break;
        }

        // Fallback to StreamingAssets
        string jsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, "patterns.json");
        
        if (Application.platform == RuntimePlatform.Android)
        {
            // On Android, use UnityWebRequest
            using (UnityWebRequest request = UnityWebRequest.Get(jsonPath))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonContent = request.downloadHandler.text;
                    patternCollection = JsonUtility.FromJson<PatternCollection>(jsonContent);
                    FilterPatternsWithoutAuthor();
                    isLoaded = true;
                    Debug.Log($"Loaded {patternCollection.TotalPatterns} patterns (filtered: only patterns with author)");
                }
                else
                {
                    Debug.LogError($"Failed to load patterns from StreamingAssets: {request.error}");
                    patternCollection = null;
                }
            }
        }
        else
        {
            // On other platforms, use File.ReadAllText
            if (System.IO.File.Exists(jsonPath))
            {
                string jsonContent = System.IO.File.ReadAllText(jsonPath);
                patternCollection = JsonUtility.FromJson<PatternCollection>(jsonContent);
                FilterPatternsWithoutAuthor();
                isLoaded = true;
                Debug.Log($"Loaded {patternCollection.TotalPatterns} patterns (filtered: only patterns with author)");
            }
            else
            {
                Debug.LogError("Patterns file not found in Resources or StreamingAssets");
                patternCollection = null;
            }
        }
        
        onComplete?.Invoke(patternCollection);
    }
}
