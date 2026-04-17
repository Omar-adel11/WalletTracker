using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.InstallmentsDTOs
{
    public class UpdateInstallmentDTO
    {
        //int installmentId, int wallet_id, decimal? newAmount, string? newDescription, int? newCategoryId, DateTime? newStartDate, DateTime? newEndDate, int? newTotalInstallments
        public int Id { get; set; }
        public decimal? amount { get; set; }
        public string? description { get; set; }
        public int? categoryId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int? NoOfPaidInstallments { get; set; }
    }
}
