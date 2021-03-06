﻿using System;
using System.Collections.Generic;
using Mediaverse.Domain.Common;
using Mediaverse.Domain.JointContentConsumption.Enums;
using Mediaverse.Domain.JointContentConsumption.ValueObjects;

namespace Mediaverse.Domain.JointContentConsumption.Entities
{
    public class Room : Entity
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new InformativeException("Given name is invalid");
                }

                _name = value;
            }
        }
        public string Description { get; set; }

        public RoomType Type { get; private set; } = RoomType.Public;
        private Invitation _invitation;

        public Invitation Invitation
        {
            get => _invitation;
            set => _invitation = value 
                                 ?? throw new InvalidOperationException("Room requires invitation");
        }
        
        public Viewer Host { get; private set; }

        private IList<Viewer> _viewers;
        private int _maxViewersQuantity = 20;
        
        public Guid? ActivePlaylistId { get; private set; }
        public bool IsPlaylistSelected => ActivePlaylistId.HasValue;
        
        public CurrentContent CurrentContent { get; set; }
        
        public IReadOnlyList<Viewer> Viewers => (IReadOnlyList<Viewer>)_viewers;
        public int MaxViewersQuantity
        {
            get => _maxViewersQuantity;
            set
            {
                if (value < 1)
                {
                    throw new InformativeException("Max viewers quantity should be more than 1");
                }

                _maxViewersQuantity = value;
            }
        }

        public Room(
            Guid id,
            string name,
            Viewer host,
            RoomType type,
            Invitation invitation,
            Guid? playlistId,
            string description = "") : base(id)
        {
            try
            {
                Name = name;
                Description = description;
                Host = host;
                Type = type;
                Invitation = invitation;
                ActivePlaylistId = playlistId;
                _viewers = new List<Viewer>();
            }
            catch (InformativeException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Could not create room", exception);
            }
        }

        public Room(
            Guid id,
            string name,
            string description,
            Viewer host,
            RoomType type,
            Invitation invitation,
            int maxViewersQuantity,
            Guid? activePlaylistId,
            IList<Viewer> viewers,
            CurrentContent currentContent) : base(id)
        {
            Name = name;
            Description = description;
            Host = host;
            Type = type;
            Invitation = invitation;
            MaxViewersQuantity = maxViewersQuantity;
            ActivePlaylistId = activePlaylistId;
            _viewers = viewers ?? new List<Viewer>();
            CurrentContent = currentContent;
        }

        public void UpdateSelectedPlaylist(Playlist playlist)
        {
            _ = playlist ?? throw new ArgumentNullException(nameof(playlist));
            if (!playlist.Owner.Equals(Host))
            {
                throw new InformativeException($"Playlist {playlist} does not belong toT host {Host}");
            }

            ActivePlaylistId = playlist.Id;
        }

        public void Join(Viewer viewer)
        {
            if (!IsSpotAvailable)
            {
                throw new InformativeException("There is no spot for the viewer");
            }
            
            _ = viewer ?? throw new ArgumentNullException(nameof(viewer));

            if (_viewers.Contains(viewer))
            {
                return;
            }
            
            _viewers.Add(viewer);
        }

        public void Leave(Viewer viewer)
        {
            _ = viewer ?? throw new ArgumentNullException(nameof(viewer));
            if (!_viewers.Contains(viewer) && !viewer.Equals(Host))
            {
                throw new InformativeException("Viewer is not in the room");
            }

            bool isHostLeavingTheRoom = Host.Equals(viewer);
            
            if (isHostLeavingTheRoom)
            {
                SelectNewHost();
            }
            else
            {
                _viewers.Remove(viewer);
            }
        }

        public bool IsVacated() => _viewers.Count == 0 && Host == null;

        public void PlayContent(ContentId contentId)
        {
            _ = contentId ?? throw new ArgumentNullException(nameof(contentId));
            
            CurrentContent = new CurrentContent(
                contentId,
                ContentPlayerState.Paused,
                playingTime: 0,
                lastUpdatedPlayingTime: DateTime.Now);
        }
        
        private bool IsSpotAvailable => _viewers.Count < _maxViewersQuantity;

        private void SelectNewHost()
        {
            var random = new Random();

            if (_viewers.Count > 0)
            {
                int newHostIndex =
                    random.Next(0, _viewers.Count - 1);
                Host = _viewers[newHostIndex];

                _viewers.Remove(Host);
            }
            else
            {
                Host = null;
            }
        }
    }
}