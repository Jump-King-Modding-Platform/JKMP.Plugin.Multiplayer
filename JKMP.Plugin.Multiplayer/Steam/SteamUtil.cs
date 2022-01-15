using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Steam
{
    internal static class SteamUtil
    {
        private static readonly Dictionary<SteamId, TaskCompletionSource<Friend>> PendingUserInfoQueries = new();

        public static void Initialize()
        {
            SteamFriends.OnPersonaStateChange += OnPersonaStateChange;
        }

        private static void OnPersonaStateChange(Friend friend)
        {
            if (PendingUserInfoQueries.TryGetValue(friend.Id, out var tcs))
            {
                PendingUserInfoQueries.Remove(friend.Id);
                tcs.SetResult(friend);
            }
        }

        public static Task<Friend?> GetUserInfo(ulong steamId, int timeoutDelayMs = 10_000) => GetUserInfo(new SteamId { Value = steamId }, timeoutDelayMs);
        public static async Task<Friend?> GetUserInfo(SteamId steamId, int timeoutDelayMs = 10_000)
        {
            if (!SteamFriends.RequestUserInformation(steamId))
            {
                return new Friend(steamId);
            }

            var tcs = new TaskCompletionSource<Friend>();
            PendingUserInfoQueries.Add(steamId, tcs);

            var timeoutTask = Task.Delay(timeoutDelayMs);
            var resultTask = tcs.Task;

            await Task.WhenAny(timeoutTask, resultTask);
            PendingUserInfoQueries.Remove(steamId);

            if (resultTask.IsCompleted)
                return resultTask.Result;
            
            return null;
        }
    }
}