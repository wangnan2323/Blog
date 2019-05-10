using AutoMapper;
using Blog.Core.IRepository;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Blog.Core.Model.ViewModels;
using Blog.Core.Services.BASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Services
{
    public class BlogArticleServices : BaseServices<BlogArticle>, IBlogArticleServices
    {
        IMapper _mapper;
        IBlogArticleRepository dal;
        public BlogArticleServices(IBlogArticleRepository dal, IMapper mapper)
        {
            this._mapper = mapper;
            this.dal = dal;
            base.baseDal = dal;
        }

        /// <summary>
        /// 获取视图博客详情信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BlogViewModels> getBlogDetails(int id)
        {
            //不用automapper的写法
            //var bloglist = await dal.Query(a => a.bID > 0, a => a.bID);
            //var idmin = bloglist.FirstOrDefault() != null ? bloglist.FirstOrDefault().bID : 0;
            //var idmax = bloglist.LastOrDefault() != null ? bloglist.LastOrDefault().bID : 1;
            //var idminshow = id;
            //var idmaxshow = id;

            //BlogArticle blogArticle = new BlogArticle();

            //blogArticle = (await dal.Query(a => a.bID == idminshow)).FirstOrDefault();

            //BlogArticle prevblog = new BlogArticle();


            //while (idminshow > idmin)
            //{
            //    idminshow--;
            //    prevblog = (await dal.Query(a => a.bID == idminshow)).FirstOrDefault();
            //    if (prevblog != null)
            //    {
            //        break;
            //    }
            //}

            //BlogArticle nextblog = new BlogArticle();
            //while (idmaxshow < idmax)
            //{
            //    idmaxshow++;
            //    nextblog = (await dal.Query(a => a.bID == idmaxshow)).FirstOrDefault();
            //    if (nextblog != null)
            //    {
            //        break;
            //    }
            //}


            //blogArticle.btraffic += 1;
            //await dal.Update(blogArticle, new List<string> { "btraffic" });

            //BlogViewModels models = new BlogViewModels()
            //{
            //    bsubmitter = blogArticle.bsubmitter,
            //    btitle = blogArticle.btitle,
            //    bcategory = blogArticle.bcategory,
            //    bcontent = blogArticle.bcontent,
            //    btraffic = blogArticle.btraffic,
            //    bcommentNum = blogArticle.bcommentNum,
            //    bUpdateTime = blogArticle.bUpdateTime,
            //    bCreateTime = blogArticle.bCreateTime,
            //    bRemark = blogArticle.bRemark,
            //};

            //if (nextblog != null)
            //{
            //    models.next = nextblog.btitle;
            //    models.nextID = nextblog.bID;
            //}

            //if (prevblog != null)
            //{
            //    models.previous = prevblog.btitle;
            //    models.previousID = prevblog.bID;
            //}
            //return models;

            //使用automapper的写法
            var bloglist = await dal.Query(a => a.bID > 0, a => a.bID);
            var blogArticle = (await dal.Query(a => a.bID == id)).FirstOrDefault();
            BlogViewModels models = null;

            if (blogArticle != null)
            {
                BlogArticle prevblog;
                BlogArticle nextblog;
                int blogIndex = bloglist.FindIndex(item => item.bID == id);
                if (blogIndex >= 0)
                {
                    try
                    {
                        // 上一篇
                        prevblog = blogIndex > 0 ? (((BlogArticle)(bloglist[blogIndex - 1]))) : null;
                        // 下一篇
                        nextblog = blogIndex + 1 < bloglist.Count() ? (BlogArticle)(bloglist[blogIndex + 1]) : null;

                        // 注意就是这里,mapper
                        models = _mapper.Map<BlogViewModels>(blogArticle);

                        if (nextblog != null)
                        {
                            models.next = nextblog.btitle;
                            models.nextID = nextblog.bID;
                        }
                        if (prevblog != null)
                        {
                            models.previous = prevblog.btitle;
                            models.previousID = prevblog.bID;
                        }
                    }
                    catch (Exception) { }
                }
                blogArticle.btraffic += 1;
                await dal.Update(blogArticle, new List<string> { "btraffic" });
            }

            return models;

        }

        /// <summary>
        /// 获取博客列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<BlogArticle>> getBlogs()
        {
            var bloglist = await dal.Query(a => a.bID > 0, a => a.bID);

            return bloglist;

        }
    }
}
