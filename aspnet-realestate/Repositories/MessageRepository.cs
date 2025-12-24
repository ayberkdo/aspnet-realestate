using aspnet_realestate.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace aspnet_realestate.Repositories
{
    public class MessageRepository : GenericRepository<Messages>
    {
        public MessageRepository(AppDbContext context) : base(context, context.Set<Messages>()){}

    }
}
