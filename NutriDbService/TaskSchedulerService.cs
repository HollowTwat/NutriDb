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
        private List<UserPing> _userPings;
        private readonly IServiceProvider _serviceProvider;
        public TaskSchedulerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
                List<int> validUsers = new List<int>() {3,13,17 };
                var users = _context.Userinfos.Include(x => x.User).Where(x => x.MorningPing != null).OrderByDescending(x => x.MorningPing)
                    .Select(x => new UserPing { Id = x.UserId, UserNoId = x.User.UserNoId, Ping = (TimeOnly)x.MorningPing, Slide = x.Timeslide }).ToList();
                users = users.Where(x => validUsers.Contains(x.Id)).ToList();
                ScheduleTasks(users);
            }
            return Task.CompletedTask;
        }

        private void ScheduleTasks(List<UserPing> usersPings)
        {
            lock (_lock)
            {
                foreach (var userPing in usersPings)
                {
                    userPing.Ping = new TimeOnly(16, 15);
                    if (userPing.Slide != null)
                        userPing.Ping.AddHours((double)userPing.Slide);
                    ScheduleTask(userPing);
                }
            }
        }

        private void ScheduleTask(UserPing userPing)
        {
            var dailyTime = userPing.Ping;
            var currentTime = DateTime.UtcNow.AddHours(3);
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

        public void TimerRestart()
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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
                    var usersPings = _context.Userinfos.Include(x => x.User).Where(x => x.MorningPing != null).OrderByDescending(x => x.MorningPing)
             .Select(x => new UserPing { UserNoId = x.User.UserNoId, Ping = (TimeOnly)x.MorningPing, Slide = x.Timeslide }).ToList();

                    _userPings = usersPings;
                    ScheduleTasks(_userPings);
                }
            }
        }

        public void UserTimerRestart(int userId)
        {
            lock (_lock)
            {
                // Остановить текущие таймеры
                foreach (var timer in _timers.Where(x => x.Id == userId))
                {
                    timer.Timer.Dispose();
                    _timers.Remove(timer);
                }
                // Настроить новые таймеры
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<railwayContext>();
                    var usersPings = _context.Userinfos.Include(x => x.User).Where(x => x.MorningPing != null && x.UserId == userId).OrderByDescending(x => x.MorningPing)
             .Select(x => new UserPing { UserNoId = x.User.UserNoId, Ping = (TimeOnly)x.MorningPing, Slide = x.Timeslide }).ToList();

                    _userPings = usersPings;
                    ScheduleTasks(_userPings);
                }
            }
        }

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
