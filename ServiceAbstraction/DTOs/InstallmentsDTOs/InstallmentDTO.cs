using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.InstallmentsDTOs
{
    public class InstallmentDTO
    {
        public int id { get; set; }
        public Money Amount { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public DateTimeOffset LastDate { get; set; }
        public int? NoOfPaidInstallments { get; set; } = 0;
        public string? To { get; set; }
        public string? Category { get; set; }

    }
}
