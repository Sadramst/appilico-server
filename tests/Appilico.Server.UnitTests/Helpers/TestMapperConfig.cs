using AutoMapper;
using Appilico.Server.Business.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Appilico.Server.UnitTests.Helpers;

public static class TestMapperConfig
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);
        return config.CreateMapper();
    }
}
