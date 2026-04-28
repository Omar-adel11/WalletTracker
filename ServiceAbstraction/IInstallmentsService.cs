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
        Task<InstallmentDTO> GetInstallmentsByIdAsync(int id,int userId);
        Task<InstallmentDTO> CreateInstallmentAsync(int userId, CreateInstallmentDTO createInstallmentDTO);
        Task DeleteInstallmentAsync(int installmentId, int userId);
        Task UpdateInstallmentAsync(int userId,int id,UpdateInstallmentDTO updateInstallmentDTO);
        Task<bool> payInstallmentAsync(int installmentId,int userId, PayInstallmentDTO dto);
    }
}
