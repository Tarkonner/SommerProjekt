using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommenCompunents
{
    public class Heartbeat : IHeartbeatHandler
    {
        public Task heartbeatTask { get; private set; }

        public int heartbeatIntervalInMiliseconds => 200;

        public event Action OnHeartbeat;

        private bool _running;

        public void Start()
        {
            _running = true;
            heartbeatTask = HeartbeatLogic();
        }

        public void Stop()
        {
            _running = false;
        }

        public async Task HeartbeatLogic()
        {
            while (_running) 
            {
                await Task.Delay(heartbeatIntervalInMiliseconds);
                OnHeartbeat?.Invoke();
            }
        }
    }
}
