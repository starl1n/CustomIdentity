using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace test.Models
{
    public class UserStore : IUserStore<Account>,
        IUserPasswordStore<Account>,
        IUserEmailStore<Account>,
        IUserRoleStore<Account>
    {
        private readonly test db;

        public UserStore(testContext db)
        {
            this.db = db;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
        }

        public Task<string> GetUserIdAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task SetUserNameAsync(Account user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(SetUserNameAsync));
        }

        public Task<string> GetNormalizedUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(GetNormalizedUserNameAsync));
        }

        public Task SetNormalizedUserNameAsync(Account user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }

        public async Task<IdentityResult> CreateAsync(Account user, CancellationToken cancellationToken)
        {
            db.Add(user);

            var result = await db.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> UpdateAsync(Account user, CancellationToken cancellationToken)
        {
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await db.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(Account user, CancellationToken cancellationToken)
        {
            db.Remove(user);

            int i = await db.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(i == 1 ? IdentityResult.Success : IdentityResult.Failed());
        }

        public async Task<Account> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {

            return await db.Account.FindAsync(userId);

        }

        public async Task<Account> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await db.Account
                           .AsAsyncEnumerable()
                           .SingleOrDefault(p => p.Email.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }

        public Task SetPasswordHashAsync(Account user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;

            return Task.FromResult((object)null);
        }

        public Task<string> GetPasswordHashAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.Password));
        }

        public async Task SetEmailAsync(Account user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await db.SaveChangesAsync(cancellationToken);
        }

        public Task<string> GetEmailAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(Account user, CancellationToken cancellationToken)
        {
            //We are by passing this because we are not confirming emails now
            return Task.FromResult(user.EmailConfirmed.HasValue && user.EmailConfirmed.Value);
        }

        public Task SetEmailConfirmedAsync(Account user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            return Task.FromResult(db.SaveChanges());
        }
        public Task<Account> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.Account
                           .AsEnumerable()
                           .SingleOrDefault(p => p.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<string> GetNormalizedEmailAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task SetNormalizedEmailAsync(Account user, string normalizedEmail, CancellationToken cancellationToken)
        {
            var theUser = db.Account.Where(x => x.Id == user.Id).FirstOrDefault();
            theUser.Email = normalizedEmail;
            if (!string.IsNullOrWhiteSpace(theUser.Id))
            {
                db.Entry(theUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;


            }
            return Task.FromResult(db.SaveChanges());
        }

        public Task AddToRoleAsync(Account user, string roleName, CancellationToken cancellationToken)
        {
            var role = db.Role.FirstOrDefault(x => x.Name == roleName);
            user.RoleId = role.Id;
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            return Task.FromResult(db.SaveChanges());
        }

        public async Task RemoveFromRoleAsync(Account user, string roleName, CancellationToken cancellationToken)
        {

            user.RoleId = string.Empty;
            db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IList<string>> GetRolesAsync(Account user, CancellationToken cancellationToken)
        {

            var data = db.Role.Where(x => x.Id == user.RoleId)
                .Select(x => x.Name);
            var parsed = data.ToList();
            var d = await Task.FromResult(parsed);
            return d;
        }

        public Task<bool> IsInRoleAsync(Account user, string roleName, CancellationToken cancellationToken)
        {
            var role = db.Role.FirstOrDefault(x => x.Name == roleName);
            return Task.FromResult(user.RoleId == role.Id);

        }

        public async Task<IList<Account>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await db.Role.FirstOrDefaultAsync(x => x.Name == roleName);
            var data = await db.Account.Where(x => x.RoleId == role.Id).ToListAsync();
            return data;
        }
    }
}
