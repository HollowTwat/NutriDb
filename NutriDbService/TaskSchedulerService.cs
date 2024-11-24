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

namespace NutriDbService
{
    public class UserPing
    {
        public int Id { get; set; }

        public long UserNoId { get; set; }

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
        public TaskSchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILogger<TaskSchedulerService>>();
        }
        private List<UserPing> GetUserPings()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
                //List<int> validUsers = new List<int>() { 17 };
                var users = _context.Userinfos.Include(x => x.User).Where(x => x.User.NotifyStatus == true && x.MorningPing != null && x.EveningPing != null)
                    .Select(x => new UserPing { Id = x.UserId, UserNoId = x.User.UserNoId, MorningPing = (TimeOnly)x.MorningPing, EveningPing = (TimeOnly)x.EveningPing, Slide = x.Timeslide }).ToList();
                //users = users.Where(x => validUsers.Contains(x.Id)).ToList();
                return users;
            }
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var users = GetUserPings();
            ScheduleTasks(users);
            _logger.LogWarning($"Timers:{Newtonsoft.Json.JsonConvert.SerializeObject(_timers)}");
            return Task.CompletedTask;
        }

        private void ScheduleTasks(List<UserPing> usersPings)
        {
            lock (_lock)
            {
                foreach (var userPing in usersPings)
                {

                    //userPing.MorningPing = new TimeOnly(3, 06);
                    //userPing.EveningPing = new TimeOnly(3, 04);

                    if (userPing.Slide != null)
                    {
                        userPing.EveningPing = userPing.EveningPing.AddHours((double)userPing.Slide);
                        userPing.MorningPing = userPing.MorningPing.AddHours((double)userPing.Slide);
                    }
                    ScheduleTask(userPing);
                }
            }
        }

        private void ScheduleTask(UserPing userPing)
        {
            //bool isMorningNext = false;
            var morningTime = userPing.MorningPing;
            var eveningTime = userPing.EveningPing;
            var currentTime = DateTime.UtcNow.ToLocalTime().AddHours(3);
            var nextMorningOccurrence = CalculateNextOccurrence(currentTime, morningTime);
            var timeToNextMorningOccurrence = nextMorningOccurrence - currentTime;
            var nextEveningOccurrence = CalculateNextOccurrence(currentTime, eveningTime);
            var timeToNextEveningOccurrence = nextEveningOccurrence - currentTime;
            //if (timeToNextMorningOccurrence < timeToNextEveningOccurrence)
            //    isMorningNext = true;

            Timer morningTimer = new Timer(async x =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var localNotHelper = scope.ServiceProvider.GetRequiredService<NotificationHelper>();

                    // Используйте ассинхронную метод SendNotification
                    await localNotHelper.SendNotification(userPing.Id, true);
                    // Планируем на следующий день

                    //ScheduleTask(userPing);
                }
            }, null, timeToNextMorningOccurrence, TimeSpan.FromHours(24));// Timeout.InfiniteTimeSpan);

            Timer eveningTimer = new Timer(async x =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var localNotHelper = scope.ServiceProvider.GetRequiredService<NotificationHelper>();

                    // Используйте ассинхронную метод SendNotification
                    await localNotHelper.SendNotification(userPing.Id, false);
                    // Планируем на следующий день

                    //ScheduleTask(userPing);
                }
            }, null, timeToNextEveningOccurrence, TimeSpan.FromHours(24));// Timeout.InfiniteTimeSpan);
                                                                          //if (isMorningNext)
            _timers.Add(new UserTimer { Id = userPing.Id, Timer = morningTimer });
            //else
            _timers.Add(new UserTimer { Id = userPing.Id, Timer = eveningTimer });
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
            await _semaphore.WaitAsync();
            try
            {
                lock (_lock)
                {
                    // Остановить текущие таймеры
                    foreach (var timer in _timers)
                    {
                        timer.Timer.Dispose();
                    }
                    _timers.Clear();

                    // Настроить новые таймеры
                    var users = GetUserPings();

                    ScheduleTasks(users);
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
                _semaphore.Release(); // Высвобождение доступа для других вызовов
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

        public List<string> GetTimers()
        {
            var res = new List<string>();
            lock (_lock)
            {
                foreach (var timer in _timers)
                    res.Add($"{timer.Id}:{Newtonsoft.Json.JsonConvert.SerializeObject(timer.Timer)}\n");
                return res;
            }
        }
    }
}
