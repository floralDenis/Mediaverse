﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Mediaverse.Application.JointContentConsumption.Common.Dtos;
using Mediaverse.Application.JointContentConsumption.Queries.GetPlaylist.Dtos;
using Mediaverse.Domain.Common;
using Mediaverse.Domain.JointContentConsumption.Repositories;
using Microsoft.Extensions.Logging;

namespace Mediaverse.Application.JointContentConsumption.Queries.GetPlaylist
{
    public class GetPlaylistQueryHandler : IRequestHandler<GetPlaylistQuery, PlaylistDto>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ILogger<GetPlaylistQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetPlaylistQueryHandler(
            IRoomRepository roomRepository,
            IPlaylistRepository playlistRepository,
            ILogger<GetPlaylistQueryHandler> logger,
            IMapper mapper)
        {
            _roomRepository = roomRepository;
            _playlistRepository = playlistRepository;
            _logger = logger;
            _mapper = mapper;
        }
        
        public async Task<PlaylistDto> Handle(GetPlaylistQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var room = await _roomRepository.GetAsync(request.RoomId, cancellationToken) 
                           ?? throw new InformativeException("Room could not be found");

                if (!room.IsPlaylistSelected)
                {
                    throw new InformativeException("There is no playlist in the room");
                }
                
                var playlist = await _playlistRepository.GetAsync(room.ActivePlaylistId.Value, cancellationToken);
                var availablePlaylists = await _playlistRepository.GetAllByViewerAsync(
                    room.Host.Profile.Id, cancellationToken);

                var dto = _mapper.Map<PlaylistDto>(playlist);
                dto.AvailablePlaylists = _mapper.Map<IList<SelectablePlaylistDto>>(availablePlaylists);
                return dto;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Could not get playlist {request.RoomId}");
                throw new InformativeException("Could not get playlist. Please retry");
            }
        }
    }
}