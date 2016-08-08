using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokeUber
{
    public interface IPokeUber : IService
    {
        Task<List<Estimate>> GetEstimate(double slat, double slng, List<UberRequestInfo> uberRequestInfo);
    }
}
