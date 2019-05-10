    //----------PasswordLib开始----------
    
using System;
namespace Blog.Core.FrameWork.Entity
{    
    /// <summary>
    /// 
    /// </summary>    
    public class PasswordLib//可以在这里加上基类等
    {
    //将该表下的字段都遍历出来，可以自定义获取数据描述等信息

      public int  PLID { get; set; }

      public bool  ? IsDeleted { get; set; }

      public string  plURL { get; set; }

      public string  plPWD { get; set; }

      public string  plAccountName { get; set; }

      public int  ? plStatus { get; set; }

      public int  ? plErrorCount { get; set; }

      public string  plHintPwd { get; set; }

      public string  plHintquestion { get; set; }

      public DateTime  ? plCreateTime { get; set; }

      public DateTime  ? plUpdateTime { get; set; }

      public DateTime  ? plLastErrTime { get; set; }

      public string  test { get; set; }
 

    }
}
    //----------PasswordLib结束----------
    