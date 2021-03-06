﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mediaverse.Domain.JointContentConsumption.Entities;

namespace Mediaverse.Domain.JointContentConsumption.Repositories
{
    public interface IPlaylistRepository
    {
        Task<Playlist> GetAsync(Guid playlistId, CancellationToken cancellationToken);
        Task<IList<Playlist>> GetAllByViewerAsync(Guid ownerId, CancellationToken cancellationToken);
        Task AddAsync(Playlist playlist, CancellationToken cancellationToken);
        Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken);
        Task DeleteAsync(Guid playlistId, CancellationToken cancellationToken);
    }
}