using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface ICategoryService
    {
        Task<int> CreateCategory(string name);
        Task<int> DeleteCategory(int CategoryId);
        Task<int> UpdateCategory(int CategoryId, string newName);
    }
}
