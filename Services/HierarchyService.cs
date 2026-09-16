using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Services
{
    public interface IHierarchyService
    {
        Task<List<int>> GetSubordinateUserIntIdsAsync(User currentManager);
        Task<List<string>> GetSubordinateUserIdsAsync(User currentManager);
        Task<List<User>> GetSubordinateUsersAsync(User currentManager);
        Task<bool> IsSubordinateAsync(User currentManager, int targetUserId);
    }

    public class HierarchyService : IHierarchyService
    {
        private readonly ApplicationDbContext _context;

        public HierarchyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<int>> GetSubordinateUserIntIdsAsync(User currentManager)
        {
            return _context.GetSubordinateUserIntIdsAsync(currentManager);
        }

        public Task<List<string>> GetSubordinateUserIdsAsync(User currentManager)
        {
            return _context.GetSubordinateUserIdsAsync(currentManager);
        }

        public Task<List<User>> GetSubordinateUsersAsync(User currentManager)
        {
            return _context.GetSubordinateUsersAsync(currentManager);
        }

        public async Task<bool> IsSubordinateAsync(User currentManager, int targetUserId)
        {
            var subordinateIds = await _context.GetSubordinateUserIntIdsAsync(currentManager);
            return subordinateIds.Contains(targetUserId);
        }
    }

    public static class HierarchyFilterExtensions
    {
        // Senior and peer roles strictly excluded from subordinate queues for managers
        public static readonly string[] ExcludedSeniorRoles = new[] { 
            "Super Admin", 
            "Admin", 
            "HR", 
            "Finance Manager", 
            "IT Manager", 
            "Auditor", 
            "Sales Manager", 
            "Manager",
            "General Manager"
        };

        /// <summary>
        /// Resolves subordinate user IDs as string representations for the current user, enforcing strict hierarchical boundaries.
        /// </summary>
        public static async Task<List<string>> GetSubordinateUserIdsAsync(
            this ApplicationDbContext context, 
            User currentManager)
        {
            var intIds = await context.GetSubordinateUserIntIdsAsync(currentManager);
            return intIds.Select(id => id.ToString()).ToList();
        }

        /// <summary>
        /// Resolves subordinate user IDs as integers for the current manager, strictly isolating peers and seniors.
        /// </summary>
        public static async Task<List<int>> GetSubordinateUserIntIdsAsync(
            this ApplicationDbContext context, 
            User currentManager)
        {
            if (currentManager == null)
            {
                return new List<int>();
            }

            var currentRole = currentManager.Role?.RoleName ?? "";

            // 1. If Super Admin, Admin, or HR, return full organizational visibility
            if (currentRole.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) || 
                currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                currentRole.Equals("HR", StringComparison.OrdinalIgnoreCase))
            {
                return await context.Users
                    .Where(u => u.IsActive && u.UserId != currentManager.UserId)
                    .Select(u => u.UserId)
                    .ToListAsync();
            }

            // 2. Identify all senior/peer users to strictly exclude
            var seniorUserIds = await context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && ExcludedSeniorRoles.Contains(u.Role.RoleName))
                .Select(u => u.UserId)
                .ToListAsync();

            var seniorSet = new HashSet<int>(seniorUserIds)
            {
                currentManager.UserId // Always exclude the manager themselves
            };

            // 3. Resolve subordinate user IDs:
            // Must report directly to current manager OR belong to the same department
            // AND user is NOT in seniorUserIds
            string managerIdStr = currentManager.UserId.ToString();
            string managerName = currentManager.FullName ?? "";
            bool isInventoryManager = currentRole.Equals("Inventory Manager", StringComparison.OrdinalIgnoreCase);

            var query = context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && !seniorSet.Contains(u.UserId));

            var subordinates = await query
                .Where(u => 
                    (u.ReportingManagerId == managerIdStr ||
                     (!string.IsNullOrEmpty(managerName) && u.ReportingManagerName == managerName) ||
                     (currentManager.DepartmentId != null && u.DepartmentId == currentManager.DepartmentId) ||
                     (isInventoryManager && u.Role != null && 
                        (u.Role.RoleName.Contains("Warehouse") || 
                         u.Role.RoleName.Contains("Inventory") || 
                         u.Role.RoleName.Contains("Logistics") || 
                         u.Role.RoleName.Contains("Storekeeper") || 
                         u.Role.RoleName.Contains("Clerk") || 
                         u.Role.RoleName.Contains("Staff") || 
                         u.Role.RoleName.Contains("Associate") || 
                         u.Role.RoleName.Contains("Driver")))
                    ))
                .Select(u => u.UserId)
                .ToListAsync();

            return subordinates;
        }

        /// <summary>
        /// Resolves subordinate User objects for the current manager, strictly filtered by hierarchy.
        /// </summary>
        public static async Task<List<User>> GetSubordinateUsersAsync(
            this ApplicationDbContext context,
            User currentManager)
        {
            var subIds = await context.GetSubordinateUserIntIdsAsync(currentManager);
            return await context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Shift)
                .Where(u => subIds.Contains(u.UserId))
                .OrderBy(u => u.FullName ?? u.UserName)
                .ToListAsync();
        }

        /// <summary>
        /// Validates if targetUserId is a valid subordinate of currentManager.
        /// </summary>
        public static async Task<bool> IsSubordinateAsync(
            this ApplicationDbContext context,
            User currentManager,
            int targetUserId)
        {
            var subIds = await context.GetSubordinateUserIntIdsAsync(currentManager);
            return subIds.Contains(targetUserId);
        }
    }
}
