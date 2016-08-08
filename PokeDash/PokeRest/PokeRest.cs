using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using Consul;

namespace PokeRest
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class PokeRest : StatelessService
    {
        private double _ConsulUpdateFreq;

        public PokeRest(StatelessServiceContext context)
            : base(context)
        {
            _ConsulUpdateFreq = 30.0;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(Startup.ConfigureApp, serviceContext, ServiceEventSource.Current, "ServiceEndpoint"))
            };
        }

        /// <summary>
        /// Maintain registration of the service with Consul
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {


                    using (var client = new ConsulClient())
                    {


                    
                        WriteResult result;
                        var serviceEndpoint = this.Context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
                        int port = serviceEndpoint.Port;

                        var services = await client.Agent.Services();

                        
                        if(!services.Response.ContainsKey("pokedash-scan"))
                        {

                       
                        var svcRegScan = new AgentServiceRegistration
                        {
                            Name = "pokedash-scan",
                            Port = port,
                        };
                        ServiceEventSource.Current.ServiceMessage(this.Context, "Registering service 'pokedash-scan' with Consul...");
                        result = await client.Agent.ServiceRegister(svcRegScan);

                        var svcChkScan = new AgentCheckRegistration
                        {
                            ID = "pokedash-scan",
                            ServiceID = "pokedash-scan",
                            HTTP = "http://localhost:"+ port + "/pokedash-scan-health",
                            Interval = TimeSpan.FromSeconds(30),
                            Name = "Scan-Health"

                        };
                        result = await client.Agent.CheckRegister(svcChkScan);
                        }


                        if (!services.Response.ContainsKey("pokedash-rank"))
                        {
                            var svcRegRank = new AgentServiceRegistration
                            {
                                Name = "pokedash-rank",
                                Port = port,
                            };
                            ServiceEventSource.Current.ServiceMessage(this.Context, "Registering service 'pokedash-rank' with Consul...");
                            result = await client.Agent.ServiceRegister(svcRegRank);

                            var svcChkRank = new AgentCheckRegistration
                            {
                                ID = "pokedash-rank",
                                ServiceID = "pokedash-rank",
                                HTTP = "http://localhost:" + port + "/pokedash-rank-health",
                                Interval = TimeSpan.FromSeconds(30),
                                Name = "Rank-Health"
                            };
                            result = await client.Agent.CheckRegister(svcChkRank);


                        }


                        if (!services.Response.ContainsKey("pokedash-uberestimate"))
                        {
                            var svcRegUber = new AgentServiceRegistration
                            {
                                Name = "pokedash-uberestimate",
                                Port = port,
                            };
                            ServiceEventSource.Current.ServiceMessage(this.Context, "Registering service 'pokedash-uber' with Consul...");
                            result = await client.Agent.ServiceRegister(svcRegUber);


                            var svcChkUber = new AgentCheckRegistration
                            {
                                ID = "pokedash-uberestimate",
                                ServiceID = "pokedash-uberestimate",
                                HTTP = "http://localhost:" + port + "/pokedash-uberestimate-health",
                                Interval = TimeSpan.FromSeconds(30),
                                Name = "Uber-Health"
                            };
                            result = await client.Agent.CheckRegister(svcChkUber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, ex.ToString());
                }

                await Task.Delay(TimeSpan.FromSeconds(_ConsulUpdateFreq), cancellationToken);
            }
        }
    }
}
