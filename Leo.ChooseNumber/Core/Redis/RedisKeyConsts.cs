using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.ChooseNumber.Core.Redis
{
    public class RedisKeyConsts
    {
        #region 中国移动

        public const string CMCC_Provinces = "CMCC:Provinces";
        public const string CMCC_Citys = "CMCC:Citys";

        public const string CMCC_SearchResult = "CMCC:SearchResult:";




        public static string CreateSearchResultKey(string province, string city, int segment = 0, int tail = 0, int homophonic = 0,
            string key = "")
        {
            var sb = new StringBuilder();

            sb.Append(CMCC_SearchResult);
            sb.Append($"segment_{segment};tail_{tail};homophonic_{homophonic}");
            if (!string.IsNullOrEmpty(key))
                sb.Append($";key_{key}");

            sb.Append($":{province}-{city}");

            return sb.ToString();
        }

        #endregion

        #region 中国联通

        public const string CUCC_Provinces = "CUCC:Provinces";

        public const string CUCC_Citys = "CUCC:Citys";

        #endregion

    }
}
