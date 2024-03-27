using Api.Interfaces;
using Api.Services;

namespace Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static void RegisterDependencies(IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICallRoomService, CallRoomService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IConnectedUserService, ConnectedUserService>();
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IJoinedRoomService, JoinedRoomService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IPeerService, PeerService>();
            services.AddScoped<IRoomModelService, RoomService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
