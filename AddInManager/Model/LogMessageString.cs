using System.Windows;

namespace RevitAddinManager.Model;

public class LogMessageString
{
    private int DEFAULD_FONT_SIZE = 12;
    public string Message { get; set; }
    public System.Windows.Media.Brush MessageColor { get; set; }
    public FontWeight FontWeight { get; set; }
    public int FontSize { get; set; }
    public LogMessageString(string message, System.Windows.Media.Brush colore)
    {
        Message = message;
        MessageColor = colore;
        FontWeight = FontWeights.Normal;
        FontSize = DEFAULD_FONT_SIZE;
    }
    public LogMessageString(string message, System.Windows.Media.Brush colore, FontWeight fontWeight)
    {
        Message = message;
        MessageColor = colore;
        FontWeight = fontWeight;
        FontSize = DEFAULD_FONT_SIZE;
    }
    public LogMessageString(string message, System.Windows.Media.Brush colore, FontWeight fontWeight, int fontSize)
    {
        Message = message;
        MessageColor = colore;
        FontWeight = fontWeight;
        FontSize = fontSize;
    }
}