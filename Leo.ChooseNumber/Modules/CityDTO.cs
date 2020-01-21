using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.ChooseNumber.Modules
{
    public class CityDTO
    {
        public CityDTO() { }
        public CityDTO(int provinceId, string provinceName,int cityId, string cityName)
        {
            Province_Id = provinceId;
            Province = provinceName;
            City_Id = cityId;
            City_Name = cityName;
        }

        public CityDTO(int provinceId, string provinceName, string cityId, string cityName)
        {
            Province_Id = provinceId;
            Province = provinceName;
            CityId = cityId;
            City_Name = cityName;
        }

        /// <summary>
        /// 省id
        /// </summary>
        public int Province_Id { get; set; }
        /// <summary>
        /// 城市id - 中国移动使用
        /// </summary>
        public int City_Id { get; set; }
        /// <summary>
        /// 城市id - 中国联通使用
        /// </summary>
        public string CityId { get; set; }

        public string Province { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        public string City_Name { get; set; }
    }
}
