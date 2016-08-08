using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace PokeScan
{
    public interface IPokeScan : IService
    {
        Task<Scan> GetScan(double lat, double lng);
    }
}
