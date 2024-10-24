using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NutriDbService
{
    public class TaskSchedulerService : IHostedService
    {
        private readonly object _lock = new object();
        private List<Timer> _timers = new List<Timer>();
        private List<DateTime> _executionTimes;

        public TaskSchedulerService(/*Dependency injection*/)
        {
           // _executionTimes = /*Pull from configuration*/;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ScheduleTasks(_executionTimes);
            return Task.CompletedTask;
        }

        private void ScheduleTasks(List<DateTime> executionTimes)
        {
            lock (_lock)
            {
                foreach (var executionTime in executionTimes)
                {
                    ScheduleTask(executionTime);
                }
            }
        }

        private void ScheduleTask(DateTime executionTime)
        {
            var dailyTime = executionTime.TimeOfDay;
            var currentTime = DateTime.Now;
            var nextOccurrence = CalculateNextOccurrence(currentTime, dailyTime);
            var timeToNextOccurrence = nextOccurrence - currentTime;

            Timer timer = new Timer(x =>
            {
                DoSmth();

                // Планируем на следующий день
                ScheduleTask(executionTime);
            }, null, timeToNextOccurrence, Timeout.InfiniteTimeSpan);

            _timers.Add(timer);
        }

        private DateTime CalculateNextOccurrence(DateTime currentTime, TimeSpan dailyTime)
        {
            var todayExecution = currentTime.Date.Add(dailyTime);
            if (currentTime < todayExecution)
            {
                return todayExecution;
            }
            else
            {
                return todayExecution.AddDays(1); // Переходим на следующий день
            }
        }

        public void TimerUpdate(List<DateTime> newExecutionTimes)
        {
            lock (_lock)
            {
                // Остановить текущие таймеры
                foreach (var timer in _timers)
                {
                    timer.Dispose();
                }
                _timers.Clear();

                // Настроить новые таймеры
                _executionTimes = newExecutionTimes;
                ScheduleTasks(_executionTimes);
            }
        }

        private void DoSmth()
        {
            Console.WriteLine("Task executed at: " + DateTime.Now);
            // Ваша бизнес-логика здесь
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                foreach (var timer in _timers)
                {
                    timer.Dispose();
                }
                _timers.Clear();
            }
            return Task.CompletedTask;
        }
    }
}
