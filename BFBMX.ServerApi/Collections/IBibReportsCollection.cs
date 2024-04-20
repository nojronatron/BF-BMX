using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Collections
{
    public interface IBibReportsCollection
    {
        bool AddEntityToCollection(WinlinkMessageModel message);
    }
}