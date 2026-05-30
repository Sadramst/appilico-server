using AutoMapper;
using AppilicoShopServer.Business.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppilicoShopServer.UnitTests.Helpers;

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
