﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Mediaverse.Domain.ContentSearch.Entities;
using Mediaverse.Domain.ContentSearch.Enums;
using Mediaverse.Domain.ContentSearch.ValueObjects;
using SearchResult = Mediaverse.Domain.ContentSearch.ValueObjects.SearchResult;
using Thumbnail = Mediaverse.Domain.ContentSearch.ValueObjects.Thumbnail;
using YouTubeData = Google.Apis.YouTube.v3.Data;

namespace Mediaverse.Infrastructure.ContentSearch.Mapping
{
    public class YouTubeProfile : Profile
    {
        private readonly Dictionary<string, MediaContentType> _contentTypeMappings = new Dictionary<string, MediaContentType>
        {
            { "youtube#video", MediaContentType.Video }
        };

        public YouTubeProfile() => ConfigureMappings();

        private void ConfigureMappings()
        {
            CreateMap<YouTubeData.SearchListResponse, SearchResult>()
                .ForMember(dest => dest.MatchingContentPreviews, o => o.MapFrom(src => src.Items));

            CreateMap<YouTubeData.VideoListResponse, SearchResult>()
                .ForMember(dest => dest.RequestedContent, o => o.MapFrom(src => src.Items.FirstOrDefault()));
            
            CreateMap<YouTubeData.SearchResult, Preview>()
                .ConstructUsing(x => new Preview(
                    x.Id.VideoId,
                    MediaContentSource.YouTube,
                    _contentTypeMappings[x.Id.Kind],
                    x.Snippet.Title,
                    x.Snippet.Description,
                    new Thumbnail(
                        x.Snippet.Thumbnails.Default__.Width.GetValueOrDefault(),
                        x.Snippet.Thumbnails.Default__.Height.GetValueOrDefault(),
                        x.Snippet.Thumbnails.Default__.Url)));

            CreateMap<YouTubeData.Video, Content>()
                .ConstructUsing(src => new Content(
                    new ContentId(
                        src.Id,
                        MediaContentSource.YouTube,
                        _contentTypeMappings[src.Kind]),
                    src.Snippet.Title,
                    new Thumbnail(
                        src.Snippet.Thumbnails.Default__.Width.GetValueOrDefault(),
                        src.Snippet.Thumbnails.Default__.Height.GetValueOrDefault(),
                        src.Snippet.Thumbnails.Default__.Url),
                    src.Player.EmbedHtml,
                    (int) src.Player.EmbedWidth.GetValueOrDefault(),
                    (int) src.Player.EmbedHeight.GetValueOrDefault(),
                    src.Snippet.Description));
        }
    }
}