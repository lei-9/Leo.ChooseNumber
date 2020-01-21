using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using Leo.ChooseNumber.Core.Redis;
using Leo.ChooseNumber.Modules;
using Newtonsoft.Json;
using RestSharp;

namespace Leo.ChooseNumber.Core.CMCC
{
    public class CMCC_Search
    {

        #region 基础数据


        /// <summary>
        /// 号段范围
        /// </summary>
        private static readonly List<int> Segment_Range = new List<int>
        {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
        };
        /// <summary>
        /// 尾号规则范围
        /// </summary>
        private static readonly List<int> Tail_Range = new List<int>
        {
            0,1,2,3,4,5,6,7
        };
        /// <summary>
        /// 谐音选号范围
        /// </summary>
        private static readonly List<int> Homophonic_Range = new List<int>
        {
            0,1,2,3,4,5,6,7,8,9
        };

        private static object _cityLock = new object();
        private static List<CityDTO> _cityList;
        public static List<CityDTO> CityList
        {
            get
            {
                lock (_cityLock)
                {
                    if (!(_cityList?.Any() ?? false))
                        _cityList = CMCC_RedisCache.GetCityList();

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
                        _provinceList = CMCC_RedisCache.GetProvinceList();

                    return _provinceList;
                }
            }
        }

        private static string CMCC_Host = "https://shop.10086.cn/";
        ///list/{固定134}_{省级id}_{市级id}_{当前页码}_{排序}_0_{号段，默认为0}_{尾号规则，默认为0}_{谐音选号，默认为0}?key={数字搜索}&p={页码}
        /// todo pageIndex and sort
        private static string CMCC_Resource = "list/134_$ProvinceId_$CityId_1_0_0_$Segment_$Tail_$Homophonic.html?key=$Key";

        #endregion

        #region 查询号码

        /// <summary>
        /// 中国移动 - 检索手机号码
        /// </summary>
        /// <param name="provinceId">省id</param>
        /// <param name="cityId">市id</param>
        /// <param name="segment">号段id
        ///    0 = 全部
        ///    1 = 134
        ///    2 = 135
        ///    3 = 136 
        ///    4 = 137
        ///    5 = 138
        ///    6 = 139
        ///    7 = 147
        ///    8 = 150
        ///    9 = 151
        ///    10 = 152
        ///    11 = 157
        ///    12 = 158
        ///    13 = 159
        ///    14 = 178
        ///    15 = 182
        ///    16 = 183
        ///    17 = 184
        ///    18 = 187
        ///    19 = 188
        /// </param>
        /// <param name="tail">尾号规则id
        ///    0 = '全部'
        ///    1 = '尾号AABB'
        ///    2 = '尾号AAAB'
        ///    3 = '尾号ABBA'
        ///    4 = '尾号ABAB'
        ///    5 = '尾号AAAA'
        ///    6 = '尾号ABCD'
        ///    7 = '尾号ABAC'
        ///</param>
        /// <param name="homophonic">谐音选号id
        ///
        ///  0 = '全部'
        ///  1 = '一生一世1314'
        ///  2 = '我爱你520'
        ///  3 = '生生世世3344'
        ///  4 = '发发发888'
        ///  5 = '六六大顺666'
        ///  6 = '一路发168'
        ///  7 = '要发要发6868'
        ///  8 = '我要发518'
        ///  9 = '一往情深1573'
        /// </param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static void Search(CMCC_ProvinceEnum provinceId = 0, CMCC_CityEnum cityId = 0, int segment = 0, int tail = 0, int homophonic = 0,
            string key = "")
        {
            var search_CityList = CityList;

            if (provinceId > 0)
                search_CityList = search_CityList.Where(c => c.Province_Id == (int)provinceId).ToList();
            if (cityId > 0)
                search_CityList = search_CityList.Where(c => c.City_Id == (int)cityId).ToList();

            if (!Segment_Range.Contains(segment))
                throw new ArgumentOutOfRangeException(nameof(segment));
            if (!Tail_Range.Contains(tail))
                throw new ArgumentOutOfRangeException(nameof(tail));
            if (!Homophonic_Range.Contains(homophonic))
                throw new ArgumentOutOfRangeException(nameof(homophonic));

            if (search_CityList.Any())
            {

                if (search_CityList.Count <= 10)
                {
                    HandlerSearch(new CMCCThreadParam
                    {
                        SearchCityList = search_CityList,
                        Segment = segment,
                        Tail = tail,
                        Homophonic = homophonic,
                        Key = key
                    });
                }
                else
                {
                    var thList = new List<Thread>();
                    //线程数量
                    var threadCount = 25;
                    //多线程
                    for (int i = 0; i < threadCount; i++)
                    {
                        var curSearchCityList = search_CityList.Skip((search_CityList.Count / threadCount) * i).Take(search_CityList.Count / threadCount).ToList();

                        thList.Add(new Thread(() => HandlerSearch(new CMCCThreadParam
                        {
                            SearchCityList = curSearchCityList,
                            Segment = segment,
                            Tail = tail,
                            Homophonic = homophonic,
                            Key = key
                        })));
                    }
                    //start thread
                    thList.ForEach(t => t.Start());

                    //除以线程数量之后有余数的情况
                    var surplusCount = search_CityList.Count % threadCount;
                    if (surplusCount > 0)
                    {
                        var curSearchCityList = search_CityList.Skip(search_CityList.Count / threadCount * threadCount).Take(surplusCount)
                            .ToList();

                        HandlerSearch(new CMCCThreadParam
                        {
                            SearchCityList = curSearchCityList,
                            Segment = segment,
                            Tail = tail,
                            Homophonic = homophonic,
                            Key = key
                        });
                    }
                }
            }
        }

