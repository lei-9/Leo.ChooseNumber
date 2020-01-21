# Leo.ChooseNumber
c# - 中国移动、联通、电信号码抓取控制台程序

+ ##### 第一次运行需要初始化省市数据到redis，请调用CMCC_Search.InitData
+ ##### 控制台直接运行即可，目前只完成了中国移动的选号页面查询。具体见 CMCC_Search.Search 方法
+ ##### 查询结果只支持redis输出，需要在RedisDataBaseManager 配置连接串。
