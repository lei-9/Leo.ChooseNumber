using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Leo.ChooseNumber.Core.Redis;
using Leo.ChooseNumber.Modules;
using Newtonsoft.Json;
using RestSharp;

namespace Leo.ChooseNumber.Core.CUCC
{
    public class CUCC_Search
    {

        #region 基础数据

        private static string RequestUrl = "http://num.10010.com/NumApp/NumberCenter/qryNum";

        private static object _cityLock = new object();
        private static List<CityDTO> _cityList;
        public static List<CityDTO> CityList
        {
            get
            {
                lock (_cityLock)
                {
                    if (!(_cityList?.Any() ?? false))
                        _cityList = CUCC_RedisCache.GetCityList();

                    return _cityList;
                }
            }
        }

        private static object _provinceLock = new object();
        private static List<ProvinceDTO> _provinceList;

        public static List<ProvinceDTO> ProvinceList
        {
            get
            {
                lock (_provinceLock)
                {
                    if (!(_provinceList?.Any() ?? false))
                        _provinceList = CUCC_RedisCache.GetProvinceList();

                    return _provinceList;
                }
            }
        }

        #endregion

        /// <summary>
        /// 查询中国联通
        /// </summary>
        /// <param name="provinceId">省id</param>
        /// <param name="cityCode">城市id</param>
        /// <param name="numNet">号段 只能为2到4位</param>
        /// <param name="codeTypeCode">靓号类型
        /// 1 = AAAAA
        /// 2 = AAAA
        /// 3 = ABCDE
        /// 4 = ABCD
        /// 5 = AAA
        /// 6 = AABB
        /// 7 = ABAB
        /// 8 = ABC
        /// 9 = AA
        /// </param>
        /// <param name="searchValue">关键字查询</param>
        public static void Search(int provinceId = 0, string cityCode = null, int? numNet = null, string codeTypeCode = null, string searchValue = null)
        {
            //http://num.10010.com/NumApp/chseNumList/init cucc选号页面
            //注意处理 configKey
            var searchCityList = CityList;
            if (provinceId > 0)
                searchCityList = searchCityList.Where(s => s.Province_Id == provinceId).ToList();

            if (!string.IsNullOrEmpty(cityCode))
                searchCityList = searchCityList.Where(s => s.CityId == cityCode).ToList();

            if (searchCityList.Any())
            {
                if (searchCityList.Count < 10)
                {
                    HandlerSearch(searchCityList, numNet, codeTypeCode, searchValue);
                }
                else
                {

                }

            }
        }

        private static void HandlerSearch(List<CityDTO> searchCityList, int? numNet, string codeTypeCode, string searchValue)
        {
            var client = new RestClient();

            //todo addHeaders

            foreach (var cityDto in searchCityList)
            {
                var request = new RestRequest(CreateRequestUrl(cityDto.Province_Id, cityDto.CityId, numNet, codeTypeCode, searchValue), Method.GET);

                var response = client.Execute(request);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonStr = response.Content.Replace("jsonp_queryMoreNums(", "");
                    //sub str
                    Console.WriteLine(jsonStr);
                    var cuccResponse = JsonConvert.DeserializeObject<CUCCResponse>(jsonStr);

                    if (cuccResponse.numArray?.Any() ?? false)
                    {
                        var matchList = cuccResponse.numArray.Where(n => n> 10000000000);
                        //todo hander success
                    }
                }
            }
        }

