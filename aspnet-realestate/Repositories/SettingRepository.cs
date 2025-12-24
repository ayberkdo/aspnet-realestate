using aspnet_realestate.Models;

namespace aspnet_realestate.Repositories
{
    public class SettingRepository : GenericRepository<Setting>
    {
        public SettingRepository(AppDbContext context) : base(context, context.Set<Setting>())
        {
        }
    }
}
