namespace LCSC.Models.BattleNet;


public partial class LadderTierList
{
    public Key? Key { get; set; }

    public List<Tier>? Tier { get; set; }
}

public partial class Key
{
    public int League_Id { get; set; }

    public int Season_Id { get; set; }

    public int Queue_Id { get; set; }

    public int Team_Type { get; set; }
}


public partial class Tier
{
    public int Id { get; set; }

    public int Min_Rating { get; set; }

    public int Max_Rating { get; set; }
}


