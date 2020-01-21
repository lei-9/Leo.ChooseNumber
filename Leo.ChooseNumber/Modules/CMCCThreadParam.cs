using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.ChooseNumber.Modules
{
   public class CMCCThreadParam
    {
        //  //List<CityDTO> search_CityList, int segment = 0, int tail = 0, int homophonic = 0,
        //string key = ""

        public List<CityDTO> SearchCityList { get; set; }
        public int Segment { get; set; }

        public int Tail { get; set; }
        public int Homophonic { get; set; }
        public string Key { get; set; }
    }
}
