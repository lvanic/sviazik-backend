using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Api.Hubs
{

    public class CallRequestData
    {
        public string PeerId { get; set; }
        public RoomModel Room { get; set; }
    }

    public class ConnectRoomData
    {
        public string PeerId { get; set; }
        public RoomModel Room { get; set; }
    }
    public class DisconnectCallData
    {
        public string PeerId { get; set; }
        public RoomModel Room { get; set; }
    }
    public class AnswerDisconnectCall
    {
        public string PeerId { get; set; }

        [Authorize]
        public class CallHub : Hub
        {
            private readonly IAuthService _authService;
            private readonly IUserService _userService;
            private readonly IRoomService _roomService;
            private readonly IConnectedUserService _connectedUserService;
            private readonly IJoinedRoomService _joinedRoomService;
            private readonly IMessageService _messageService;
            private readonly ICallRoomService _callService;
            private readonly IChatService _chatService;
            private readonly IPeerService _peerService;

            public CallHub(
                IAuthService authService,
                IUserService userService,
                IRoomService roomService,
                IConnectedUserService connectedUserService,
                IJoinedRoomService joinedRoomService,
                IMessageService messageService,
                ICallRoomService callService,
                IChatService chatService,
                IPeerService peerService)
            {
                _authService = authService;
                _userService = userService;
                _roomService = roomService;
                _connectedUserService = connectedUserService;
                _joinedRoomService = joinedRoomService;
                _messageService = messageService;
                _callService = callService;
                _chatService = chatService;
                _peerService = peerService;
            }






        }
    }
}

