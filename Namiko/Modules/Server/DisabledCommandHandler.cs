using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public static class DisabledCommandHandler
    {
        public static Dictionary<ulong, HashSet<string>> DisabledCommands;
        public static Dictionary<ulong, HashSet<string>> DisabledModules;
        public static HashSet<ulong> DisabledImages;

        static DisabledCommandHandler()
        {
            var raw = DisabledCommandDb.GetAll().Result;

            DisabledCommands = raw.Where(x => x.Type == DisabledCommandType.Command)
                .GroupBy(x => x.GuildId)
                .ToDictionary(x => x.Key, x => x.Select(x => x.Name).ToHashSet());

            DisabledModules = raw.Where(x => x.Type == DisabledCommandType.Module)
                .GroupBy(x => x.GuildId)
                .ToDictionary(x => x.Key, x => x.Select(x => x.Name).ToHashSet());

            DisabledImages = raw.Where(x => x.Type == DisabledCommandType.Images)
                .Select(x => x.GuildId)
                .ToHashSet();
        }

        public static async Task<bool> AddNew(string name, ulong guildId, DisabledCommandType type)
        {
            HashSet<string> set;
            var dict = type switch
            {
                DisabledCommandType.Command => DisabledCommands,
                DisabledCommandType.Module => DisabledModules,
                DisabledCommandType.Images => null,
                _ => throw new InvalidEnumArgumentException("Unknown type")
            };

            if (type == DisabledCommandType.Images)
            {
                if (DisabledImages.Contains(guildId))
                {
                    return false;
                } 
                DisabledImages.Add(guildId);
            }
            else if (dict.TryGetValue(guildId, out set))
            {
                if (set.Contains(name))
                    return false;
                set.Add(name);
            }
            else
            {
                set = new HashSet<string>();
                set.Add(name);
                dict.Add(guildId, set);
            }

            await DisabledCommandDb.AddNewDisabledCommand(guildId, name, type);
            return true;
        }
        public static async Task Remove(string name, ulong guildId, DisabledCommandType type)
        {
            HashSet<string> set;
            var dict = type switch
            {
                DisabledCommandType.Command => DisabledCommands,
                DisabledCommandType.Module => DisabledModules,
                DisabledCommandType.Images => null,
                _ => throw new InvalidEnumArgumentException("Unknown type")
            };

            if (type == DisabledCommandType.Images)
            {
                DisabledImages.Remove(guildId);
            }
            else if (dict.TryGetValue(guildId, out set))
            {
                set.Remove(name);
            }

            await DisabledCommandDb.DeleteDisabledCommand(guildId, name, type);
        }
        public static bool IsDisabled(string name, ulong guildId, DisabledCommandType type)
        {
            HashSet<string> set;
            var dict = type switch
            {
                DisabledCommandType.Command => DisabledCommands,
                DisabledCommandType.Module => DisabledModules,
                DisabledCommandType.Images => null,
                _ => throw new InvalidEnumArgumentException("Unknown type")
            };

            if (dict == null)
            {
                return DisabledImages.Contains(guildId);
            }
            else if (dict.TryGetValue(guildId, out set))
            {
                return set.Contains(name);
            }
            return false;
        }
    }
}
