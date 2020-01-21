using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Leo.ChooseNumber.Modules
{
    public class ProvinceDTO
    {
        public ProvinceDTO() { }
        public ProvinceDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public int Num { get; set; }
    }
}
