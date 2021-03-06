﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lib.AspNetCore.ServerSentEvents;
using MediatR;
using Mediaverse.Application.JointContentConsumption.Commands.AddContentToPlaylist;
using Mediaverse.Application.JointContentConsumption.Commands.ChangeActivePlaylist;
using Mediaverse.Application.JointContentConsumption.Commands.ChangeContentPlayerState;
using Mediaverse.Application.JointContentConsumption.Commands.CloseRoom;
using Mediaverse.Application.JointContentConsumption.Commands.CreateRoom;
using Mediaverse.Application.JointContentConsumption.Commands.DeletePlaylist;
using Mediaverse.Application.JointContentConsumption.Commands.JoinRoom;
using Mediaverse.Application.JointContentConsumption.Commands.LeaveRoom;
using Mediaverse.Application.JointContentConsumption.Commands.PlaySpecificContent;
using Mediaverse.Application.JointContentConsumption.Commands.RemoveContentFromPlaylist;
using Mediaverse.Application.JointContentConsumption.Commands.SwitchContent;
using Mediaverse.Application.JointContentConsumption.Common.Dtos;
using Mediaverse.Application.JointContentConsumption.Queries.GetAvailablePlaylists;
using Mediaverse.Application.JointContentConsumption.Queries.GetCurrentlyPlayingContent;
using Mediaverse.Application.JointContentConsumption.Queries.GetPlaylist;
using Mediaverse.Application.JointContentConsumption.Queries.GetPlaylist.Dtos;
using Mediaverse.Application.JointContentConsumption.Queries.GetPublicRooms;
using Mediaverse.Application.JointContentConsumption.Queries.GetRoom;
using Mediaverse.Infrastructure.Authentication.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mediaverse.Web.Controllers
{
    [Authorize]
    public class ContentConsumptionController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IServerSentEventsService _service;
        
        public ContentConsumptionController(
            IMediator mediator,
            IServerSentEventsService sentEventsService)
        {
            _mediator = mediator;
            _service = sentEventsService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> CreateRoom()
        {
            var viewerId = User.GetCurrentUserId();
            
            var query = new GetAvailablePlaylistsQuery {HostId = viewerId};
            var availablePlaylists = await _mediator.Send(query);
            
            // adding default option
            availablePlaylists.Add(new SelectablePlaylistDto
            {
                Id = default,
                Name = "Create new"
            });
            
            var command = new CreateRoomCommand {HostId = viewerId, AvailablePlaylists = availablePlaylists};
            return View(command);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRoom(CreateRoomCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var room = await _mediator.Send(command, cancellationToken);

                return Json(new
                {
                    redirectToUrl = @Url.Action(
                        "Room",
                        "ContentConsumption",
                        new {roomId = room.Id})
                });
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpPost]
        public async Task<ActionResult> CloseRoom(Guid roomId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CloseRoomCommand {RoomId = roomId, MemberId = User.GetCurrentUserId()};
                await _mediator.Send(command, cancellationToken);

                return Json(new
                {
                    redirectToUrl = @Url.Action("Index", "ContentConsumption")
                });
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult JoinRoomByLink(string roomToken)
        {
            return View("JoinRoomByLink", roomToken);
        }
        
        [HttpPost]
        public async Task<ActionResult> JoinRoom(string roomToken, CancellationToken cancellationToken)
        {
            try
            {
                var command = new JoinRoomCommand
                {
                    ViewerId = User.GetCurrentUserId(),
                    RoomToken = roomToken
                };
                var room = await _mediator.Send(command, cancellationToken);

                return Json(new
                {
                    redirectToUrl = @Url.Action(
                        "Room",
                        "ContentConsumption",
                        new {roomId = room.Id})
                });
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpPost]
        public async Task LeaveRoom(Guid roomId)
        {
            var command = new LeaveRoomCommand {RoomId = roomId, ViewerId = User.GetCurrentUserId()};
            await _mediator.Send(command, CancellationToken.None);
        }

        [HttpPost]
        public async Task<ActionResult> AddContentToPlaylist(
            AddContentToPlaylistCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);
                
                await _service.SendEventAsync(
                    "playlist_updated",
                    GetClientPredicate(affectedViewers),
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> RemoveContentFromPlaylist(
            RemoveContentFromPlaylistCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);

                await _service.SendEventAsync(
                    "playlist_updated",
                    GetClientPredicate(affectedViewers),
                    cancellationToken);
                
                return Ok();
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> SwitchContent(SwitchContentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);
                
                await _service.SendEventAsync(
                    "switched_content",
                    GetClientPredicate(affectedViewers),
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpPost]
        public async Task<ActionResult> PlaySpecificContent(PlaySpecificContentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);
                
                await _service.SendEventAsync(
                    "switched_content",
                    GetClientPredicate(affectedViewers),
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> ChangeContentPlayerState(
            ChangeContentPlayerStateCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);

                var messageObject = new 
                    { state = command.State, currentTime = command.CurrentPlaybackTimeInSeconds };
                string message = JsonConvert.SerializeObject(messageObject);
                
                await _service.SendEventAsync(
                    message,
                    GetClientPredicate(affectedViewers),
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            { 
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpPost]
        public async Task<ActionResult> ChangeActivePlaylist(ChangeActivePlaylistCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var affectedViewers = await _mediator.Send(command, cancellationToken);
                
                await _service.SendEventAsync(
                    "playlist_updated",
                    GetClientPredicate(affectedViewers),
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            { 
                return BadRequest(new {message = exception.Message});
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> DeletePlaylist(Guid roomId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeletePlaylistCommand {MemberId = User.GetCurrentUserId(), RoomId = roomId};
                var affectedViewers = await _mediator.Send(command, cancellationToken);
                
                await _service.SendEventAsync(
                    "playlist_updated",
                    GetClientPredicate(affectedViewers), 
                    cancellationToken);

                return Ok();
            }
            catch (Exception exception)
            { 
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpGet]
        public async Task<ActionResult> Room(Guid roomId, CancellationToken cancellationToken)
        {
            var query = new GetRoomQuery {RoomId = roomId};
            var room = await _mediator.Send(query, cancellationToken);
            return View(room);
        }

        [HttpGet]
        public async Task<ActionResult> Playlist(Guid roomId, CancellationToken cancellationToken)
        {
            var query = new GetPlaylistQuery {RoomId = roomId};
            var playlist = await _mediator.Send(query, cancellationToken);
            return PartialView(playlist);
        }
        
        [HttpGet]
        public async Task<ActionResult> CurrentlyPlayingContent(Guid roomId, CancellationToken cancellationToken)
        {
            var query = new GetCurrentlyPlayingContentQuery {RoomId = roomId}; 
            var roomDto = await _mediator.Send(query, cancellationToken);
            return PartialView("ContentPlayer", roomDto);
        }

        [HttpGet]
        public async Task<ActionResult> GetPublicRooms(CancellationToken cancellationToken)
        {
            var query = new GetPublicRoomsQuery();
            var rooms = await _mediator.Send(query, cancellationToken);
            return PartialView("PublicRooms", rooms);
        }

        private Func<IServerSentEventsClient, bool> GetClientPredicate(AffectedViewersDto affectedViewersDto) =>
            client => affectedViewersDto.ViewerIds.Contains(client.User.GetCurrentUserId());
    }
}