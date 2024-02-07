using System.Collections.ObjectModel;
using BFBMX.Service.Models;

namespace BFBMX.Service.Collections;

public class MessageModelCollection : ObservableCollection<BibMessageModel>
{
  public List<BibMessageModel> MessageRecords { get; set; } = new List<BibMessageModel>();

  public MessageModelCollection() { }
}