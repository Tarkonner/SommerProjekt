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
        public async Task ContinuousHeartbeat()
        {
            int listenTimeMul = 5;
            int beats = 0;
            int maxExpectedBeats = listenTimeMul + 1;

            Heartbeat heartbeat = new Heartbeat();

            heartbeat.OnHeartbeat += () => { beats++; };

            heartbeat.Start();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            Assert.True(beats <= maxExpectedBeats,
                $"Expected {listenTimeMul} or {listenTimeMul + 1} beats, but got {beats}");

            await heartbeat.StopAsync();
        }


        [Fact]
        public async Task StopHeartbeat()
        {
            int listenTimeMul = 2;
            int beats = 0;
            int waitTimeMul = 4;

            int maxExpectedBeats = listenTimeMul + 1;

            Heartbeat heartbeat = new Heartbeat();

            heartbeat.OnHeartbeat += () => { beats++; };

            heartbeat.Start();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            await heartbeat.StopAsync();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * waitTimeMul + 200);

            Assert.True(heartbeat.heartbeatTask.IsCompleted);
            Assert.True(beats <= maxExpectedBeats,
                $"Expected {listenTimeMul} or {listenTimeMul + 1} beats, but got {beats}");
        }

        [Fact]
        public async Task RestartHeartbeat()
        {
            int listenTimeMul = 2;
            int beats = 0;
            int waitTimeMul = 4;
            int maxExpectedBeats = listenTimeMul * 2 + 2;

            Heartbeat heartbeat = new Heartbeat();

            heartbeat.OnHeartbeat += () => { beats++; };

            heartbeat.Start();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            await heartbeat.StopAsync();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * waitTimeMul + 200);

            heartbeat.Start();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            Assert.True(beats <= maxExpectedBeats,
                $"Expected {listenTimeMul} or {listenTimeMul + 1} beats, but got {beats}");
        }

        [Fact]
        public async Task ListenToHeartbeat()
        {
            bool haveBennCalled = false;

            Heartbeat heartbeat = new Heartbeat();

            heartbeat.OnHeartbeat += () => { haveBennCalled = true; };

            heartbeat.Start();

            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * 2);

            Assert.True(haveBennCalled);

            await heartbeat.StopAsync();
        }

        [Fact]
        public async Task DoesNotRaiseEvents_WhenNeverStarted()
        {
            int listenTimeMul = 2;
            int beats = 0;

            Heartbeat heartbeat = new Heartbeat();

            heartbeat.OnHeartbeat += () => { beats++; };


            await Task.Delay(heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            Assert.Equal(0, beats);
        }
    }
}
