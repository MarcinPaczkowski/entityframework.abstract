using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Models
{
    public class PageInfo
    {
        public int ItemsPerPage { get; set; }
        public int CurrentPageNumber { get; set; }
    }
}
