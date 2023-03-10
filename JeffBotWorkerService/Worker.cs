using JeffBot;

namespace JeffBotWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        #region StreamerSettings
        // TODO: Get this from the "database", dynamo db maybe?
        public List<StreamerSettings> StreamerSettings => new List<StreamerSettings>
        {

        }; 
        #endregion

        #region Constructor
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        } 
        #endregion

        #region ExecuteAsync - Override
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var streamer in StreamerSettings)
            {
                var runMe = new JeffBot.JeffBot(streamer);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        } 
        #endregion
    }
}