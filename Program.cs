using System;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace BillValidatorKeep_Alive
{
    internal class Program
    {
        public enum DeviceState
        {
            Running,
            Maintenance
        }
        // Create A New Class 
        public class BillValidator
        {
            private readonly object _lock = new object();

            private bool _ackEnabled = true;
            private DeviceState _state = DeviceState.Running;
            private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(10);
            private readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);

            private CancellationTokenSource _cts;

            public void Start()
            {
                _cts = new CancellationTokenSource();
                Task.Run(() => KeepAliveLoop(_cts.Token));
            }

            public void Stop()
            {
                _cts.Cancel();
            }

            public void SetAck(bool enabled)
            {
                lock (_lock)
                {
                    _ackEnabled = enabled;
                    Console.WriteLine($"ACK set to: {(enabled ? "ON" : "OFF")}");
                }
            }

            private async Task KeepAliveLoop(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending keep-alive ping...");

                    var responseTask = SimulateAckAsync(token);
                    var completedTask = await Task.WhenAny(responseTask, Task.Delay(_timeout, token));

                    if (completedTask == responseTask && responseTask.Result)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ACK received.");

                        lock (_lock)
                        {
                            if (_state == DeviceState.Maintenance)
                            {
                                Console.WriteLine("Device recovered. Transitioning to RUNNING.");
                                _state = DeviceState.Running;
                            }
                        }
                    }
                    else
                    {
                        HandleTimeout();
                    }

                    await Task.Delay(_pingInterval, token);
                }
            }

            private async Task<bool> SimulateAckAsync(CancellationToken token)
            {
                bool ackEnabled;

                lock (_lock)
                {
                    ackEnabled = _ackEnabled;
                }

                if (!ackEnabled)
                    return false;

                // Simulated response delay
                await Task.Delay(500, token);
                return true;
            }

            private void HandleTimeout()
            {
                lock (_lock)
                {
                    if (_state == DeviceState.Maintenance)
                    {
                        // Idempotent: do nothing if already in maintenance
                        return;
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR: No ACK received within timeout.");
                    Console.WriteLine("Transitioning to MAINTENANCE mode.");

                    _state = DeviceState.Maintenance;
                }
            }
        }
        static void Main(string[] args)
        {
            var validator = new BillValidator();
            validator.Start();

            Console.WriteLine("Bill Validator CLI started.");
            Console.WriteLine("Commands:");
            Console.WriteLine("  device bill_validator ack on");
            Console.WriteLine("  device bill_validator ack off");
            Console.WriteLine("  exit");

            while (true)
            {
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input == "exit")
                {
                    validator.Stop();
                    break;
                }

                if (input == "device bill_validator ack on")
                {
                    validator.SetAck(true);
                }
                else if (input == "device bill_validator ack off")
                {
                    validator.SetAck(false);
                }
                else
                {
                    Console.WriteLine("Unknown command.");
                }
            }
        }
    }
}



