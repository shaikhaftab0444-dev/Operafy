using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ERP_System.Hubs
{
    public class ErpNotificationHub : Hub
    {
        public async Task JoinRoleGroup(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roleName);
            }
        }

        public async Task JoinDepartmentGroup(string departmentName)
        {
            if (!string.IsNullOrWhiteSpace(departmentName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, departmentName);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
