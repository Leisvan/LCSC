using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Http.Models.Pulse;

public record Character
{
    public int? Realm { get; set; }
    public string? Name { get; set; }
    public long? Id { get; set; }
    public long? AccountId { get; set; }
    public string? Region { get; set; }
    public int? BattlenetId { get; set; }
}