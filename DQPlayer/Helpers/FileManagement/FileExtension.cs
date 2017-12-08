namespace DQPlayer.Helpers.FileManagement
{
    public class FileExtension
    {
        public string Extension { get; }
        public string Name { get; }

        public FileExtension(string extension, string name)
        {
            Extension = extension;
            Name = name;
        }

        public override string ToString() => $"{Name} ({Extension})";
    }
}