using System.ComponentModel;
using DaramRenamer.Helpers;

namespace DaramRenamer.Commands;

[Serializable]
public class ConvensionCommand : ICommand
{
    public Convension Convension { get; set; } = Convension.Lower;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Order => int.MinValue + 10;
    public CommandCategory Category => CommandCategory.Filename;
    
    public void Apply(FileItem item, int _)
    {
        var filename = item.ChangedNameWithoutExtension;
        var ext = item.ChangedExtension;

        var newName = Convension switch
        {
            Convension.Upper => filename.ToUpper(),
            Convension.Lower => filename.ToLower(),
            Convension.UpperFirstLetter => filename.ToUpperFirstLetter(),
            Convension.SnakeCase => filename.ToSnakeCase(),
            Convension.UpperSnakeCase => filename.ToUpperSnakeCase(),
            Convension.KebabCase => filename.ToKebabCase(),
            Convension.UpperKebabCase => filename.ToUpperKebabCase(),
            Convension.CamelCase => filename.ToCamelCase(),
            Convension.PascalCase => filename.ToPascalCase(),
            _ => throw new ArgumentOutOfRangeException()
        };

        item.ChangedName = $"{newName}{ext}";
    }
}