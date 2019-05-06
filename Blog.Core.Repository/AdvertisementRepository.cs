using System;
using System.Collections.Generic;
using System.Text;
using Blog.Core.IRepository;

namespace Blog.Core.Repository  
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        public int Sum(int i, int j)
        {
            return i + j;
        }
    }
}
