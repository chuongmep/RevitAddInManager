
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitElementBipChecker.Model;

public static class CosineSimilarity
{
    /// <summary>
    /// Calculates the cosine similarity between two strings 'str1' and 'str2'.
    /// Cosine similarity measures the cosine of the angle between two non-zero vectors
    /// in an inner product space, representing the similarity of their directions.
    /// This method treats the input strings as vectors of term frequencies and calculates
    /// </summary>
    /// <param name="str1">The first input string to compare as a vector representation of term frequencies.</param>
    /// <param name="str2">The second input string to compare as a vector representation of term frequencies.</param>
    /// <returns>
    /// The cosine similarity value between the two input strings.
    /// </returns>
    public static double CalculateCosineSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
            return 1;
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0;
        // Tokenize the input strings into individual words
        string[] words1 = TokenizeString(str1);
        string[] words2 = TokenizeString(str2);

        // Create a bag of words (term frequency vectors) for each input string
        var bagOfWords1 = GetBagOfWords(words1);
        var bagOfWords2 = GetBagOfWords(words2);

        // Get all unique words in both strings
        var allWords = GetAllUniqueWords(words1, words2);

        // Create vectors of term frequencies for both strings
        var vector1 = GetTermFrequencyVector(allWords, bagOfWords1);
        var vector2 = GetTermFrequencyVector(allWords, bagOfWords2);

        // Calculate the Cosine similarity between the two vectors
        double similarity = CalculateCosine(vector1, vector2);
        return similarity;
    }

    /// <summary>
    /// Tokenizes the input string 'str' into individual words.
    /// </summary>
    /// <param name="str">The input string to tokenize.</param>
    /// <returns>
    /// An array of individual words extracted from the input string.
    /// </returns>
    private static string[] TokenizeString(string str)
    {
        return str.Split(new[] { ' ', ',', '.', ';', ':', '?', '!'}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Creates a bag of words (term frequency vectors) for the input array of 'words'.
    /// The bag of words is represented as a dictionary with each word as the key and its
    /// corresponding frequency as the value.
    /// </summary>
    /// <param name="words">The array of words to create the bag of words from.</param>
    /// <returns>
    /// A dictionary containing the bag of words with word frequencies.
    /// </returns>
    private static Dictionary<string, double> GetBagOfWords(string[] words)
    {
        var bagOfWords = new Dictionary<string, double>();
        foreach (var word in words)
        {
            if (bagOfWords.ContainsKey(word))
                bagOfWords[word]++;
            else
                bagOfWords[word] = 1;
        }
        return bagOfWords;
    }

    /// <summary>
    /// Combines the unique words from 'words1' and 'words2' into a single array.
    /// </summary>
    /// <param name="words1">The first array of words.</param>
    /// <param name="words2">The second array of words.</param>
    /// <returns>
    /// An array containing all unique words from both 'words1' and 'words2'.
    /// </returns>
    private static string[] GetAllUniqueWords(string[] words1, string[] words2)
    {
        return words1.Union(words2).Distinct().ToArray();
    }

    /// <summary>
    /// Creates a vector of term frequencies for the given 'allWords' based on the 'bagOfWords'.
    /// </summary>
    /// <param name="allWords">The array of all unique words from both input strings.</param>
    /// <param name="bagOfWords">The bag of words (term frequency vectors) for one of the input strings.</param>
    /// <returns>
    /// A double array representing the vector of term frequencies for the given 'allWords'.
    /// </returns>
    private static double[] GetTermFrequencyVector(string[] allWords, Dictionary<string, double> bagOfWords)
    {
        var vector = new double[allWords.Length];
        for (int i = 0; i < allWords.Length; i++)
        {
            if (bagOfWords.TryGetValue(allWords[i], out var frequency))
                vector[i] = frequency;
            else
                vector[i] = 0;
        }
        return vector;
    }

    /// <summary>
    /// Calculates the cosine similarity between two vectors 'vector1' and 'vector2'.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>
    /// The cosine similarity value between the two input vectors.
    /// </returns>
    private static double CalculateCosine(double[] vector1, double[] vector2)
    {
        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = System.Math.Sqrt(magnitude1);
        magnitude2 = System.Math.Sqrt(magnitude2);

        double similarity = dotProduct / (magnitude1 * magnitude2);
        return similarity;
    }
}