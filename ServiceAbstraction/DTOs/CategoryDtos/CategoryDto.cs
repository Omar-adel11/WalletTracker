using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.CategoryDtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; } = null;
        public string Name { get; set; }
    }
}
