using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.Common;
using Blog.Core.Common.Redis;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {

        IAdvertisementServices advertisementServices;
        IBlogArticleServices blogArticleServices;
        IRedisCacheManager redisCacheManager;
        /// <summary>
        /// 构造函数 构造方法依赖注入
        /// </summary>
        /// <param name="advertisementServices"></param>
        /// <param name="blogArticleServices"></param>
        public BlogController(IAdvertisementServices advertisementServices, IBlogArticleServices blogArticleServices, IRedisCacheManager redisCacheManager)
        {
            this.advertisementServices = advertisementServices;
            this.blogArticleServices = blogArticleServices;
            this.redisCacheManager = redisCacheManager;
        }


        // GET: api/Blog/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "Get")]
        public async Task<object> Get(int id)
        {
            var model = await blogArticleServices.getBlogDetails(id);//调用该方法
            var data = new { success = true, data = model };
            return data;
        }

        // POST: api/Blog
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Blog/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 获取博客列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBlogs")]
        
        public async Task<List<BlogArticle>> GetBlogs()
        {
            var connect = Appsettings.app(new string[] { "AppSettings", "RedisCaching", "ConnectionString" });//按照层级的顺序，依次写出来
           

            List<BlogArticle> blogArticleList = new List<BlogArticle>();
            //Controller使用Redis缓存
            if (redisCacheManager.Get<object>("Redis.Blog") != null)
            {
                blogArticleList = redisCacheManager.Get<List<BlogArticle>>("Redis.Blog");
            }
            else
            {
                blogArticleList = await blogArticleServices.getBlogs();
                redisCacheManager.Set("Redis.Blog", blogArticleList, TimeSpan.FromHours(2));//缓存2小时
            }

            return blogArticleList;
        }
    }
}
