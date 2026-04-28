using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using ServiceAbstraction.DTOs.InstallmentsDTOs;
using Shared;

namespace ServiceAbstraction
{
    public interface IInstallmentsService
    {
        Task<PagedResult<InstallmentDTO>> GetAllInstallmentsAsync(int userId, int? PageNumber = 1, int? PageSize = 5);
        Task<InstallmentDTO> GetInstallmentsByIdAsync(int id,int userId);
        Task<InstallmentDTO> CreateInstallmentAsync(int userId, CreateInstallmentDTO createInstallmentDTO);
        Task DeleteInstallmentAsync(int installmentId, int userId);
        Task UpdateInstallmentAsync(int userId,int id,UpdateInstallmentDTO updateInstallmentDTO);
        Task<bool> payInstallmentAsync(int installmentId,int userId, PayInstallmentDTO dto);
    }
}
