    //----------BlogArticle开始----------
    
using System;
namespace Blog.Core.FrameWork.Entity
{    
    /// <summary>
    /// 
    /// </summary>    
    public class BlogArticle//可以在这里加上基类等
    {
    //将该表下的字段都遍历出来，可以自定义获取数据描述等信息

      public int  bID { get; set; }

      public string  bsubmitter { get; set; }

      public string  btitle { get; set; }

      public string  bcategory { get; set; }

      public string  bcontent { get; set; }

      public int  btraffic { get; set; }

      public int  bcommentNum { get; set; }

      public DateTime  bUpdateTime { get; set; }

      public DateTime  bCreateTime { get; set; }

      public string  bRemark { get; set; }

      public bool  ? IsDeleted { get; set; }
 

    }
}
    //----------BlogArticle结束----------
    