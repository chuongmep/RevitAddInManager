using System.Text;

namespace RevitAddinManager.Model;

/// <summary>
/// Builds a Windows command line out of raw values so that the receiving process
/// gets them back unchanged through <see cref="System.Environment.GetCommandLineArgs"/>.
/// </summary>
public static class ArgumentUtils
{
    public static string Quote(params string[] values)
    {
        var builder = new StringBuilder();
        foreach (var value in values)
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append(Escape(value));
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;
        var builder = new StringBuilder("\"");
        for (var i = 0; i < value.Length; i++)
        {
            var backslashes = 0;
            while (i < value.Length && value[i] == '\\')
            {
                backslashes++;
                i++;
            }

            if (i == value.Length)
            {
                // Trailing backslashes must not escape the closing quote.
                builder.Append('\\', backslashes * 2);
                break;
            }

            if (value[i] == '"')
            {
                builder.Append('\\', backslashes * 2 + 1);
                builder.Append('"');
            }
            else
            {
                builder.Append('\\', backslashes);
                builder.Append(value[i]);
            }
        }

        return builder.Append('"').ToString();
    }
}
