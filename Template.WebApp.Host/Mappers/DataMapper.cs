namespace Template.WebApp.Host.Mappers;

using Smart.Mapper;

using Template.WebApp.Host.Areas.Default.Models;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataEditForm ToForm(DataEntity entity);
}
