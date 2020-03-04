using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastFileSend.Main
{
    static class DynamicMapper
    {
        static IMapper Mapper { get; set; }

        static DynamicMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {

            });

            Mapper = configuration.CreateMapper();
        }

        public static TDestination Map<TDestination>(object source)
        {
            return Mapper.Map<TDestination>(source);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }
    }
}
