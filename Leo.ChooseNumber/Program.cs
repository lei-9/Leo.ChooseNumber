using System;
using Leo.ChooseNumber.Core.CMCC;

namespace Leo.ChooseNumber
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CMCC_Search.Search(key: "000");
                //CMCC_Search.InitData();
                //3连号查询
                //for (int i = 0; i < 1; i++)
                //{
                //    CMCC_Search.Search(key: $"{i}{i}{i}");
                //}


                //for (int i = 1; i < 10; i++)
                //{
                //    if (i <= 7)
                //    {   //
                //        CMCC_Search.Search(tail: i);
                //    }
                //    CMCC_Search.Search(homophonic: i);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Console.WriteLine("程序运行结束。");
            Console.ReadKey();
        }
    }
}
