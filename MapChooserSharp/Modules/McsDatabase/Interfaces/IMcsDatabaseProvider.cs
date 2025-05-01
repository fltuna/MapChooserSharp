using MapChooserSharp.Modules.McsDatabase.Repositories;
using MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Interfaces;

internal interface IMcsDatabaseProvider
{
    internal IMcsMapInformationRepository MapInfoRepository { get; }
    internal McsGroupInformationRepository GroupInfoRepository { get; }
}