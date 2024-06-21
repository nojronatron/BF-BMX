
namespace BFBMX.Service.Models
{
    public class DiscoveredFileModel : IEquatable<DiscoveredFileModel?>, IComparable<DiscoveredFileModel>
    {
        public FileInfo FullFileInfo { get; }

        /// <summary>
        /// Create a new DiscoveredFile object. ** Does NOT verify the file exists!! **
        /// </summary>
        /// <param name="fullPath"></param>
        public DiscoveredFileModel(string fullPath)
        {
            FullFileInfo = new FileInfo(fullPath);
        }

        public bool HasWarningFlag { get; set; } = false;
        public string FileName => FullFileInfo.Name;
        public string FullFilePath => FullFileInfo.FullName;
        public DateTime FileTimeStamp => FullFileInfo.CreationTime;

        public override bool Equals(object? obj)
        {
            return Equals(obj as DiscoveredFileModel);
        }

        public bool Equals(DiscoveredFileModel? other)
        {
            return other is not null &&
                   FullFilePath == other.FullFilePath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FullFilePath);
        }

        public int CompareTo(DiscoveredFileModel? other)
        {
            if (other == null) return 1;
            return string.Compare(FullFilePath, other.FullFilePath, StringComparison.Ordinal);
        }

        public static bool operator ==(DiscoveredFileModel? left, DiscoveredFileModel? right)
        {
            return EqualityComparer<DiscoveredFileModel>.Default.Equals(left, right);
        }

        public static bool operator !=(DiscoveredFileModel? left, DiscoveredFileModel? right)
        {
            return !(left == right);
        }
    }
}