        /// <summary>
        /// 创建完整请求链接
        /// </summary>
        /// <param name="provinceId">省id</param>
        /// <param name="cityCode">城市id</param>
        /// <param name="numNet">号段</param>
        /// <param name="codeTypeCode">靓号类型</param>
        /// <param name="searchValue">关键字查询</param>
        /// <returns></returns>
        private static string CreateRequestUrl(int provinceId, string cityCode, int? numNet, string codeTypeCode, string searchValue)
        {

            /*
            * callback: jsonp_queryMoreNums 固定，回调函数方法名 
provinceCode: 11 省id
cityCode: 110 市id
advancePayLower: 0 
sortType: 1 排序类型
goodsNet: 4 
searchCategory: 3
qryType: 01
numNet: 186 号段
codeTypeCode: AAAA
searchValue : 1234
groupKey: 
            //省市查询
            http://num.10010.com/NumApp/NumberCenter/qryNum?callback=jsonp_queryMoreNums&provinceCode=11&cityCode=110&sortType=1&goodsNet=4&qryType=01&searchCategory=3&groupKey=

            //带号段
            http://num.10010.com/NumApp/NumberCenter/qryNum?callback=jsonp_queryMoreNums&provinceCode=11&cityCode=110&advancePayLower=0&sortType=1&goodsNet=4&searchCategory=3&qryType=01&numNet=186&groupKey=

            //带靓号
            http://num.10010.com/NumApp/NumberCenter/qryNum?callback=jsonp_queryMoreNums&provinceCode=11&cityCode=110&advancePayLower=0&sortType=1&goodsNet=4&searchCategory=3&qryType=01&codeTypeCode=AAAAA&groupKey=

            //带查询条件
            http://num.10010.com/NumApp/NumberCenter/qryNum?callback=jsonp_queryMoreNums&provinceCode=11&cityCode=110&advancePayLower=0&sortType=1&goodsNet=4&searchCategory=3&qryType=01&searchValue=5555&searchType=02&inputCode=&configKey=ZJ2t2ofiM9Ikpv4JE8Dctg%3D%3D&groupKey=

            //查询条件 + 号段
            http://num.10010.com/NumApp/NumberCenter/qryNum?callback=jsonp_queryMoreNums&provinceCode=11&cityCode=110&advancePayLower=0&sortType=1&goodsNet=4&searchCategory=3&qryType=01&numNet=186&groupKey=
            */
            //var url = new StringBuilder();
            //url.Append($"{RequestUrl}?callback=jsonp_queryMoreNums&provinceCode={provinceId}&cityCode={cityCode}");

            //
            return $"{RequestUrl}?callback=jsonp_queryMoreNums&provinceCode={provinceId}&cityCode={cityCode}&advancePayLower=0&sortType=1&goodsNet=4&searchCategory=3&qryType=01&codeTypeCode=AAA&groupKey=";
        }


        #region 初始化省市数据

