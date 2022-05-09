using System.Diagnostics;
using System.IO;
using System.Text;

namespace RevitAddinManager.Model;

public class CodeListener  : TraceListener
{
    private bool IsExecuting { get; set; }
    public override void Write(string message)
    {
        WriteMessage(message);
        
    }
    public override void WriteLine(string message)
    {
        WriteMessage(message);
    }

    void WriteMessage(string message)
    {
        if (!IsExecuting) 
        {
            IsExecuting = true;
            using (StreamWriter st = new StreamWriter(DefaultSetting.PathLogFile, true))
            {
                string join = String.Join(": ",$"{DateTime.Now}", message);
                st.WriteLine(join);
                st.Close();
            }
            IsExecuting = false;
        }
    }
}