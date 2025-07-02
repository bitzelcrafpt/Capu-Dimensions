#if EDITOR

#else
namespace Capu_Dimensions.Models;

public class DimensionPackage
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string spawnPosPath { get; set; }
    public string terminalPosPath { get; set; }
    public bool UsePostProcessing { get; set; }
    public bool UseNewShader { get; set; }

    public DimensionPackage(string name, string author, string description, string spawnpospath, string terminalpospath, bool usePostProcessing, bool useNewShader)
    {
        Name = name;
        Author = author;
        Description = description;
        spawnPosPath = spawnpospath;
        terminalPosPath = terminalpospath;
        UsePostProcessing = usePostProcessing;
        UseNewShader = useNewShader;
    }
}
#endif