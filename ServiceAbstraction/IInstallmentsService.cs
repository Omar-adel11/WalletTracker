using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using ServiceAbstraction.DTOs.InstallmentsDTOs;

namespace ServiceAbstraction
{
    public interface IInstallmentsService
    {
        Task<IEnumerable<InstallmentDTO>> GetAllInstallmentsAsync(int userId);
        Task<InstallmentDTO> GetInstallmentsByIdAsync(int id);
        Task<InstallmentDTO> CreateInstallmentAsync(CreateInstallmentDTO createInstallmentDTO);
        Task DeleteInstallmentAsync(int installmentId);
        Task UpdateInstallmentAsync(UpdateInstallmentDTO updateInstallmentDTO);
        Task<bool> payInstallmentAsync(int installmentId, int wallet_id, MoneySource source);
    }
}
