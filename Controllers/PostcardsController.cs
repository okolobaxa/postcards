﻿using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PostcardsManager.DAL;
using PostcardsManager.Models;
using PostcardsManager.ViewModels;
using Resourses;

namespace PostcardsManager.Controllers
{
    public class PostcardsController : Controller
    {
        private readonly PostcardContext db = new PostcardContext();
        // GET: Series
        public ActionResult Index(int? selectedSeries, int? selectedPhotographer)
        {
            var series = db.Series;
            ViewBag.SelectedSeries = new SelectList(series, "Id", "Title", selectedSeries);
            var seriesId = selectedSeries.GetValueOrDefault();

            var photo = db.Photographers;
            ViewBag.SelectedPhotographers = new SelectList(photo, "Id", "FullName", selectedPhotographer);
            var photoId = selectedPhotographer.GetValueOrDefault();

            var postcards = db.Postcards
                .Where(c => !selectedSeries.HasValue || c.SeriesId == seriesId)
                .Where(c => !selectedPhotographer.HasValue || c.PhotographerId == photoId)
                .OrderBy(d => d.SeriesId)
                .Include(d => d.Series)
                .Include(d => d.Photographer);

            return View(postcards.ToList());
        }

        // GET: Series/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var postcard = db.Postcards.Find(id);
            if (postcard == null)
            {
                return HttpNotFound();
            }
            return View(postcard);
        }

        public ActionResult Create()
        {
            PopulateSeriesDropDownList();
            PopulatePhotographersDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(
                Include =
                    "Year, PhotographerId, SeriesId, ImageFrontUrl, FrontTitle, FrontTitleFont, FrontTitleFontColor, ImageBackUrl, BackTitle, BackTitleFont, BackTitleFontColor,B ackTitlePlace, BackType, NumberInSeries, PostDate"
                )] PostcardEditViewModel postcardVm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var storage = db.Storages.ToList().First(s => s.IsActive);

                    Image imageFront = null;
                    if (postcardVm.ImageFrontUrl != null)
                    {
                        imageFront = new Image
                        {
                            StorageId = storage.Id,
                            Url = postcardVm.ImageFrontUrl
                        };
                        
                        db.Images.Add(imageFront);
                    }

                    Image imageBack = null;
                    if (postcardVm.ImageBackUrl != null)
                    {
                        imageBack = new Image
                        {
                            StorageId = storage.Id,
                            Url = postcardVm.ImageBackUrl
                        };

                        db.Images.Add(imageBack);
                    }

                    var postcard = new Postcard
                    {
                        Year = postcardVm.Year,
                        PhotographerId = postcardVm.PhotographerId,
                        SeriesId = postcardVm.SeriesId,
                        FrontTitle = postcardVm.FrontTitle,
                        FrontTitleFont = postcardVm.FrontTitleFont,
                        FrontTitleFontColor = postcardVm.FrontTitleFontColor,
                        BackTitle = postcardVm.BackTitle,
                        BackTitleFont = postcardVm.BackTitleFont,
                        BackTitleFontColor = postcardVm.BackTitleFontColor,
                        BackTitlePlace = postcardVm.BackTitlePlace,
                        BackType = postcardVm.BackType,
                        NumberInSeries = postcardVm.NumberInSeries,
                        PostDate = postcardVm.PostDate
                    };

                    if (imageFront != null)
                    {
                        postcard.ImageFront = imageFront;
                    }
                    if (imageFront != null)
                    {
                        postcard.ImageBack = imageBack;
                    }

                    db.Postcards.Add(postcard);

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("",
                    Resources
                        .PostcardsController_Create_Unable_to_save_changes__Try_again__and_if_the_problem_persists__see_your_system_administrator_);
            }

            PopulateSeriesDropDownList(postcardVm.SeriesId);
            PopulatePhotographersDropDownList(postcardVm.PhotographerId);
            return View(postcardVm);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var postcard = db.Postcards.Find(id);
            if (postcard == null)
            {
                return HttpNotFound();
            }
            PopulateSeriesDropDownList(postcard.SeriesId);
            PopulatePhotographersDropDownList(postcard.PhotographerId);
            return View(postcard);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var postcardToUpdate = db.Postcards.Find(id);
            if (TryUpdateModel(postcardToUpdate, "", new[] {"FrontTitle", "Year", "SeriesId", "PhotographerId", "ImageLink"}))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("",
                        Resources
                            .PostcardsController_Create_Unable_to_save_changes__Try_again__and_if_the_problem_persists__see_your_system_administrator_);
                }
            }
            PopulateSeriesDropDownList(postcardToUpdate.SeriesId);
            PopulatePhotographersDropDownList(postcardToUpdate.PhotographerId);

            return View(postcardToUpdate);
        }

        private void PopulateSeriesDropDownList(object selectedSeries = null)
        {
            var seriesQuery = from d in db.Series
                select d;
            ViewBag.SeriesId = new SelectList(seriesQuery, "Id", "Title", selectedSeries);
        }

        private void PopulatePhotographersDropDownList(object selectedPhotographer = null)
        {
            var photographerQuery = from d in db.Photographers
                select d;
            ViewBag.PhotographerId = new SelectList(photographerQuery, "Id", "FullName",
                selectedPhotographer);
        }

        // GET: Series/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var postcard = db.Postcards.Find(id);
            if (postcard == null)
            {
                return HttpNotFound();
            }
            return View(postcard);
        }

        // POST: Series/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var series = db.Postcards.Find(id);
            db.Postcards.Remove(series);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}