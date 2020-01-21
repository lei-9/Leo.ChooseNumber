using System.Collections.Generic;
using Leo.ChooseNumber.Core.Redis;
using Leo.ChooseNumber.Modules;
using Newtonsoft.Json;

namespace Leo.ChooseNumber.Core.CMCC
{
    public class CMCC_RedisCache
    {

        public static List<CityDTO> GetCityList()
        {
            return JsonConvert.DeserializeObject<List<CityDTO>>(
                RedisDataBaseManager.GetDatabase().StringGet(RedisKeyConsts.CMCC_Citys));
        }

        public static List<ProvinceDTO> GetProvinceList()
        {
            return JsonConvert.DeserializeObject<List<ProvinceDTO>>(
                RedisDataBaseManager.GetDatabase().StringGet(RedisKeyConsts.CMCC_Provinces));
        }
    }
}
