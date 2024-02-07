using System.Collections.ObjectModel;
using BFBMX.Service.Models;

namespace BFBMX.Service.Collections;

public class MessageModelCollection : Collection<MessageModel>
{
  public List<MessageModel> MessageRecords { get; set; } = new List<MessageModel>();

  public MessageModelCollection() { }
}