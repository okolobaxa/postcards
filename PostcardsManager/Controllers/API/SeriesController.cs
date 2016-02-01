﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using PostcardsManager.ViewModels;
using PostcardsManager.Repositories;

namespace PostcardsManager.Controllers.API
{
    [EnableCors("*", "*", "*")]
    public class SeriesController : ApiController
    {
        [ResponseType(typeof(IEnumerable<SeriesViewModel>))]
        public HttpResponseMessage GetAll()
        {
            var seriesRepository = new SeriesRepository();

            IDisposable context;
            var series = seriesRepository.GetAll(out context).OrderByDescending(p => p.Id).ToList();

            using (context)
            {
                var results = series.Select(p => new SeriesViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    PublisherName = p.Publisher?.Name,
                    Year = p.Year
                });

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
        }
    }
}