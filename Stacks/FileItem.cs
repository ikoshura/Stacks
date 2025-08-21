using System.Windows.Media;

namespace Stacks
{
    public class FileItem
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public ImageSource? Thumbnail { get; set; }
    }
}