using AutoMapper;
using TreeApi.DTOs;
using TreeApi.Models;

namespace TreeApi.Utils
{
    public class TreeProfile : Profile
    {
        public TreeProfile()
        {
            CreateMap<Node, NodeDto>()
                .ForMember(dest => dest.HasChildren,
                           opt => opt.MapFrom(src => src.Children.Any()))
                .ForMember(dest => dest.Children,
                           opt => opt.MapFrom(src => src.Children));

            CreateMap<NodeCreateDto, Node>();
        }
    }
}
