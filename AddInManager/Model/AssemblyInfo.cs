using System.Reflection;

namespace RevitAddinManager.Model;

[Serializable]
public class AssemblyInfo
{
    public AssemblyInfo(Assembly assembly)
    {
        this.FullPath = assembly.FullName;
        this.FileName = assembly.Location;
        this.ReferencedAssemblies = assembly.GetReferencedAssemblies();
    }
    public AssemblyInfo(string fileName, string fullPath, string fileExtension)
    {
        this.FileName = fileName;
        this.FullPath = fullPath;
        this.FileExtension = fileExtension;
    }

    public AssemblyInfo(
        string fileName,
        string fullPath,
        string fileExtension,
        PortableExecutableKinds portableExecutableKinds,
        ImageFileMachine imageFileMachine,
        IEnumerable<AssemblyName> referencedAssemblies,
        string imageRuntimeVersion
    )
    {
        this.FileName = fileName;
        this.FullPath = fullPath;
        this.FileExtension = fileExtension;
        this.PortableExecutableKinds = portableExecutableKinds;
        this.ImageFileMachine = imageFileMachine;
        this.ReferencedAssemblies = referencedAssemblies;
        this.ImageRuntimeVersion = imageRuntimeVersion;
    }

    public string FileName { get; private set; }
    public string FullPath { get; private set; }
    public string FileExtension { get; private set; }
    public PortableExecutableKinds PortableExecutableKinds { get; private set; }
    public ImageFileMachine ImageFileMachine { get; private set; }
    public IEnumerable<AssemblyName> ReferencedAssemblies { get; private set; }
    public string ImageRuntimeVersion { get; private set; }
}