        //private delegate void Delegate_HandlerSearch(object param);

        private static void HandlerSearch(object param)
        {
            var fileUtils = new FileUtils();
            var txtFilePath = $"files\\{Thread.GetCurrentProcessorId()}.txt";
            var p = (CMCCThreadParam)param;
            var redisDB = RedisDataBaseManager.GetDatabase();

            var client = new RestClient(CMCC_Host);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "zh,zh-CN;q=0.9");
            request.AddHeader("Host", "shop.10086.cn");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");


            foreach (var searchCity in p.SearchCityList)
            {
                var resource = CreateRequestResource(searchCity.Province_Id, searchCity.City_Id, p.Segment, p.Tail, p.Homophonic, p.Key);
                request.Resource = resource;
                var response = client.Execute(request);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response.Content);

                    var pageNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'pagination')]//em[2]");
                    
                    var matchCount = Convert.ToInt32(pageNode.InnerHtml.Replace("共", "").Replace("条", ""));
                    fileUtils.CreateOrAppendTxt(txtFilePath, $"{searchCity.Province}:{searchCity.City_Name} match：{matchCount} num");

                    //todo 获取分页大小 并根据当前页判断下次请求resource 并进行循环
                    if (matchCount > 0)
                    {
                        var number_list = doc.DocumentNode.SelectNodes("//tr//@phone_number");
                        var price_list = doc.DocumentNode.SelectNodes("//div[@class='goodsList']//td[contains(@class,'price')]//text()");
                        var pay_url_list = doc.DocumentNode.SelectNodes("//div[@class='goodsList']//a[contains(text(),'立即购买')]//@href");
                        var result = new List<SearchResult>();
                        foreach (var number in number_list)
                        {
                            var index = number_list.IndexOf(number);

                            var phoneNumber = number.Attributes["phone_number"].Value;
                            var payUrl = pay_url_list[index].Attributes["href"].Value;
                            var price = Convert.ToDecimal(price_list[index].InnerHtml.Substring(1));

                            result.Add(new SearchResult(phoneNumber, searchCity.Province, searchCity.City_Name, payUrl, price));
                        }
                        fileUtils.CreateOrAppendTxt(txtFilePath, $"{searchCity.Province}:{searchCity.City_Name} contains match result.");
                        //add match result to redis
                        var key = RedisKeyConsts.CreateSearchResultKey(searchCity.Province, searchCity.City_Name, p.Segment, p.Tail, p.Homophonic, p.Key);

                        redisDB.StringSet(key, JsonConvert.SerializeObject(result));
                    }

                    // sleep 100ms
                    Thread.Sleep(100);
                }
            }
            
            Console.WriteLine($"【{Thread.GetCurrentProcessorId()}】run over，search city num ：{p.SearchCityList.Count}");
            fileUtils.CreateOrAppendTxt(txtFilePath,
                $"【{Thread.GetCurrentProcessorId()}】run over，search city num ：{p.SearchCityList.Count}");
        }

        private static string CreateRequestResource(int provinceId, int cityId, int segment, int tail,
            int homophonic, string key)
        {
            return CMCC_Resource.Replace("$ProvinceId", provinceId.ToString())
                .Replace("$CityId", cityId.ToString())
                .Replace("$Segment", segment.ToString())
                .Replace("$Tail", tail.ToString())
                .Replace("$Homophonic", homophonic.ToString())
                .Replace("$Key", key);
        }

        #endregion

        #region 初始化省市数据

        public static void InitData()
        {
            var provinceList = new List<ProvinceDTO>
            {
                new ProvinceDTO(100,"北京"),
                new ProvinceDTO(230,"重庆"),
                new ProvinceDTO(210,"上海"),
                new ProvinceDTO(220,"天津")
            };
            var cityList = new List<CityDTO>
            {
                new CityDTO(100,"北京",100,"北京"),
                new CityDTO(230,"重庆",230,"重庆"),
                new CityDTO(210,"上海",210,"上海"),
                new CityDTO(220,"天津",220,"天津"),
            };

            var files = Directory.GetFiles(@"C:\doit\dev\Python\leo_project\search_supplier_number\data\cmcc_city\", "*.json");

            foreach (var file in files)
            {

                var fileName = Path.GetFileName(file).Replace(".json", "");
                Console.WriteLine($"处理文件：{fileName}");
                var split_fileName = fileName.Split("_");
                var provinceId = Convert.ToInt32(split_fileName[1]);
                var provinceName = split_fileName[0];
                provinceList.Add(new ProvinceDTO(provinceId, provinceName));

                var fileReader = File.ReadAllText(file);

                var curCityList = JsonConvert.DeserializeObject<BaseResponse<List<CityDTO>>>(fileReader);

                if (curCityList.code == 0 && (curCityList.data?.Any() ?? false))
                {
                    foreach (var curCity in curCityList.data)
                    {
                        cityList.Add(new CityDTO(curCity.Province_Id, provinceName, curCity.City_Id, curCity.City_Name));
                    }
                }
            }

            var redisDatabase = RedisDataBaseManager.GetDatabase();
            redisDatabase.StringSet(RedisKeyConsts.CMCC_Provinces, JsonConvert.SerializeObject(provinceList));
            redisDatabase.StringSet(RedisKeyConsts.CMCC_Citys, JsonConvert.SerializeObject(cityList));

            Console.WriteLine("初始化中国移动省市数据完成。");
        }


        public static void CreateProvinceAndCityEnumStr()
        {
            var provinceEnumStr = new StringBuilder();
            var cityEnumStr = new StringBuilder();
            foreach (var provinceDto in ProvinceList)
            {
                provinceEnumStr.Append($"{provinceDto.Name}={provinceDto.Id},");
            }

            foreach (var cityDto in CityList)
            {
                cityEnumStr.Append($"{cityDto.City_Name}={cityDto.City_Id},");
            }
            var provinceStr = provinceEnumStr.ToString().TrimEnd(',');
            var cityStr = cityEnumStr.ToString().TrimEnd(',');
        }

        #endregion

    }
}
