using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDeletable>())
            {

                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
     
    }
}
