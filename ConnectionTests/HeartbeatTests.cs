using CommenCompunents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartbeatTests
{
    public class HeartbeatTests
    {
        [Fact]
        public async Task StartHeartbeat()
        {
            Heartbeat heartbeat = new Heartbeat();
            heartbeat.Start();


        }

        [Fact]
        public async Task StopHeartbeat()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task ListenToHeartbeat()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task HasOwnTask()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task CanAttachMultipleListeners()
        {
            // Should allow multiple subscribers to OnHeartbeat event
            Assert.True(false);
        }

        [Fact]
        public async Task DoesNotRaiseEvents_WhenNeverStarted()
        {
            // Should not raise any heartbeat events unless Start is called
            Assert.True(false);
        }

        [Fact]
        public async Task RaisesHeartbeatEvent_AtInterval()
        {
            // Should raise OnHeartbeat at expected time intervals
            Assert.True(false);
        }
    }
}
