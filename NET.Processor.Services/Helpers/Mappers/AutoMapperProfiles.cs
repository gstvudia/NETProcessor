using AutoMapper;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;

namespace NET.Processor.Core.Helpers.Mappers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Item, NodeRoot>()
                .ForMember(property => property.Id, options => options.MapFrom(
                    source => source.Id))
                .ForMember(property => property.Name, options => options.MapFrom(
                    source => source.Name));
        }
    }
}
