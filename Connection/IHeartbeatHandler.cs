using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommenCompunents
{
    public interface IHeartbeatHandler
    {
        int heartbeatIntervalInMiliseconds { get; set; }

        event Action OnHeartbeat;

        void Start(int timeBetweenBeats);

        Task HeartbeatLogic(CancellationToken cancellationToken);

        Task StopAsync();
    }
}
