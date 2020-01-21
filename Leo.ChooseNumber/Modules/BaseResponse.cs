using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.ChooseNumber.Modules
{
    public class BaseResponse<T> 
    {
        public int code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
    }
}
