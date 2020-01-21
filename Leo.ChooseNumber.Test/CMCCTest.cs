using System;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using IOS.ConsoleApp.Core;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;

namespace Leo.ChooseNumber.Test
{
    [TestClass]
    public class CMCCTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            var client = new RestClient("https://shop.10086.cn/");
            var request = new RestRequest("list/134_200_754_1_0_0_1_0_0.html");
            request.Method = Method.POST;

            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(response.Content);

                var number_list = doc.DocumentNode.SelectNodes("//div[@class='goodsList']//td[contains(@class,'name')]//text()");
                var price_list = doc.DocumentNode.SelectNodes("//div[@class='goodsList']//td[contains(@class,'price')]//text()");
                var pay_url_list = doc.DocumentNode.SelectNodes("//div[@class='goodsList']//a[contains(text(),'Á¢¼´¹ºÂò')]//@href");

                foreach (var number in number_list)
                {
                    var index = number_list.IndexOf(number);

                    var phoneNumber = number.InnerHtml;
                    var price = price_list[index].InnerHtml.Substring(1);
                    var payUrl = pay_url_list[index].Attributes["href"].Value;
                }
            }
        }

        [TestMethod]
        public void CreateTxt()
        {
            var filePath = "files\\1.txt";
            //FileUtils.CreateOrAppendTxt(filePath,"test123");
        }
    }
}
