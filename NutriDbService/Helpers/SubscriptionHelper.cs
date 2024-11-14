using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using System.Threading.Tasks;

namespace NutriDbService.Helpers
{

    public class SubscriptionHelper
    {
        public railwayContext _nutriDbContext { get; set; }
        private readonly IServiceScopeFactory _serviceProviderFactory;
        private readonly ILogger _logger;
        public SubscriptionHelper(railwayContext nutriDbContext, IServiceScopeFactory serviceProviderFactory)
        {
            _nutriDbContext = nutriDbContext;
            _serviceProviderFactory = serviceProviderFactory;
            _logger = _serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<TransmitterHelper>>();
        }

        public async Task<bool> CheckSub(long TgId)
        {
            return true;
        }
    }
}
