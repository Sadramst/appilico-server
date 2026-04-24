using AutoMapper;
using Appilico.Server.Business.Mappings;

namespace Appilico.Server.UnitTests.Helpers;

public static class TestMapperConfig
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        return config.CreateMapper();
    }
}
