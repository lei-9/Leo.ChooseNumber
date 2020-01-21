using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.ChooseNumber.Modules
{
    public class SearchResult
    {
        public SearchResult(string number, string province, string city, string payUrl, decimal price)
        {
            this.Number = number;
            this.Province = province;
            this.City = city;
            this.PayUrl = payUrl;
            this.Price = price;
        }
        public string Number { get; set; }

        public string Province { get; set; }
        public string City { get; set; }
        public string PayUrl { get; set; }
        public decimal Price { get; set; }
    }
}
