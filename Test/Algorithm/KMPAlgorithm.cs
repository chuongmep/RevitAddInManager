namespace Test.Algorithm;

//Given a text txt[0..n-1] and a pattern pat[0..m-1], write a function search(char pat[], char txt[])
public static class KMPAlgorithm
{
    public static bool KmpSearch(string pat, string txt)
    {
        pat = pat.ToLower();
        txt = txt.ToLower();
        int M = pat.Length;
        int N = txt.Length;
        int[] lps = new int[M];
        int j = 0; // index for pat[]

        ComputeLpsArray(pat, M, lps);
        int i = 0;
        while (i < N)
        {
            if (pat[j] == txt[i])
            {
                j++;
                i++;
            }
            if (j == M)
            {
                return true;
            }
            if (i < N && pat[j] != txt[i])
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i = i + 1;
            }
        }
        return false;
    }

    static void ComputeLpsArray(string pat, int M, int[] lps)
    {
        int len = 0;
        int i = 1;
        lps[0] = 0;
        while (i < M)
        {
            if (pat[i] == pat[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else 
            {
                if (len != 0)
                {
                    len = lps[len - 1];
                }
                else
                {
                    lps[i] = len;
                    i++;
                }
            }
        }
    } 
}