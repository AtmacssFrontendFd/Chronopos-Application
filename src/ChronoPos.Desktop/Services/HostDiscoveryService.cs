using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChronoPos.Desktop.Services
{
    public class HostBroadcastMessage
    {
        public string Type { get; set; } = "ChronoPOS_HOST_BROADCAST";
        public string HostName { get; set; } = string.Empty;
        public string HostIp { get; set; } = string.Empty;
        public string LicenseFingerprint { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public int PlanId { get; set; }
        public int MaxPosDevices { get; set; }
        public int CurrentClientCount { get; set; }
    }

    public interface IHostDiscoveryService
    {
        Task<List<HostBroadcastMessage>> DiscoverHostsAsync(int timeoutSeconds = 10, CancellationToken cancellationToken = default);
        Task StartBroadcastingAsync(HostBroadcastMessage hostInfo, CancellationToken cancellationToken = default);
    }

    public class HostDiscoveryService : IHostDiscoveryService
    {
        private const string MULTICAST_ADDRESS = "239.255.42.99";
        private const int MULTICAST_PORT = 42099;

        public async Task<List<HostBroadcastMessage>> DiscoverHostsAsync(int timeoutSeconds = 10, CancellationToken cancellationToken = default)
        {
            var hosts = new List<HostBroadcastMessage>();
            var seenHosts = new HashSet<string>();

            try
            {
                using var udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                
                var localEndPoint = new IPEndPoint(IPAddress.Any, MULTICAST_PORT);
                udpClient.Client.Bind(localEndPoint);

                var multicastAddress = IPAddress.Parse(MULTICAST_ADDRESS);
                udpClient.JoinMulticastGroup(multicastAddress);

                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                try
                {
                    while (!linkedCts.Token.IsCancellationRequested)
                    {
                        var receiveTask = udpClient.ReceiveAsync();
                        var delayTask = Task.Delay(100, linkedCts.Token);
                        
                        var completedTask = await Task.WhenAny(receiveTask, delayTask);
                        
                        if (completedTask == receiveTask)
                        {
                            var result = await receiveTask;
                            var message = Encoding.UTF8.GetString(result.Buffer);
                            
                            var hostMessage = JsonConvert.DeserializeObject<HostBroadcastMessage>(message);
                            if (hostMessage != null && hostMessage.Type == "ChronoPOS_HOST_BROADCAST")
                            {
                                var hostKey = $"{hostMessage.HostIp}:{hostMessage.HostName}";
                                if (!seenHosts.Contains(hostKey))
                                {
                                    seenHosts.Add(hostKey);
                                    hosts.Add(hostMessage);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected on timeout
                }
                finally
                {
                    udpClient.DropMulticastGroup(multicastAddress);
                    linkedCts.Dispose();
                    timeoutCts.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Host discovery error: {ex.Message}");
            }

            return hosts;
        }

        public async Task StartBroadcastingAsync(HostBroadcastMessage hostInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                using var udpClient = new UdpClient();
                var multicastEndPoint = new IPEndPoint(IPAddress.Parse(MULTICAST_ADDRESS), MULTICAST_PORT);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var json = JsonConvert.SerializeObject(hostInfo);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    
                    await udpClient.SendAsync(bytes, bytes.Length, multicastEndPoint);
                    
                    await Task.Delay(3000, cancellationToken); // Broadcast every 3 seconds
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Broadcasting error: {ex.Message}");
            }
        }
    }
}
