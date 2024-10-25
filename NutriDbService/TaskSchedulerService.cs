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
        public TimeOnly Ping { get; set; }

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
                List<int> validUsers = new List<int>() { 3, 13, 17 };
                var users = _context.Userinfos.Include(x => x.User).Where(x => x.MorningPing != null).OrderByDescending(x => x.MorningPing)
                    .Select(x => new UserPing { Id = x.UserId, UserNoId = x.User.UserNoId, Ping = (TimeOnly)x.MorningPing, Slide = x.Timeslide }).ToList();
                users = users.Where(x => validUsers.Contains(x.Id)).ToList();
                return users;
            }
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var users = GetUserPings();
            ScheduleTasks(users);
            return Task.CompletedTask;
        }

        private void ScheduleTasks(List<UserPing> usersPings)
        {
            lock (_lock)
            {
                foreach (var userPing in usersPings)
                {
                    //if (userPing.UserNoId != 403489853)
                    //    userPing.Ping = new TimeOnly(18, 5);
                    if (userPing.Slide != null)
                        userPing.Ping.AddHours((double)userPing.Slide);
                    ScheduleTask(userPing);
                }
            }
        }

        private void ScheduleTask(UserPing userPing)
        {
            var dailyTime = userPing.Ping;
            var currentTime = DateTime.UtcNow.ToLocalTime().AddHours(3);
            var nextOccurrence = CalculateNextOccurrence(currentTime, dailyTime);
            var timeToNextOccurrence = nextOccurrence - currentTime;

            Timer timer = new Timer(async x =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var localNotHelper = scope.ServiceProvider.GetRequiredService<NotificationHelper>();

                    // Используйте ассинхронную метод SendNotification
                    await localNotHelper.SendNotification(userPing.Id);
                    // Планируем на следующий день
                    ScheduleTask(userPing);
                }
            }, null, timeToNextOccurrence, Timeout.InfiniteTimeSpan);

            _timers.Add(new UserTimer { Id = userPing.Id, Timer = timer });
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
    }
}
