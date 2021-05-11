﻿using System;
using MediatR;
using Mediaverse.Application.JointContentConsumption.Common.Dtos;

namespace Mediaverse.Application.JointContentConsumption.Commands.ChangeActivePlaylist
{
    public class ChangeActivePlaylistCommand : IRequest<AffectedViewers>
    {
        public Guid RoomId { get; set; }
        public Guid PlaylistId { get; set; }
    }
}