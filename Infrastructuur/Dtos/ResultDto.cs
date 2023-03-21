using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Dtos
{
    public class ResultDto<T>
    {
        public List<string> Errors { get; set; } = new List<string>();
        public T? Result { get; set; }
    }
}
