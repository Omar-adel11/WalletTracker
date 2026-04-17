using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.InstallmentsDTOs;

namespace ServiceAbstraction
{
    public interface IInstallmentsService
    {
        Task<IEnumerable<InstallmentDTO>> GetAllInstallments();
        Task<int> CreateInstallment(CreateInstallmentDTO createInstallmentDTO);
        Task<int> DeleteInstallment(int installmentId);
        Task<int> UpdateInstallment(UpdateInstallmentDTO updateInstallmentDTO);
        Task<int> payInstallment(int installmentId, int wallet_id);
    }
}
