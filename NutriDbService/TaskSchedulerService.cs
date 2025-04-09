using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace NutriDbService
{
    public class UserPing
    {
        public int UserId { get; set; }

        public long UserTgId { get; set; }

        public TimeOnly MorningPing { get; set; }

        public TimeOnly EveningPing { get; set; }

        public decimal? Slide { get; set; }
    }
    public class UserTimer
    {
        public int Id { get; set; }
        public Timer Timer { get; set; }
    }
    public class TaskSchedulerService : IHostedService
    {
        private readonly object _lock = new object();
        private List<UserTimer> _timers = new List<UserTimer>();
        //private List<UserPing> _userPings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _dbThrottle = new SemaphoreSlim(10, 10);
        public TaskSchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILogger<TaskSchedulerService>>();
        }
        private async Task<List<UserPing>> GetUserPingsAsync()
        {
            await _dbThrottle.WaitAsync();
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
                    var us = _context.Userinfos.Include(x => x.User).Where(x => x.User.NotifyStatus == true && x.User.IsActive && x.MorningPing != null && x.EveningPing != null).ToList();
                    var users = us.Select(x => new UserPing { UserId = x.UserId, UserTgId = (long)x.User.TgId, MorningPing = (TimeOnly)x.MorningPing, EveningPing = (TimeOnly)x.EveningPing, Slide = x.Timeslide }).ToList();
                    return users;
                }
            }
            finally
            {
                _dbThrottle.Release();
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var users = await GetUserPingsAsync();
            ScheduleTasks(users);
            _logger.LogWarning($"Timers:{Newtonsoft.Json.JsonConvert.SerializeObject(_timers)}");
        }

        private void ScheduleTasks(List<UserPing> usersPings)
        {
            lock (_lock)
            {
                //foreach (var timer in _timers)
                //{
                //    timer.Timer?.Dispose();
                //}
                //_timers.Clear();

                foreach (var userPing in usersPings)
                {

                    if (userPing.Slide != null)
                    {
                        userPing.EveningPing = userPing.EveningPing.AddHours(-(double)userPing.Slide);
                        userPing.MorningPing = userPing.MorningPing.AddHours(-(double)userPing.Slide);
                    }
                    ScheduleTask(userPing);
                }
            }
        }

        private void ScheduleTask(UserPing userPing)
        {
            var morningTime = userPing.MorningPing;
            var eveningTime = userPing.EveningPing;
            var currentTime = DateTime.UtcNow.ToLocalTime().AddHours(3);
            var nextMorningOccurrence = CalculateNextOccurrence(currentTime, morningTime);
            var timeToNextMorningOccurrence = nextMorningOccurrence - currentTime;
            var nextEveningOccurrence = CalculateNextOccurrence(currentTime, eveningTime);
            var timeToNextEveningOccurrence = nextEveningOccurrence - currentTime;
            Timer morningTimer = null;
            morningTimer = new Timer(async x =>
           {
               try
               {
                   await _dbThrottle.WaitAsync();
                   using (var scope = _serviceProvider.CreateScope())
                   {
                       var localNotHelper = scope.ServiceProvider.GetRequiredService<NotificationHelper>();

                       await localNotHelper.SendNotificationH(userPing, true);


                       //ScheduleTask(userPing);
                   }
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error in morning timer callback for user {UserId}", userPing.UserId);
               }
               finally
               {
                   _dbThrottle.Release();
                   //morningTimer?.Change(TimeSpan.FromHours(24), Timeout.InfiniteTimeSpan);
               }
           }, null, timeToNextMorningOccurrence, TimeSpan.FromHours(24));// Timeout.InfiniteTimeSpan);
            Timer eveningTimer = null;
            eveningTimer = new Timer(async x =>
           {
               try
               {
                   await _dbThrottle.WaitAsync();
                   using (var scope = _serviceProvider.CreateScope())
                   {
                       var localNotHelper = scope.ServiceProvider.GetRequiredService<NotificationHelper>();

                       // Используйте ассинхронную метод SendNotification
                       await localNotHelper.SendNotificationH(userPing, false);

                       //ScheduleTask(userPing);
                   }
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error in evening timer callback for user {UserId}", userPing.UserId);
               }
               finally
               {
                   _dbThrottle.Release();
                   //eveningTimer?.Change(TimeSpan.FromHours(24), Timeout.InfiniteTimeSpan);
               }
           }, null, timeToNextEveningOccurrence, TimeSpan.FromHours(24));

            _timers.Add(new UserTimer { Id = userPing.UserId, Timer = morningTimer });
            //else
            _timers.Add(new UserTimer { Id = userPing.UserId, Timer = eveningTimer });
        }

        private DateTime CalculateNextOccurrence(DateTime currentTime, TimeOnly dailyTime)
        {
            var todayExecution = currentTime.Date.Add(dailyTime.ToTimeSpan());
            if (currentTime < todayExecution)
            {
                return todayExecution;
            }
            else
            {
                return todayExecution.AddDays(1); // Переходим на следующий день
            }
        }

        public async Task TimerRestart()
        {
            try
            {
                //await _semaphore.WaitAsync();

                List<UserTimer> timersToDispose;
                lock (_lock)
                {
                    timersToDispose = _timers.ToList();
                    _timers.Clear();
                }
                foreach (var timer in timersToDispose)
                {
                    try
                    {
                        await timer.Timer.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing timer for user {UserId}", timer.Id);
                        throw;
                    }
                }
                List<UserPing> users = await GetUserPingsAsync().ConfigureAwait(false);
                lock (_lock)
                {
                    foreach (var userPing in users)
                    {
                        try
                        {
                            ScheduleTask(userPing);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to schedule task for user {UserId}", userPing.UserId);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"TimerRestartError ", ex);
                ErrorHelper.SendErrorMess($"TimerRestartError ", ex).GetAwaiter().GetResult();
            }
            finally
            {
                _logger.LogWarning($"Notofication Restarted");
            }

        }

        //public void UserTimerRestart(int userId)
        //{
        //    try
        //    {
        //        lock (_lock)
        //        {
        //            List<UserTimer> timersToRemove = new List<UserTimer>();
        //            // Остановить текущие таймеры
        //            foreach (var timer in _timers.Where(x => x.Id == userId))
        //            {
        //                timersToRemove.Add(timer);

        //                timer.Timer.Dispose();

        //            }
        //            foreach (var timer in timersToRemove)
        //                _timers.Remove(timer);
        //            // Настроить новые таймеры
        //            using (var scope = _serviceProvider.CreateScope())
        //            {
        //                var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
        //                var usersPings = _context.Userinfos.Include(x => x.User).Where(x => x.MorningPing != null && x.UserId == userId).OrderByDescending(x => x.MorningPing)
        //         .Select(x => new UserPing { UserNoId = x.User.UserNoId, Ping = (TimeOnly)x.MorningPing, Slide = x.Timeslide }).ToList();

        //                _userPings = usersPings;
        //                ScheduleTasks(_userPings);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        _logger.LogError($"UserTimerRestartError for User:{userId}", ex);
        //        ErrorHelper.SendErrorMess($"UserTimerRestartError for User:{userId}", ex).GetAwaiter().GetResult();
        //    }
        //}

        public Task StopAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                foreach (var timer in _timers)
                {
                    timer.Timer.Dispose();
                }
                _timers.Clear();
            }
            return Task.CompletedTask;
        }

        public async Task<List<string>> GetTimers()
        {
            var res = new List<string>();

            var pings = await GetUserPingsAsync();
            var timercount = _timers.Count;
            foreach (var ping in pings)
            {
                if (ping.Slide != null)
                {
                    ping.EveningPing = ping.EveningPing.AddHours(-(double)ping.Slide);
                    ping.MorningPing = ping.MorningPing.AddHours(-(double)ping.Slide);
                }
                res.Add($"{ping.UserTgId}:morning={ping.MorningPing}, evening={ping.EveningPing}\n");
            }
            return res;

        }
    }
}
