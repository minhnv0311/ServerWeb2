using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace VSDCompany.Models
{
    partial class Entities : DbContext
    {
        public override async Task<int> SaveChangesAsync()
        {
            AddBaseInfomation();
            return await base.SaveChangesAsync();
        }

        public override int SaveChanges()
        {
            AddBaseInfomation();
            return base.SaveChanges();
        }
        public void AddBaseInfomation()
        {
            Entities db = new Entities();
            var entities = ChangeTracker.Entries().Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));

            var currentUsername = !string.IsNullOrEmpty(System.Web.HttpContext.Current?.User?.Identity?.Name)
                ? System.Web.HttpContext.Current.User.Identity.Name
                : "Guest";

            foreach (var entity in entities)
            {
                if (entity.State != EntityState.Deleted)
                {
                    if (entity.State == EntityState.Added)
                    {
                        if (entity.CurrentValues.PropertyNames.Contains("FCreateTime"))
                            entity.Property("FCreateTime").CurrentValue = DateTime.Now;
                        if (entity.CurrentValues.PropertyNames.Contains("FUserCreate"))
                            entity.Property("FUserCreate").CurrentValue = currentUsername;
                        if (entity.CurrentValues.PropertyNames.Contains("FInUse"))
                            entity.Property("FInUse").CurrentValue = true;
                       
                    }
                    if (entity.CurrentValues.PropertyNames.Contains("FUserUpdate"))
                        entity.Property("FUserUpdate").CurrentValue = currentUsername;
                    if (entity.CurrentValues.PropertyNames.Contains("FUpdateTime"))
                        entity.Property("FUpdateTime").CurrentValue = DateTime.Now;
                }

            }
        }
    }
}