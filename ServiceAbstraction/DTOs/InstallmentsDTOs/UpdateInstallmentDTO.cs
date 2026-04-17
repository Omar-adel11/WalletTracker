using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.InstallmentsDTOs
{
    public class UpdateInstallmentDTO
    {
        public int Id { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int? NoOfPaidInstallments { get; set; }
    }
}
