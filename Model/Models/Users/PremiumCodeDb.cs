using Microsoft.EntityFrameworkCore;
using Model.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models.Users
{
    public static class PremiumCodeDb
    {
        public static async Task<IEnumerable<PremiumCode>> GenerateCodes(ProType type, int durationDays, int useAmount, int codeAmount, string prefix = null, string id = null, DateTime? expiresAt = null)
        {
            if (codeAmount > 1 && id != null)
            {
                throw new NamikoException("Only 1 code can be made with a custom id. Use prefix instead.");
            }
            if (codeAmount < 1)
            {
                throw new NamikoException("Amount must be >0");
            }
            if (durationDays < 1)
            {
                throw new NamikoException("DurationDays must be >0");
            }

            using var db = new NamikoDbContext();

            var codes = new List<PremiumCode>();

            for (int i = 0; i<codeAmount; i++)
            {
                var code = id ?? prefix ?? "";
                if (id == null) 
                {
                    code += RandomString(8);
                }

                var item = new PremiumCode
                {
                    CreatedAt = DateTime.Now,
                    ExpiresAt = expiresAt,
                    DurationDays = durationDays,
                    Type = type,
                    UsesLeft = useAmount,
                    Id = code
                };

                codes.Add(item);
            }

            db.PremiumCodes.AddRange(codes);
            var res = await db.SaveChangesAsync();

            return codes;
        }

        public static async Task<PremiumCode> TestCode(string id)
        {
            using var db = new NamikoDbContext();
            var res = await db.PremiumCodes.FirstOrDefaultAsync(x => x.Id == id && (x.ExpiresAt == null || x.ExpiresAt > DateTime.Now) && x.UsesLeft > 0);
            if (res == null)
                throw new NamikoException("Not found");

            return res;
        }

        public static async Task<Premium> RedeemCode(string id, ulong userId, ulong guildId)
        {
            var db = new NamikoDbContext();

            var res = await db.PremiumCodes
                .Include(x => x.Uses.Where(x => x.UserId == userId || x.GuildId == guildId))
                .FirstOrDefaultAsync(x => x.Id == id && (x.ExpiresAt == null || x.ExpiresAt > DateTime.Now) && x.UsesLeft > 0);

            if (res == null)
            {
                throw new NamikoException("Code invalid");
            }
            else if (res.Uses.Any())
            {
                throw new NamikoException("Code already redeemed on this account/server");
            }

            bool active = res.Type switch
            {
                ProType.Guild => PremiumDb.IsPremium(guildId, res.Type),
                ProType.GuildPlus => PremiumDb.IsPremium(guildId, res.Type),
                ProType.Pro => PremiumDb.IsPremium(userId, res.Type),
                ProType.ProPlus => PremiumDb.IsPremium(userId, res.Type),
                _ => throw new ArgumentException("Unknown pro type")
            };
            if (active)
            {
                throw new NamikoException($"{res.Type.ToString()} already active");
            }

            var premium = new Premium
            {
                GuildId = guildId,
                UserId = userId,
                Type = res.Type,
                ClaimDate = System.DateTime.Now,
                ExpireSent = false,
                ExpiresAt = DateTime.Now.AddDays(res.DurationDays)
            };

            var use = new PremiumCodeUse
            {
                GuildId = guildId,
                UserId = userId,
                PremiumCode = res,
                PremiumCodeId = res.Id
            };

            res.UsesLeft--;

            db.PremiumCodeUses.Add(use);
            db.Premiums.Add(premium);
            db.Update(res);

            await db.SaveChangesAsync();
            return premium;
        }

        private static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }
    }
}
