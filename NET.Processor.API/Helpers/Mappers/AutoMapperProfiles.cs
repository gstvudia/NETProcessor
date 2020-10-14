using AutoMapper;
using NET.Processor.API.Models.DTO;
using NET.Processor.Core.Models;
using System.Linq;

namespace NET.Processor.API.Helpers.Mappers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Item, NodeData>()
                .ForMember(property => property.id, options => options.MapFrom(
                    source => source.Id))
                .ForMember(property => property.name, options => options.MapFrom(
                    source => source.Name));
        }
    }
}
