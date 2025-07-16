using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommenCompunents
{
    public interface IHeartbeatHandler
    {
        int heartbeatIntervalInMiliseconds { get; }

        event Action OnHeartbeat;

        void Start();

        Task HeartbeatLogic();

        void Stop();
    }
}
