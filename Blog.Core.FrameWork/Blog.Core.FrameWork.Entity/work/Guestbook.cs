    //----------Guestbook开始----------
    
using System;
namespace Blog.Core.FrameWork.Entity
{    
    /// <summary>
    /// 
    /// </summary>    
    public class Guestbook//可以在这里加上基类等
    {
    //将该表下的字段都遍历出来，可以自定义获取数据描述等信息

      public int  id { get; set; }

      public int  blogId { get; set; }

      public DateTime  createdate { get; set; }

      public string  username { get; set; }

      public string  phone { get; set; }

      public string  QQ { get; set; }

      public string  body { get; set; }

      public string  ip { get; set; }

      public bool  isshow { get; set; }
 

    }
}
    //----------Guestbook结束----------
    