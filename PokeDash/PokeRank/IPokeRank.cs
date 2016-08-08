using Microsoft.ServiceFabric.Services.Remoting;
using PokeScan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeRank
{
    public interface IPokeRank : IService
    {
        Task<List<int>> GetRank(List<Domain.Pokemon> pokeList);
        Task<List<int>> GetRankTest();
    }
}
