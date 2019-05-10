    //----------Topic开始----------
    
using System;
namespace Blog.Core.FrameWork.Entity
{    
    /// <summary>
    /// 
    /// </summary>    
    public class Topic//可以在这里加上基类等
    {
    //将该表下的字段都遍历出来，可以自定义获取数据描述等信息

      public string  tLogo { get; set; }

      public string  tName { get; set; }

      public string  tDetail { get; set; }

      public string  tAuthor { get; set; }

      public string  tSectendDetail { get; set; }

      public bool  tIsDelete { get; set; }

      public int  tRead { get; set; }

      public int  tCommend { get; set; }

      public int  tGood { get; set; }

      public DateTime  tCreatetime { get; set; }

      public DateTime  tUpdatetime { get; set; }

      public int  Id { get; set; }
 

    }
}
    //----------Topic结束----------
    