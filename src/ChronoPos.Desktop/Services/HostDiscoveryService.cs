using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ChronoPos.Application.Logging;

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
            AppLogger.LogSeparator("HOST DISCOVERY START", "host_discovery");
            AppLogger.LogInfo($"[HOST DISCOVERY] Starting host discovery (timeout: {timeoutSeconds}s)", filename: "host_discovery");
            var hosts = new List<HostBroadcastMessage>();
            var seenHosts = new HashSet<string>();

            try
            {
                // Log network interfaces
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    .ToList();
                
                AppLogger.LogInfo($"[HOST DISCOVERY] Found {networkInterfaces.Count} active network interface(s):", filename: "host_discovery");
                foreach (var ni in networkInterfaces)
                {
                    var ipProps = ni.GetIPProperties();
                    var ipv4Addresses = ipProps.UnicastAddresses
                        .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(addr => addr.Address.ToString())
                        .ToList();
                    
                    if (ipv4Addresses.Any())
                    {
                        AppLogger.LogInfo($"[HOST DISCOVERY]   - {ni.Name} ({ni.NetworkInterfaceType}): {string.Join(", ", ipv4Addresses)}", filename: "host_discovery");
                        AppLogger.LogInfo($"[HOST DISCOVERY]     Supports Multicast: {ni.SupportsMulticast}", filename: "host_discovery");
                    }
                }
                
                using var udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                
                var localEndPoint = new IPEndPoint(IPAddress.Any, MULTICAST_PORT);
                udpClient.Client.Bind(localEndPoint);
                AppLogger.LogInfo($"[HOST DISCOVERY] ✅ Bound to local endpoint: {localEndPoint}", filename: "host_discovery");
                AppLogger.LogInfo($"[HOST DISCOVERY] Actual socket local endpoint: {udpClient.Client.LocalEndPoint}", filename: "host_discovery");

                var multicastAddress = IPAddress.Parse(MULTICAST_ADDRESS);
                
                try
                {
                    udpClient.JoinMulticastGroup(multicastAddress);
                    AppLogger.LogInfo($"[HOST DISCOVERY] ✅ Joined multicast group: {MULTICAST_ADDRESS}:{MULTICAST_PORT}", filename: "host_discovery");
                }
                catch (Exception joinEx)
                {
                    AppLogger.LogError($"[HOST DISCOVERY] ❌ Failed to join multicast group", joinEx, filename: "host_discovery");
                    throw;
                }
                
                // Log firewall warning
                AppLogger.LogWarning($"[HOST DISCOVERY] ⚠️ IMPORTANT: Ensure Windows Firewall allows UDP inbound on port {MULTICAST_PORT}", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY] ⚠️ If no hosts found, check: 1) Firewall rules, 2) Router settings, 3) Host is broadcasting", filename: "host_discovery");

                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                AppLogger.LogInfo("[HOST DISCOVERY] 👂 Listening for broadcasts...", filename: "host_discovery");
                
                int attemptCount = 0;
                int emptyReceives = 0;
                DateTime startTime = DateTime.Now;
                
                try
                {
                    while (!linkedCts.Token.IsCancellationRequested)
                    {
                        attemptCount++;
                        var receiveTask = udpClient.ReceiveAsync();
                        var delayTask = Task.Delay(100, linkedCts.Token);
                        
                        var completedTask = await Task.WhenAny(receiveTask, delayTask);
                        
                        if (completedTask == receiveTask)
                        {
                            var result = await receiveTask;
                            var message = Encoding.UTF8.GetString(result.Buffer);
                            
                            AppLogger.LogInfo($"[HOST DISCOVERY] 📨 Received message from {result.RemoteEndPoint} ({result.Buffer.Length} bytes)", filename: "host_discovery");
                            AppLogger.LogDebug($"[HOST DISCOVERY] Raw message: {message}", filename: "host_discovery");
                            
                            try
                            {
                                var hostMessage = JsonConvert.DeserializeObject<HostBroadcastMessage>(message);
                                if (hostMessage != null && hostMessage.Type == "ChronoPOS_HOST_BROADCAST")
                                {
                                    var hostKey = $"{hostMessage.HostIp}:{hostMessage.HostName}";
                                    if (!seenHosts.Contains(hostKey))
                                    {
                                        seenHosts.Add(hostKey);
                                        hosts.Add(hostMessage);
                                        AppLogger.LogInfo($"[HOST DISCOVERY] ✅✅✅ Found NEW host: {hostMessage.HostName} ({hostMessage.HostIp})", filename: "host_discovery");
                                        AppLogger.LogInfo($"[HOST DISCOVERY]   - Plan: {hostMessage.PlanId}, Max Devices: {hostMessage.MaxPosDevices}, Clients: {hostMessage.CurrentClientCount}", filename: "host_discovery");
                                    }
                                    else
                                    {
                                        AppLogger.LogDebug($"[HOST DISCOVERY] Duplicate host message from {hostMessage.HostName}", filename: "host_discovery");
                                    }
                                }
                                else
                                {
                                    AppLogger.LogWarning($"[HOST DISCOVERY] ⚠️ Invalid message type or null message", filename: "host_discovery");
                                    if (hostMessage != null)
                                    {
                                        AppLogger.LogWarning($"[HOST DISCOVERY] Message type was: {hostMessage.Type}", filename: "host_discovery");
                                    }
                                }
                            }
                            catch (Exception parseEx)
                            {
                                AppLogger.LogError($"[HOST DISCOVERY] ❌ Error parsing message", parseEx, filename: "host_discovery");
                                AppLogger.LogError($"[HOST DISCOVERY] Failed message content: {message}", filename: "host_discovery");
                            }
                        }
                        else
                        {
                            emptyReceives++;
                        }
                        
                        // Log progress every 2 seconds
                        if (attemptCount % 20 == 0)
                        {
                            var elapsed = DateTime.Now - startTime;
                            AppLogger.LogInfo($"[HOST DISCOVERY] Still listening... (attempts: {attemptCount}, empty: {emptyReceives}, hosts found: {hosts.Count}, elapsed: {elapsed.TotalSeconds:F1}s)", filename: "host_discovery");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var elapsed = DateTime.Now - startTime;
                    AppLogger.LogInfo($"[HOST DISCOVERY] ⏱️ Discovery timed out after {elapsed.TotalSeconds:F1}s", filename: "host_discovery");
                }
                finally
                {
                    try
                    {
                        udpClient.DropMulticastGroup(multicastAddress);
                        AppLogger.LogInfo($"[HOST DISCOVERY] Left multicast group", filename: "host_discovery");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogWarning($"[HOST DISCOVERY] Error leaving multicast group: {ex.Message}", filename: "host_discovery");
                    }
                    
                    linkedCts.Dispose();
                    timeoutCts.Dispose();
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[HOST DISCOVERY] ❌ Discovery error", ex, filename: "host_discovery");
                AppLogger.LogError($"[HOST DISCOVERY] Exception type: {ex.GetType().Name}", filename: "host_discovery");
                AppLogger.LogError($"[HOST DISCOVERY] Inner exception: {ex.InnerException?.Message}", filename: "host_discovery");
                Console.WriteLine($"Host discovery error: {ex.Message}");
            }

            if (hosts.Count > 0)
            {
                AppLogger.LogInfo($"[HOST DISCOVERY] ✅✅✅ Discovery complete: Found {hosts.Count} host(s)!", filename: "host_discovery");
                foreach (var host in hosts)
                {
                    AppLogger.LogInfo($"[HOST DISCOVERY]   - {host.HostName} ({host.HostIp})", filename: "host_discovery");
                }
            }
            else
            {
                AppLogger.LogWarning($"[HOST DISCOVERY] ⚠️⚠️⚠️ Discovery complete: NO hosts found", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY] Troubleshooting steps:", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY]   1. Check Windows Firewall (both host and client)", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY]   2. Verify both devices are on same network/subnet", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY]   3. Check router AP isolation settings", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY]   4. Verify host is actually broadcasting (check host logs)", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST DISCOVERY]   5. Try temporarily disabling firewall for testing", filename: "host_discovery");
            }
            
            AppLogger.LogSeparator("", "host_discovery");
            return hosts;
        }

        public async Task StartBroadcastingAsync(HostBroadcastMessage hostInfo, CancellationToken cancellationToken = default)
        {
            AppLogger.LogSeparator("HOST BROADCAST START", "host_discovery");
            AppLogger.LogInfo($"[HOST BROADCAST] Starting host broadcast: {hostInfo.HostName} ({hostInfo.HostIp})", filename: "host_discovery");
            AppLogger.LogInfo($"[HOST BROADCAST] Multicast: {MULTICAST_ADDRESS}:{MULTICAST_PORT}, Interval: 3s", filename: "host_discovery");
            
            try
            {
                using var udpClient = new UdpClient();
                
                // Log socket configuration
                AppLogger.LogInfo($"[HOST BROADCAST] Creating UDP client for multicast broadcasting", filename: "host_discovery");
                AppLogger.LogInfo($"[HOST BROADCAST] Socket created - Local endpoint: {udpClient.Client.LocalEndPoint}", filename: "host_discovery");
                
                var multicastEndPoint = new IPEndPoint(IPAddress.Parse(MULTICAST_ADDRESS), MULTICAST_PORT);
                AppLogger.LogInfo($"[HOST BROADCAST] Target multicast endpoint: {multicastEndPoint}", filename: "host_discovery");
                
                // Set socket options for better multicast support
                try
                {
                    udpClient.EnableBroadcast = true;
                    AppLogger.LogInfo($"[HOST BROADCAST] Enabled UDP broadcast", filename: "host_discovery");
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"[HOST BROADCAST] Could not enable broadcast: {ex.Message}", filename: "host_discovery");
                }
                
                // Set multicast TTL
                try
                {
                    udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
                    AppLogger.LogInfo($"[HOST BROADCAST] Set multicast TTL to 255", filename: "host_discovery");
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"[HOST BROADCAST] Could not set multicast TTL: {ex.Message}", filename: "host_discovery");
                }
                
                // Get network interfaces
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    .ToList();
                
                AppLogger.LogInfo($"[HOST BROADCAST] Found {networkInterfaces.Count} active network interface(s):", filename: "host_discovery");
                foreach (var ni in networkInterfaces)
                {
                    var ipProps = ni.GetIPProperties();
                    var ipv4Addresses = ipProps.UnicastAddresses
                        .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(addr => addr.Address.ToString())
                        .ToList();
                    
                    if (ipv4Addresses.Any())
                    {
                        AppLogger.LogInfo($"[HOST BROADCAST]   - {ni.Name} ({ni.NetworkInterfaceType}): {string.Join(", ", ipv4Addresses)}", filename: "host_discovery");
                    }
                }
                
                var json = JsonConvert.SerializeObject(hostInfo);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                AppLogger.LogInfo($"[HOST BROADCAST] Message payload ({bytes.Length} bytes):", filename: "host_discovery");
                AppLogger.LogInfo($"[HOST BROADCAST] {json}", filename: "host_discovery");
                
                // Log Windows Firewall warning
                AppLogger.LogWarning($"[HOST BROADCAST] ⚠️ IMPORTANT: Ensure Windows Firewall allows UDP outbound on port {MULTICAST_PORT}", filename: "host_discovery");
                AppLogger.LogWarning($"[HOST BROADCAST] ⚠️ If clients can't discover this host, add firewall rule for ChronoPos.Desktop.exe", filename: "host_discovery");

                int broadcastCount = 0;
                DateTime lastLogTime = DateTime.Now;
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await udpClient.SendAsync(bytes, bytes.Length, multicastEndPoint);
                        broadcastCount++;
                        
                        // Log first broadcast immediately, then every 20 broadcasts (every minute)
                        if (broadcastCount == 1 || broadcastCount % 20 == 0)
                        {
                            var elapsed = DateTime.Now - lastLogTime;
                            AppLogger.LogInfo($"[HOST BROADCAST] ✅ Sent broadcast #{broadcastCount} (elapsed: {elapsed.TotalSeconds:F1}s)", filename: "host_discovery");
                            lastLogTime = DateTime.Now;
                        }
                    }
                    catch (Exception sendEx)
                    {
                        AppLogger.LogError($"[HOST BROADCAST] ❌ Failed to send broadcast #{broadcastCount}", sendEx, filename: "host_discovery");
                        AppLogger.LogError($"[HOST BROADCAST] Inner exception: {sendEx.InnerException?.Message}", filename: "host_discovery");
                    }
                    
                    await Task.Delay(3000, cancellationToken); // Broadcast every 3 seconds
                }
                
                AppLogger.LogInfo($"[HOST BROADCAST] Stopped broadcasting (total broadcasts: {broadcastCount})", filename: "host_discovery");
            }
            catch (OperationCanceledException)
            {
                AppLogger.LogInfo("[HOST BROADCAST] Broadcasting cancelled", filename: "host_discovery");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[HOST BROADCAST] ❌ Broadcasting error", ex, filename: "host_discovery");
                AppLogger.LogError($"[HOST BROADCAST] Exception type: {ex.GetType().Name}", filename: "host_discovery");
                AppLogger.LogError($"[HOST BROADCAST] Inner exception: {ex.InnerException?.Message}", filename: "host_discovery");
                Console.WriteLine($"Broadcasting error: {ex.Message}");
            }
            
            AppLogger.LogSeparator("", "host_discovery");
        }
    }
}