        public static void Init_Data()
        {
            #region 省

            var provinceList = new List<ProvinceDTO>
            {
                new ProvinceDTO(11, "北京"),
                new ProvinceDTO(30, "安徽"),
                new ProvinceDTO(83, "重庆"),
                new ProvinceDTO(38, "福建"),
                new ProvinceDTO(51, "广东"),
                new ProvinceDTO(87, "甘肃"),
                new ProvinceDTO(59, "广西"),
                new ProvinceDTO(85, "贵州"),
                new ProvinceDTO(71, "湖北"),
                new ProvinceDTO(74, "湖南"),
                new ProvinceDTO(18, "河北"),
                new ProvinceDTO(76, "河南"),
                new ProvinceDTO(50, "海南"),
                new ProvinceDTO(34, "江苏"),
                new ProvinceDTO(90, "吉林"),
                new ProvinceDTO(75, "江西"),
                new ProvinceDTO(97, "黑龙江"),
                new ProvinceDTO(91, "辽宁"),
                new ProvinceDTO(88, "宁夏"),
                new ProvinceDTO(70, "青海"),
                new ProvinceDTO(17, "山东"),
                new ProvinceDTO(31, "上海"),
                new ProvinceDTO(19, "山西"),
                new ProvinceDTO(84, "陕西"),
                new ProvinceDTO(81, "四川"),
                new ProvinceDTO(10, "内蒙古"),
                new ProvinceDTO(13, "天津"),
                new ProvinceDTO(89, "新疆"),
                new ProvinceDTO(79, "西藏"),
                new ProvinceDTO(86, "云南"),
                new ProvinceDTO(36, "浙江")
            };

            #endregion

            #region 城市 

            // //a[contains(@class,'cityS')]//@value
            // //a[contains(@class,'cityS')]

            var provinceCityCountList = new List<int>
            {
                1,16,1,9,21,14,14,9,14,14,11,18,18,13,9,11,13,14,5,9,17,1,11,10,21,12,1,16,7,16,11
            };
            var cityNameStr = @"北京,
合肥,  
安庆,
蚌埠,
亳州,
池州,
滁州,
阜阳,
淮北,
淮南,
黄山,
六安,
马鞍山,
宿州,
铜陵,
芜湖,
宣城,重庆,
福州,
厦门,
泉州,
漳州,
宁德,
莆田,
南平,
三明,
龙岩,
广州,
深圳,
东莞,
佛山,
惠州,
珠海,
中山,
江门,
汕头,
湛江,
揭阳,
肇庆,
清远,
韶关,
潮州,
茂名,
河源,
汕尾,
阳江,
梅州,
云浮,
兰州,
酒泉,
庆阳,
天水,
武威,
临夏,
白银,
定西,
平凉,
陇南,
张掖,
嘉峪关,
金昌,
甘南,
南宁,
柳州,
桂林,
梧州,
玉林,
百色,
钦州,
河池,
北海,
防城港,
贵港,
贺州,
崇左,
来宾,
贵阳,
遵义,
安顺,
黔南,
黔东南,
铜仁,
毕节,
六盘水,
黔西南,
武汉,
宜昌,
荆州,
黄冈,
黄石,
襄阳,
孝感,
鄂州,
咸宁,
十堰,
随州,
荆门,
恩施,
仙桃/潜江/天门,
长沙,
衡阳,
株洲,
湘潭,
岳阳,
邵阳,
郴州,
常德,
益阳,
怀化,
永州,
娄底,
湘西,
张家界,
石家庄,
唐山,
秦皇岛,
邯郸,
邢台,
保定,
张家口,
承德,
廊坊,
沧州,
衡水,
郑州,
洛阳,
开封,
焦作,
新乡,
许昌,
漯河,
安阳,
商丘,
平顶山,
周口,
驻马店,
三门峡,
濮阳,
鹤壁,
济源,
信阳,
南阳,
海口,
三亚,
儋州,
琼海,
文昌,
万宁,
定安,
澄迈,
屯昌,
琼中,
乐东,
陵水,
保亭,
五指山,
东方,
临高,
昌江,
白沙,
南京,
苏州,
无锡,
常州,
扬州,
镇江,
南通,
徐州,
泰州,
盐城,
淮安,
连云港,
宿迁,
长春,
吉林,
延边,
四平,
通化,
白城,
辽源,
松原,
白山,
南昌,
九江,
上饶,
抚州,
宜春,
吉安,
赣州,
景德镇,
萍乡,
新余,
鹰潭,
哈尔滨,
齐齐哈尔,
牡丹江,
佳木斯,
绥化,
大庆,
鸡西,
黑河,
伊春,
双鸭山,
鹤岗,
七台河,
大兴安岭,
沈阳,
大连,
鞍山,
抚顺,
本溪,
丹东,
锦州,
营口,
阜新,
辽阳,
铁岭,
朝阳,
盘锦,
葫芦岛,
银川,
石嘴山,
吴忠,
固原,
中卫,
西宁,
海东,
格尔木,
海西,
海北,
海南,
黄南,
果洛,
玉树,
济南,
青岛,
淄博,
枣庄,
东营,
烟台,
潍坊,
济宁,
泰安,
威海,
日照,
莱芜,
临沂,
德州,
聊城,
滨州,
菏泽,
上海,
太原,
大同,
阳泉,
长治,
晋城,
朔州,
忻州,
晋中,
吕梁,
临汾,
运城,
西安,
咸阳,
渭南,
宝鸡,
汉中,
延安,
榆林,
铜川,
安康,
商洛,
成都,
自贡,
雅安,
绵阳,
乐山,
德阳,
攀枝花,
宜宾,
阿坝,
内江,
眉山,
资阳,
泸州,
南充,
达州,
遂宁,
广元,
广安,
巴中,
凉山,
甘孜,
呼和浩特,
包头,
乌海,
赤峰,
呼伦贝尔,
兴安盟,
通辽,
乌兰察布,
巴彦淖尔,
阿拉善盟,
鄂尔多斯,
锡林郭勒盟,
天津,
乌鲁木齐,
昌吉,
石河子,
奎屯,
塔城,
克拉玛依,
伊犁,
博乐,
阿勒泰,
吐鲁番,
哈密,
巴音郭楞,
阿克苏,
喀什,
和田,
克孜勒苏,
拉萨,
日喀则,
山南,
林芝,
昌都,
那曲,
阿里,
昆明,
德宏,
保山,
文山,
临沧,
怒江,
迪庆,
西双版纳,
红河,
大理,
丽江,
楚雄,
玉溪,
曲靖,
昭通,
普洱,
杭州,
宁波,
温州,
台州,
金华,
嘉兴,
绍兴,
湖州,
丽水,
衢州,
舟山";
            var cityIdStr = @"110,
305,
302,
301,
318,
317,
312,
306,
314,
307,
316,
304,
300,
313,
308,
303,
311,
831,
380,
390,
480,
395,
386,
385,
387,
389,
384,
510,
540,
580,
530,
570,
620,
556,
550,
560,
520,
526,
536,
535,
558,
531,
568,
670,
525,
565,
528,
538,
870,
931,
873,
877,
874,
878,
879,
871,
872,
960,
875,
876,
930,
961,
591,
593,
592,
594,
595,
596,
597,
598,
599,
590,
589,
588,
600,
601,
850,
787,
789,
788,
786,
785,
851,
853,
852,
710,
711,
712,
714,
715,
716,
717,
718,
719,
721,
723,
724,
727,
713,
741,
744,
742,
743,
745,
792,
748,
749,
747,
795,
796,
791,
793,
794,
188,
181,
182,
186,
185,
187,
184,
189,
183,
180,
720,
760,
761,
762,
763,
764,
765,
766,
767,
768,
769,
770,
771,
772,
773,
774,
775,
776,
777,
501,
502,
503,
A04,
A06,
A07,
A01,
A02,
A05,
A08,
A10,
A11,
A09,
A13,
A16,
A18,
A15,
A14,
340,
450,
330,
440,
430,
343,
358,
350,
445,
348,
354,
346,
349,
901,
902,
909,
903,
905,
907,
906,
904,
908,
750,
755,
757,
759,
756,
751,
752,
740,
758,
753,
754,
971,
973,
988,
976,
989,
981,
991,
990,
996,
994,
993,
992,
995,
910,
940,
912,
913,
914,
915,
916,
917,
918,
919,
911,
920,
921,
922,
880,
884,
883,
885,
886,
700,
701,
702,
704,
706,
705,
707,
708,
709,
170,
166,
150,
157,
156,
161,
155,
158,
172,
152,
154,
160,
153,
173,
174,
151,
159,
310,
190,
193,
192,
195,
194,
199,
198,
191,
200,
197,
196,
841,
844,
843,
840,
849,
842,
845,
846,
848,
847,
810,
818,
811,
824,
814,
825,
813,
817,
829,
816,
819,
830,
815,
822,
820,
821,
826,
823,
827,
812,
828,
101,
102,
106,
107,
108,
113,
109,
103,
105,
114,
104,
111,
130,
890,
891,
893,
892,
952,
899,
898,
951,
953,
894,
900,
895,
896,
897,
955,
954,
790,
797,
798,
799,
800,
801,
802,
860,
730,
731,
732,
733,
734,
735,
736,
861,
862,
863,
864,
865,
866,
867,
869,
360,
370,
470,
476,
367,
363,
365,
362,
469,
468,
364";

            #endregion


            var cityList = new List<CityDTO>();

            var cityNameList = cityNameStr.Split(",").ToList();
            var cityCodeList = cityIdStr.Split(",").ToList();

            foreach (var provinceDto in provinceList)
            {
                var index = provinceList.IndexOf(provinceDto);

                provinceDto.Num = provinceCityCountList[index];

                var skipNum = provinceList.Where(p => p.Num > 0).Sum(p => p.Num) - provinceDto.Num;
                var takeNum = provinceDto.Num;
                var cityNames = cityNameList.Skip(skipNum).Take(takeNum).ToList();
                var cityCodes = cityCodeList.Skip(skipNum).Take(takeNum).ToList();

                for (var i = 0; i < provinceDto.Num; i++)
                {
                    var cityCode = cityCodes[i].Replace("/r/n", "").Trim();
                    var cityName = cityNames[i].Replace("/r/n", "").Trim();
                    cityList.Add(new CityDTO(provinceDto.Id, provinceDto.Name, cityCode, cityName));

                }

                var redisDatabase = RedisDataBaseManager.GetDatabase();
                redisDatabase.StringSet(RedisKeyConsts.CUCC_Provinces, JsonConvert.SerializeObject(provinceList));
                redisDatabase.StringSet(RedisKeyConsts.CUCC_Citys, JsonConvert.SerializeObject(cityList));
            }
        }

        #endregion
    }
}
