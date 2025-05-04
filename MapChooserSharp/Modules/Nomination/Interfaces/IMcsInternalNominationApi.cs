using MapChooserSharp.API.Nomination;
using MapChooserSharp.Interfaces;

namespace MapChooserSharp.Modules.Nomination.Interfaces;

internal interface IMcsInternalNominationApi: IMcsInternalApiBase, IMcsNominationApi
{
    /// <summary>
    /// Internal nomination data collection
    /// </summary>
    internal Dictionary<string, IMcsNominationData> NominatedMaps { get; }
}