using Api.Hubs;
using Api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services
{
    public class UserCleanupService : BackgroundService
    {
        private readonly ILogger<UserCleanupService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserCleanupService(ILogger<UserCleanupService> logger, IHubContext<ChatHub> hubContext, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("User cleanup service is running.");
                //using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                //{
                //    IJoinedRoomService joinedRoomService = scope.ServiceProvider.GetRequiredService<IJoinedRoomService>();

                //    var users = await joinedRoomService.GetAll();
                //    foreach (var user in users)
                //    {
                //        try
                //        {
                //            await _hubContext.Groups.AddToGroupAsync(user.SocketId, "temp_group");
                //            _logger.LogInformation($"Client with socket ID {user.SocketId} is connected.");
                //        }
                //        catch (Exception ex)
                //        {
                //            await joinedRoomService.DeleteBySocketIdAsync(user.SocketId);
                //            _logger.LogInformation($"Client with socket ID {user.SocketId} is not connected.");
                //        }
                //        finally
                //        {
                //            await _hubContext.Groups.RemoveFromGroupAsync(user.SocketId, "temp_group");
                //        }
                //    }
                //}


                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task<bool> IsSocketIdValid(string socketId)
        {



            // Замените этот метод на вашу конкретную логику проверки существования SocketId.
            return true;
        }
    }
}
