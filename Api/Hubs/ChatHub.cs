using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly IConnectedUserService _connectedUserService;
        private readonly IJoinedRoomService _joinedRoomService;
        private readonly IMessageService _messageService;
        private readonly ICallRoomService _callService;
        private readonly IChatService _chatService;

        public ChatHub(
            IAuthService authService,
            IUserService userService,
            IRoomService roomService,
            IConnectedUserService connectedUserService,
            IJoinedRoomService joinedRoomService,
            IMessageService messageService,
            ICallRoomService callService,
            IChatService chatService)
        {
            _authService = authService;
            _userService = userService;
            _roomService = roomService;
            _connectedUserService = connectedUserService;
            _joinedRoomService = joinedRoomService;
            _messageService = messageService;
            _callService = callService;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();

            try
            {
                var decodedToken = await _authService.VerifyJwt(authorizationHeader);
                var user = await _userService.FindByEmailAsync(decodedToken.FindFirstValue(ClaimTypes.Email));
                if (user == null)
                {
                    await Disconnect();
                }
                else
                {
                    Context.Items["User"] = user;
                    var rooms = await _roomService.GetRoomsForUser(user.Id, 1, 20);
                    await _connectedUserService.CreateAsync(new ConnectedUserModel { SocketId = Context.ConnectionId, User = user });
                    await Clients.Caller.SendAsync("rooms", rooms);
                }
            }
            catch
            {
                await Disconnect();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var joinUsers = await _joinedRoomService.FindBySocketIdAsync(Context.ConnectionId);
                foreach (var user in joinUsers)
                {
                    await Clients.Client(user.SocketId).SendAsync("userLeaved");
                }
            }
            catch
            {
            }
            finally
            {
                await Disconnect();
            }
        }

        private async Task Disconnect()
        {
            await Clients.Caller.SendAsync("Error");
            Context.Abort();
        }

        public async Task CreateRoom(RoomModel room)
        {
            var createdRoom = await _roomService.CreateRoom(room, (UserModel)Context.Items["User"]);//check
            foreach (var user in createdRoom.Users)
            {
                var rooms = await _roomService.GetRoomsForUser(user.Id, 1, 20);
                await Clients.Caller.SendAsync("rooms", rooms);
            }
        }

        public async Task PaginateRooms(int page)
        {
            try
            {
                var rooms = await _roomService.GetRoomsForUser(
                    ((UserModel)Context.Items["User"]).Id,
                    page, 20
                );
                await Clients.Caller.SendAsync("rooms", rooms);
            }
            catch
            {
                // Log error
            }
        }

        public async Task JoinRoom(RoomModel room)
        {
            var messages = await _messageService.FindMessagesForRoom(room, new PaginationOptions { Limit = 50, Page = 1 });
            var roomHandler = await _roomService.GetRoomById(room.Id);
            var callRoom = await _callService.FindByRoom(roomHandler);
            var joinUsers = await _joinedRoomService.FindByRoomAsync(roomHandler);

            await _joinedRoomService.CreateAsync(new JoinedRoomModel { SocketId = Context.ConnectionId, User = (UserModel)Context.Items["User"], Room = room });
            var shareString = "qwerty";
            //_configService.Get("CLIENT_SERVER") + $"/connect?token={await _chatService.EncryptToken(roomHandler.Id)}";
            var onlineCount = await _joinedRoomService.CountByRoomAsync(roomHandler);

            await Clients.Caller.SendAsync("messages", new
            {
                messages,
                room = roomHandler,
                callRoom,
                shareString,
                onlineCount
            });

            foreach (var user in joinUsers)
            {
                await Clients.Client(user.SocketId).SendAsync("userJoined", new { onlineCount });
            }
        }

        public async Task PuginateMessages((RoomModel, int) args)
        {
            var messages = await _messageService.FindMessagesForRoom(
                args.Item1,
                new PaginationOptions { Limit = 50, Page = args.Item2 }
            );
            await Clients.Caller.SendAsync("messagePaginated", messages);
        }

    }
}
