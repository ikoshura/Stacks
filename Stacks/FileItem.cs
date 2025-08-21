using System.Windows.Media; // Pastikan using ini ada

namespace Stacks
{
    public class FileItem
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        // Ubah tipe data dari ImageSource? menjadi ImageSource?
        public ImageSource? Thumbnail { get; set; }
    }
}