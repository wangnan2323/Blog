    //----------TopicDetail开始----------
    
using System;
namespace Blog.Core.FrameWork.Entity
{    
    /// <summary>
    /// 
    /// </summary>    
    public class TopicDetail//可以在这里加上基类等
    {
    //将该表下的字段都遍历出来，可以自定义获取数据描述等信息

      public int  TopicId { get; set; }

      public string  tdLogo { get; set; }

      public string  tdName { get; set; }

      public string  tdContent { get; set; }

      public string  tdDetail { get; set; }

      public string  tdSectendDetail { get; set; }

      public bool  tdIsDelete { get; set; }

      public int  tdRead { get; set; }

      public int  tdCommend { get; set; }

      public int  tdGood { get; set; }

      public DateTime  tdCreatetime { get; set; }

      public DateTime  tdUpdatetime { get; set; }

      public int  tdTop { get; set; }

      public string  tdAuthor { get; set; }

      public int  Id { get; set; }
 

    }
}
    //----------TopicDetail结束----------
    