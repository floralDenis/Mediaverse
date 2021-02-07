﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Mediaverse.Domain.JointContentConsumption.Entities;

namespace Mediaverse.Domain.JointContentConsumption.Repositories
{
    public interface IPlaylistRepository
    {
        Task<Playlist> GetAsync(Guid playlistId, CancellationToken cancellationToken);
        Task SaveAsync(Playlist playlist, CancellationToken cancellationToken);
    }
}