namespace Stacks
{
    public enum LayoutOrientation { Horizontal, Vertical }
    public enum AppTheme { System, Light, Dark }

    public class AppSettings
    {
        public string SourceFolderPath { get; set; } = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        public LayoutOrientation Orientation { get; set; } = LayoutOrientation.Horizontal;
        public AppTheme Theme { get; set; } = AppTheme.System;
        public bool RunAtStartup { get; set; } = false;
        public int VerticalOffset { get; set; } = 320;
    }
}