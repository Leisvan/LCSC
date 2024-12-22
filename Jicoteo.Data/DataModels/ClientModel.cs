using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LCSC.Data.DataModels;

[Table("Client")]
public class ClientModel
{
    [Key]
    public int Id { get; set; }

    public ICollection<MessageTriggerDataModel> MessageTriggers
    {
        get; set;
    } = [];
}