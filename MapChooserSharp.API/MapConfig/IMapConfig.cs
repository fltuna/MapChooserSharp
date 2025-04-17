namespace MapChooserSharp.API.MapConfig;

public interface IMapConfig
{
    /// <summary>
    /// Map name
    /// </summary>
    public string MapName { get; }
    
    /// <summary>
    /// Alias name for display
    /// </summary>
    public string MapNameAlias { get; }
    
    /// <summary>
    /// Map description
    /// </summary>
    public string MapDescription { get; }
    
    /// <summary>
    /// Is this map disabled?
    /// </summary>
    public bool IsDisabled { get; }
    
    /// <summary>
    /// The value should be 0, when workshop ID is not specified in map config.
    /// </summary>
    public long WorkshopId { get; }
    
    /// <summary>
    /// This map should be nomination only? <br/>
    /// This is used for random map pick when vote initiation.
    /// </summary>
    public bool OnlyNomination { get; }
    
    /// <summary>
    /// How many extends available in this map?
    /// </summary>
    public int MaxExtends { get; }
    
    /// <summary>
    /// Map's default mp_timelimit value
    /// </summary>
    public int MapTime { get; }
    
    /// <summary>
    /// How many minutes extended in per extend?
    /// </summary>
    public int ExtendTimePerExtends { get; }
    
    /// <summary>
    /// Map's default mp_maxround
    /// </summary>
    public int MapRounds { get; }
    
    /// <summary>
    /// How many rounds extended in per extend?
    /// </summary>
    public int ExtendRoundsPerExtends { get; }
    
    /// <summary>
    /// Group settings 
    /// </summary>
    public List<IMapGroupSettings> GroupSettings { get; }
    
    /// <summary>
    /// Nomination settings
    /// </summary>
    public INominationConfig NominationConfig { get; }
    
    /// <summary>
    /// Map cooldown things
    /// </summary>
    public IMapCooldown MapCooldown { get; }
    
    
    /// <summary>
    /// This is for API developers to define custom value like integrate with shop plugin or any other custom plugin. <br/>
    /// <br/>
    /// If defined in config like this: <br/>
    ///
    /// [ze_xxxxx] <br/>
    /// description = "ze xxxxx map!" <br/>
    /// MapNameAlias = "ze xxxxx" <br/>
    /// <br/>
    /// [ze_xxxxxx.extra.shop] <br/>
    /// cost = 10 <br/>
    /// <br/>
    /// Then you can access the value like this: <br/>
    /// <br/>
    /// string cost = ExtraConfiguration["shop"]["cost"] <br/>
    /// 
    /// 
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> ExtraConfiguration { get; }
}