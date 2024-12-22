using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSCC.Data.DataModels;

[Table("MessageTrigger")]
public class MessageTriggerDataModel
{
    public string[]? Answers { get; set; }

    public ClientModel? Client { get; set; }

    [Key]
    public int Id { get; set; }

    public int Priority { get; set; }

    public string[]? Reactions { get; set; }

    public string[]? Triggers { get; set; }
}