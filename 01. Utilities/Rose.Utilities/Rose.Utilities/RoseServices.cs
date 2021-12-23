using Microsoft.Extensions.Logging;
using Rose.Utilities.Services.Chaching;
using Rose.Utilities.Services.Localizations;
using Rose.Utilities.Services.ObjectMappers;
using Rose.Utilities.Services.Serializers;
using Rose.Utilities.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose.Utilities
{
    public class RoseServices
    {
        public readonly ITranslator Translator;
        public readonly ICacheAdapter CacheAdapter;
        public readonly IMapperAdapter MapperFacade;
        public readonly ILoggerFactory LoggerFactory;
        public readonly IJsonSerializer Serializer;
        public readonly IUserInfoService UserInfoService;

        public RoseServices(ITranslator translator,
                ILoggerFactory loggerFactory,
                IJsonSerializer serializer,
                IUserInfoService userInfoService,
                ICacheAdapter cacheAdapter,
                IMapperAdapter mapperFacade)
        {
            Translator = translator;
            LoggerFactory = loggerFactory;
            Serializer = serializer;
            UserInfoService = userInfoService;
            CacheAdapter = cacheAdapter;
            MapperFacade = mapperFacade;
        }
    }
}
