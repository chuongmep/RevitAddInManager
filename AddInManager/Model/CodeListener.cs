using System.Diagnostics;
using System.IO;

namespace RevitAddinManager.Model;

public class CodeListener  : TraceListener
{
    public override void Write(string message)
    {
        using (StreamWriter st = new StreamWriter(DefaultSetting.PathLogFile, true))
        {
            st.Write(message);
            st.Close();
        }
    }

    public override void WriteLine(string message)
    {
        using (StreamWriter st = new StreamWriter(DefaultSetting.PathLogFile, true))
        {
            st.Write(message);
            st.Close();
        }
    }
}