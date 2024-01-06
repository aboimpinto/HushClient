using Microsoft.Extensions.Logging;
using Olimpo;

namespace HushClient.Workflows;

public class HushClientWorkflow : IHushClientWorkflow
{
    private readonly IBootstrapperManager _bootstrapperManager;
        private readonly ILogger<HushClientWorkflow> _logger;

    public HushClientWorkflow(
        IBootstrapperManager bootstrapperManager,
        ILogger<HushClientWorkflow> logger)
    {
        this._bootstrapperManager = bootstrapperManager;
        this._logger = logger;
    }

    public void Start()
    {
        this._logger.LogInformation("Starting HushClientWorkflow...");
        
        this._bootstrapperManager.Start();
    }
}
