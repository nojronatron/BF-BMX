using System.Collections.ObjectModel;
using BFBMX.Service.Models;

namespace BFBMX.Service.Collections;

public class MessageModelCollection : ObservableCollection<WinlinkMessageModel>
{
  public List<WinlinkMessageModel> MessageRecords { get; set; } = new List<WinlinkMessageModel>();

  public MessageModelCollection() { }
}