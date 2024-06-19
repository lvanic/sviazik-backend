using Api.Interfaces;
using Api.Models;
using Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly IPeerService _peerService;
        private readonly IConfiguration _configuration;
        private readonly string x = string.Empty;

        public ChatHub(
            IAuthService authService,
            IUserService userService,
            IRoomService roomService,
            IConnectedUserService connectedUserService,
            IJoinedRoomService joinedRoomService,
            IMessageService messageService,
            ICallRoomService callService,
            IChatService chatService,
            IPeerService peerService,
            IConfiguration configuration)
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
            _configuration = configuration;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var context = Context.GetHttpContext();
                var user = await _userService.FindByEmailAsync(context.User.FindFirstValue(ClaimTypes.Email));
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
                    await Clients.Client(user.SocketId).SendAsync("userLeaved", new { onlineCount = joinUsers.Count() - 1 });
                }

                await _connectedUserService.DeleteBySocketIdAsync(Context.ConnectionId);


                var userC = (UserModel)Context.Items["User"];
                var userDb = await _userService.GetOneAsync(userC.Id);
                await _joinedRoomService.DeleteByUserIdAsync(userDb.Id);
                var call = await _callService.FindByUser(userDb);
                if (call != null)
                {
                    var peerUser = await _peerService.GetOneByUserAsync(userDb);
                    await _peerService.DeletePeerUserAsync(peerUser);
                    var connectedUsers = await _connectedUserService.FindByRoomAsync(call.Room);

                    var answer = new AnswerDisconnectCall { PeerId = peerUser.PeerId };

                    foreach (var user in connectedUsers)
                    {
                        await Clients.User(user.UserId.ToString()).SendAsync("userDisconected", answer);
                    }

                    if (call.PeersUsers.Count == 0)
                    {
                        await _callService.DeleteOne(call);
                    }
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
            var createdRoom = await _roomService.CreateRoom(room, (UserModel)Context.Items["User"]);

            var user = createdRoom.Users.FirstOrDefault();
            var rooms = await _roomService.GetRoomsForUser(user.Id, 1, 20);
            await Clients.Caller.SendAsync("rooms", rooms);

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
            var userDb = await _userService.FindByEmailAsync(Context.User.FindFirstValue(ClaimTypes.Email));
            await _joinedRoomService.CreateAsync(new JoinedRoomModel { SocketId = Context.ConnectionId, User = userDb, Room = roomHandler });
            var shareString = _configuration["ClientHost"] + $"/connect?token={_chatService.EncryptToken(roomHandler.Id)}";
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

        public async Task PuginateMessages(RoomModel room, int page)
        {
            var messages = await _messageService.FindMessagesForRoom(
                room,
                new PaginationOptions { Limit = 50, Page = page }
            );
            await Clients.Caller.SendAsync("messagePaginated", messages);
        }

        public async Task UpdateMessage(MessageModel message)
        {
            var messageId = message.Id;
            var messageText = message.Text;
            var userHandler = Context.Items["User"] as UserModel;

            var messageHandler = await _messageService.GetOne(messageId);
            if (messageHandler.UserId == userHandler.Id)
            {
                await _messageService.UpdateMessage(messageId, messageText);

                var roomHandler = await _roomService.GetRoomByMessage(messageId);
                var joinedRoomUsers = await _joinedRoomService.FindByRoomAsync(roomHandler);
                foreach (var user in joinedRoomUsers)
                {
                    await Clients.Client(user.SocketId).SendAsync("messageUpdated", new { id = messageId, text = messageText });
                }
            }
            else
            {
                Console.WriteLine("Недостаточно прав");
            }
        }

        public async Task RemoveUser(string username, int roomId)
        {
            var user = Context.Items["User"] as UserModel;
            var removeUser = await _userService.FindByUsernameAsync(username);
            var room = await _roomService.GetRoomById(roomId);

            if (room.Admins.Any(x => x.Id == user.Id))
            {
                await _roomService.RemoveUser(room.Id, removeUser);

                var messageHandler = new MessageModel
                {
                    Text = $"{user.Username} выгнал {removeUser.Username}",
                    RoomId = room.Id,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    MessageType = MessageType.System
                };
                var createdMessage = await _messageService.Create(messageHandler);
                var joinedUsers = await _joinedRoomService.FindByRoomAsync(room);

                foreach (var userJoin in joinedUsers)
                {
                    if (userJoin.UserId != removeUser.Id)
                    {
                        await Clients.Client(userJoin.SocketId).SendAsync("messageAdded", createdMessage);

                        var messages = await _messageService.FindMessagesForRoom(room, new PaginationOptions { Limit = 50, Page = 1 });
                        var roomHandler = await _roomService.GetRoomById(room.Id);
                        var callRoom = await _callService.FindByRoom(roomHandler);
                        var shareString = _configuration["ClientHost"] + $"/connect?token={_chatService.EncryptToken(roomHandler.Id)}";
                        var onlineCount = await _joinedRoomService.CountByRoomAsync(roomHandler);
                        await Clients.Client(userJoin.SocketId).SendAsync("messages", new
                        {
                            messages,
                            room = roomHandler,
                            callRoom,
                            shareString,
                            onlineCount
                        });
                    }
                    else
                    {
                        await Clients.Client(userJoin.SocketId).SendAsync("removedFromRoom", room);
                    }
                }
            }
            else
            {
                Console.WriteLine("Недостаточно прав");
            }
        }

        public async Task EnterRoom(RoomModel room)
        {
            var userDb = await _userService.FindByEmailAsync(Context.User.FindFirstValue(ClaimTypes.Email));
            var roomHandler = await _roomService.EnterRoom(room, userDb);


            var connectedUser = await _connectedUserService.FindByRoomAsync(room);
            var joinedUsers = await _joinedRoomService.FindByRoomAsync(room);

            if (roomHandler != null)
            {
                var messageHandler = new MessageModel
                {
                    Text = $"{((UserModel)Context.Items["User"]).Username} вошел в комнату {roomHandler.Name}",
                    RoomId = roomHandler.Id,
                    UserId = ((UserModel)Context.Items["User"]).Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    MessageType = MessageType.System

                };
                var createdMessage = await _messageService.Create(messageHandler);

                foreach (var user in connectedUser)
                {
                    if (joinedUsers.Any(joinedUser => joinedUser.UserId == user.User.Id))
                    {
                        await Clients.Client(user.SocketId).SendAsync("messageAdded", createdMessage);

                        var messages = await _messageService.FindMessagesForRoom(room, new PaginationOptions { Limit = 50, Page = 1 });
                        var roomHand = await _roomService.GetRoomById(room.Id);
                        var callRoom = await _callService.FindByRoom(roomHandler);
                        var shareString = _configuration["ClientHost"] + $"/connect?token={_chatService.EncryptToken(roomHandler.Id)}";
                        var onlineCount = await _joinedRoomService.CountByRoomAsync(roomHand);

                        await Clients.Client(user.SocketId).SendAsync("messages", new
                        {
                            messages,
                            room = roomHandler,
                            callRoom,
                            shareString,
                            onlineCount
                        });
                    }
                    else
                    {
                        var notify = new
                        {
                            ChatId = room.Id,
                            Message = $"{((UserModel)Context.Items["User"]).Username} вошел в комнату {room.Name}",
                            ChatName = room.Name
                        };
                        await Clients.Client(user.SocketId).SendAsync("notify", notify);
                    }
                }
            }

            var userId = ((UserModel)Context.Items["User"]).Id;
            var rooms = await _roomService.GetRoomsForUser(userId, 1, 20);

            await Clients.Client(Context.ConnectionId).SendAsync("rooms", rooms);
        }

        public async Task RefreshRoom(RoomModel room)
        {
            var messages = await _messageService.FindMessagesForRoom(room, new PaginationOptions { Limit = 50, Page = 1 });
            var roomHandler = await _roomService.GetRoomById(room.Id);
            var callRoom = await _callService.FindByRoom(roomHandler);
            var shareString = _configuration["ClientHost"] + $"/connect?token={_chatService.EncryptToken(roomHandler.Id)}";

            var onlineCount = await _joinedRoomService.CountByRoomAsync(roomHandler);
            await Clients.Client(Context.ConnectionId).SendAsync("messages", new
            {
                messages,
                room = roomHandler,
                callRoom,
                shareString,
                onlineCount
            });
        }

        public async Task DeleteRoom(RoomModel room)
        {
            var roomToRemove = await _roomService.GetRoomById(room.Id);
            var jonedUsers = await _joinedRoomService.FindByRoomAsync(room);
            foreach(var jonedUser in jonedUsers)
            {
                await Clients.Client(jonedUser.SocketId).SendAsync("removedFromRoom", room);
            }
            await _roomService.DeleteRoom(roomToRemove);

        }

        public async Task LeaveRoom()
        {
            var joinUsers = await _joinedRoomService.FindBySocketIdAsync(Context.ConnectionId);
            await _joinedRoomService.DeleteBySocketIdAsync(Context.ConnectionId);
            var onlineCount = await _joinedRoomService.CountByRoomAsync(joinUsers.First().Room);

            foreach (var user in joinUsers)
            {
                await Clients.Client(user.SocketId).SendAsync("userLeaved", new { onlineCount });
            }
        }
        public async Task ExitRoom(RoomModel room)
        {
            var userDb = await _userService.FindByEmailAsync(Context.User.FindFirstValue(ClaimTypes.Email));
            var joinUsers = await _joinedRoomService.FindBySocketIdAsync(Context.ConnectionId);
            await _joinedRoomService.DeleteBySocketIdAsync(Context.ConnectionId);
            await _roomService.RemoveUser(room.Id, userDb);

            var onlineCount = await _joinedRoomService.CountByRoomAsync(joinUsers.First().Room);

            await Clients.Caller.SendAsync("removedFromRoom", room);

            foreach (var user in joinUsers)
            {
                await Clients.Client(user.SocketId).SendAsync("userLeaved", new { onlineCount });
            }
        }

        public async Task AddMessage(MessageModel message)
        {
            var room = await _roomService.GetRoomById(message.Room.Id);
            var messageHandler = message;
            messageHandler.UserId = ((UserModel)Context.Items["User"]).Id;
            messageHandler.Room = room;
            messageHandler.MessageType = MessageType.User;

            var createdMessage = await _messageService.Create(messageHandler);

            room.UpdatedAt = DateTime.Now;
            var roomHandler = await _roomService.UpdateRoom(room);

            var connectedUser = await _connectedUserService.FindByRoomAsync(room);
            var joinedUsers = await _joinedRoomService.FindByRoomAsync(room);

            foreach (var user in connectedUser)
            {
                await Clients.Client(user.SocketId).SendAsync("updateChat", roomHandler);
                if (joinedUsers.Any(joinedUser => joinedUser.UserId == user.User.Id))
                {
                    await Clients.Client(user.SocketId).SendAsync("messageAdded", createdMessage);
                }
                else
                {
                    var notify = new
                    {
                        ChatId = room.Id,
                        Message = $"{((UserModel)Context.Items["User"]).Username} написал сообщение в комнате {room.Name}",
                        ChatName = room.Name
                    };
                    await Clients.Client(user.SocketId).SendAsync("notify", notify);
                }
            }
        }

        public async Task UploadImage(IFormFile file, int chatId)
        {
            var contextUser = (UserModel)Context.Items["User"];
            var user = await _userService.GetOneAsync(contextUser.Id);
            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("http://localhost:3003"); // Адрес вашего Node.js сервера

            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await httpClient.PostAsync("/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var message = new MessageModel
                {
                    Room = new RoomModel { Id = chatId },
                    Text = responseContent,
                    User = user,
                    MessageType = MessageType.User
                };
                await AddMessage(message);
            }
            else
            {
                Console.WriteLine("Ошибка при загрузке изображения");
            }
        }

        public async Task DeleteMessage(MessageModel message)
        {
            var messageHandler = await _messageService.GetOne(message.Id);
            var roomHandler = await _roomService.GetRoomById(messageHandler.Room.Id);
            var joinedRoomUsers = await _joinedRoomService.FindByRoomAsync(roomHandler);
            var deleted = await _messageService.DeleteMessage(messageHandler.Id);
            if (deleted)
            {
                foreach (var user in joinedRoomUsers)
                {
                    await Clients.Client(user.SocketId).SendAsync("messageDeleted", new { id = messageHandler.Id });
                }
            }
        }
        public async Task AddSecureMessage(MessageModel message)
        {
            var messageHandler = message;
            messageHandler.User = ((UserModel)Context.Items["User"]);
            message.MessageType = MessageType.User;
            var room = await _roomService.GetRoomById(message.Room.Id);
            var joinedUsers = await _joinedRoomService.FindByRoomAsync(room);

            foreach (var user in joinedUsers)
            {
                await Clients.Client(user.SocketId).SendAsync("messageAdded", messageHandler);
            }
        }

        public async Task SearchRooms(string name)
        {
            var rooms = await _roomService.GetRoomsByName(name);
            await Clients.Client(Context.ConnectionId).SendAsync("searchedRooms", rooms);
        }

        public async Task CallRequest(CallRequestData data)
        {
            var room = await _roomService.GetRoomById(data.Room.Id);
            var userId = ((UserModel)Context.Items["User"]).Id;
            var user = await _userService.GetOneAsync(userId);
            var connectedUsers = await _connectedUserService.FindByRoomAsync(data.Room);
            var joinedUsers = await _joinedRoomService.FindByRoomAsync(data.Room);

            var call = new CallRoomModel
            {
                Room = room,
            };

            try
            {
                var callHandler = await _callService.Create(call, user, data.PeerId);
                var peerUser = new UserPeerModel
                {
                    User = user,
                    PeerId = data.PeerId,
                    Call = callHandler
                };
                await _peerService.AddPeerUserAsync(peerUser);

                foreach (var connectedUser in connectedUsers)
                {
                    if (joinedUsers.Any(joinedUser => joinedUser.UserId == connectedUser.UserId))
                    {
                        if (connectedUser.Id != Convert.ToInt32(Context.UserIdentifier))
                        {
                            await Clients.User(connectedUser.UserId.ToString()).SendAsync("CallRequest", new
                            {
                                data.PeerId,
                                data.Room,
                                From = user
                            });
                        }
                    }
                    else
                    {
                        var notify = new//notify
                        {
                            ChatId = data.Room.Id,
                            Message = $"{user.Username} начал звонок в комнате {data.Room.Name}",
                            ChatName = data.Room.Name
                        };
                        await Clients.User(connectedUser.UserId.ToString()).SendAsync("Notify", notify);
                    }
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                var callHandler = await _callService.FindByRoom(room);
                var peerUser = new UserPeerModel
                {
                    User = user,
                    PeerId = data.PeerId,
                    Call = callHandler
                };
                await _peerService.AddPeerUserAsync(peerUser);
            }
        }

        public async Task ConnectRoom(ConnectRoomData data)
        {
            var room = await _roomService.GetRoomById(data.Room.Id);
            var user = await _userService.GetOneAsync(((UserModel)Context.Items["User"]).Id);

            var call = await _callService.FindByRoom(room);
            await Clients.Caller.SendAsync("Peers", call);

            var peerUser = new UserPeerModel
            {
                User = user,
                PeerId = data.PeerId,
                Call = call
            };
            await _peerService.AddPeerUserAsync(peerUser);
        }

        public async Task DisconnectCall(DisconnectCallData data)
        {
            var userDb = await _userService.GetOneAsync(((UserModel)Context.Items["User"]).Id);

            var call = await _callService.FindByUser(userDb);

            var peerUser = await _peerService.GetOneByUserAsync(userDb);
            await _peerService.DeletePeerUserAsync(peerUser);

            var connectedUsers = await _connectedUserService.FindByRoomAsync(data.Room);
            var joinedUsers = await _joinedRoomService.FindByRoomAsync(data.Room);
            var answer = new AnswerDisconnectCall { PeerId = peerUser.PeerId };
            //await Clients.All.SendAsync("userDisconected", answer);
            foreach (var user in connectedUsers)
            {
                await Clients.User(user.UserId.ToString()).SendAsync("userDisconected", answer);
            }


            if (call.PeersUsers.Count == 0)
            {
                await _callService.DeleteOne(call);
            }
        }

    }
}
