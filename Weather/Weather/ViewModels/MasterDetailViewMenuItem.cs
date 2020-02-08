using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather
{

    public class MasterDetailViewMenuItem
    {
        public MasterDetailViewMenuItem()
        {
            TargetType = typeof(MasterDetailViewMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}