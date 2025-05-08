using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebService
{
    public class DeepSeekResult
    {
        public string id { get; set; }
        public string _object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
        public PromptTokensDetails prompt_tokens_details { get; set; }
        public string system_fingerprint { get; set; }
    }
    // choices 数组中的单个对象类
    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    // message 对象类
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    // usage 对象类
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    // prompt_tokens_details 对象类
    public class PromptTokensDetails
    {
        public int cached_tokens { get; set; }
        public int prompt_cache_hit_tokens { get; set; }
        public int prompt_cache_miss_tokens { get; set; }
    }
}
