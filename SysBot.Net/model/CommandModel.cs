using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SysBot.Net.model
{
    public class CommandModel
    {
        public CommandModel()
        {

        }

        public String command { get; set; }

        public Dictionary<String, Object> param { get; set; }

        public Object data { get; set; }

        public int code { get; set; }

        public String error { get; set; }


    }
}


