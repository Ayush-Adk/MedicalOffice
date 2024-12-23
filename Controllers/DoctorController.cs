﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using MedicalOffice.Utilities;
using MedicalOffice.CustomControllers;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize]
    public class DoctorController : ElephantController
    {
        private readonly MedicalOfficeContext _context;

        public DoctorController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Doctor
        public async Task<IActionResult> Index(int? page, int? pageSizeID)
        {
            var doctors = _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .AsNoTracking();

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Doctor>.CreateAsync(doctors.AsNoTracking(), page ?? 1, pageSize);

            if (User.IsInRole("Admin"))
            {
                return View("IndexAdmin", pagedData);
            }
            else if (User.IsInRole("Supervisor"))
            {
                return View("IndexSupervisor", pagedData);
            }
            else
            {
                return View(pagedData);
            }

        }

        // GET: Doctor/Details/5
        [Authorize(Roles ="Admin,Supervisor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            Doctor doctor = new Doctor();
            PopulateAssignedSpecialtyData(doctor);
            return View(doctor);
        }

        // POST: Doctor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName")] Doctor doctor,
            string[] selectedOptions, List<IFormFile> theFiles)
        {
            try
            {
                UpdateDoctorSpecialties(selectedOptions, doctor);
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(doctor, theFiles);
                    _context.Add(doctor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { doctor.ID });

                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            //Validation Error so give the user another chance.
            PopulateAssignedSpecialtyData(doctor);
            return View(doctor);
        }

        // GET: Doctor/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
               .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
               .Include(d => d.DoctorDocuments)
               .FirstOrDefaultAsync(d => d.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            PopulateAssignedSpecialtyData(doctor);
            return View(doctor);
        }

        // POST: Doctor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions, List<IFormFile> theFiles)
        {
            //Go get the Doctor to update
            var doctorToUpdate = await _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (doctorToUpdate == null)
            {
                return NotFound();
            }

            //Update the Doctor's Specialties
            UpdateDoctorSpecialties(selectedOptions, doctorToUpdate);


            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Doctor>(doctorToUpdate, "",
                d => d.FirstName, d => d.MiddleName, d => d.LastName))
            {
                try
                {
                    await AddDocumentsAsync(doctorToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    //Instead of going back to the Index, why not show the revised
                    //version in full detail?
                    return RedirectToAction("Details", new { doctorToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctorToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }
            //Validation Error so give the user another chance.
            PopulateAssignedSpecialtyData(doctorToUpdate);
            return View(doctorToUpdate);

        }

        // GET: Doctor/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSpecialties).ThenInclude(d => d.Specialty)
                .Include(d => d.DoctorDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (doctor != null)
                {
                    _context.Doctors.Remove(doctor);
                }

                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Doctor. Remember, you cannot delete a Doctor that has patients assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }
            return View(doctor);
        }
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();

            if (theFile?.FileContent?.Content == null || theFile.MimeType == null)
            {
                return new FileContentResult(Array.Empty<byte>(), "application/octet-stream");
            }

            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }
        [Authorize(Roles = "Admin,Supervisor")]
        public PartialViewResult ListOfSpecialtiesDetails(int id)
        {
            var query = from s in _context.DoctorSpecialties.Include(p => p.Specialty)
                        where s.DoctorID == id
                        orderby s.Specialty.SpecialtyName
                        select s;
            return PartialView("_ListOfSpecialities", query.ToList());
        }
        [Authorize(Roles = "Admin,Supervisor")]
        public PartialViewResult ListOfPatientsDetails(int id)
        {
            var query = from p in _context.Patients
                        where p.DoctorID == id
                        orderby p.LastName, p.FirstName
                        select p;
            return PartialView("_ListOfPatients", query.ToList());
        }
        [Authorize(Roles = "Admin,Supervisor")]
        public PartialViewResult ListOfDocumentsDetails(int id)
        {
            var query = from p in _context.DoctorDocuments
                        where p.DoctorID == id
                        orderby p.FileName
                        select p;
            return PartialView("_ListOfDocuments", query.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult GetSpecialties(string skip)
        {
            return Json(SpecialtySelectList(skip));
        }

        //For Adding Specialty
        private SelectList SpecialtySelectList(string skip)
        {
            var SpecialtyQuery = _context.Specialties
                .AsNoTracking();

            if (!String.IsNullOrEmpty(skip))
            {
                //Convert the string to an array of integers
                //so we can make sure we leave them out of the data we download
                string[] avoidStrings = skip.Split('|');
                int[] skipKeys = Array.ConvertAll(avoidStrings, s => int.Parse(s));
                SpecialtyQuery = SpecialtyQuery
                    .Where(s => !skipKeys.Contains(s.ID));
            }
            return new SelectList(SpecialtyQuery.OrderBy(d => d.SpecialtyName), "ID", "SpecialtyName");
        }

        private void PopulateAssignedSpecialtyData(Doctor doctor)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Specialties;
            var currentOptionsHS = new HashSet<int>(doctor.DoctorSpecialties.Select(b => b.SpecialtyID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.SpecialtyName
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.SpecialtyName
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateDoctorSpecialties(string[] selectedOptions, Doctor doctorToUpdate)
        {
            if (selectedOptions == null)
            {
                doctorToUpdate.DoctorSpecialties = new List<DoctorSpecialty>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(doctorToUpdate.DoctorSpecialties.Select(b => b.SpecialtyID));
            foreach (var s in _context.Specialties)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(s.ID))//but not currently in the Doctor's collection - Add it!
                    {
                        doctorToUpdate.DoctorSpecialties.Add(new DoctorSpecialty
                        {
                            SpecialtyID = s.ID,
                            DoctorID = doctorToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(s.ID))//but is currently in the Doctor's collection - Remove it!
                    {
                        DoctorSpecialty? specToRemove = doctorToUpdate.DoctorSpecialties
                            .FirstOrDefault(d => d.SpecialtyID == s.ID);
                        if (specToRemove != null)
                        {
                            _context.Remove(specToRemove);
                        }
                    }
                }
            }
        }

        private async Task AddDocumentsAsync(Doctor doctor, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        DoctorDocument d = new DoctorDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        doctor.DoctorDocuments.Add(d);
                    };
                }
            }
        }


        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.ID == id);
        }
    }
}
