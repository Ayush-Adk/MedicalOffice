using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.CustomControllers;
using System.Numerics;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;
using MedicalOffice.ViewModels;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class AppointmentReasonController : LookupsController
    {
        private readonly MedicalOfficeContext _context;

        public AppointmentReasonController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: AppointmentReason
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: AppointmentReason/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppointmentReason/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ReasonName")] AppointmentReason appointmentReason)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(appointmentReason);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ExceptionMessageVM msg = new();
                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }

            return View(appointmentReason);
        }

        // GET: AppointmentReason/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AppointmentReasons == null)
            {
                return NotFound();
            }

            var appointmentReason = await _context.AppointmentReasons.FindAsync(id);
            if (appointmentReason == null)
            {
                return NotFound();
            }
            return View(appointmentReason);
        }

        // POST: AppointmentReason/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the Doctor to update
            var appointmentReasonToUpdate = await _context.AppointmentReasons
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (appointmentReasonToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<AppointmentReason>(appointmentReasonToUpdate, "",
                d => d.ReasonName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentReasonExists(appointmentReasonToUpdate.ID))
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
                    ExceptionMessageVM msg = new();
                    ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
                }
            }
            return View(appointmentReasonToUpdate);
        }

        // GET: AppointmentReason/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AppointmentReasons == null)
            {
                return NotFound();
            }

            var appointmentReason = await _context.AppointmentReasons
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointmentReason == null)
            {
                return NotFound();
            }

            return View(appointmentReason);
        }

        // POST: AppointmentReason/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AppointmentReasons == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.AppointmentReasons'  is null.");
            }
            var appointmentReason = await _context.AppointmentReasons
                  .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (appointmentReason != null)
                {
                    _context.AppointmentReasons.Remove(appointmentReason);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                ExceptionMessageVM msg = new();
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    msg.ErrProperty = "";
                    msg.ErrMessage = "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] +
                        " that has related records.";
                }
                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            return View(appointmentReason);
        }

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            string feedBack = string.Empty;
            if (theExcel != null)
            {
                string mimeType = theExcel.ContentType;
                long fileLength = theExcel.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
                    {
                        ExcelPackage excel;
                        using (var memoryStream = new MemoryStream())
                        {
                            await theExcel.CopyToAsync(memoryStream);
                            excel = new ExcelPackage(memoryStream);
                        }
                        var workSheet = excel.Workbook.Worksheets[0];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        int successCount = 0;
                        int errorCount = 0;
                        if (workSheet.Cells[1, 1].Text == "Appointment Reason")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                AppointmentReason appointmentReason = new AppointmentReason();
                                try
                                {
                                    // Row by row...
                                    appointmentReason.ReasonName = workSheet.Cells[row, 1].Text;
                                    _context.AppointmentReasons.Add(appointmentReason);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += "Error: Record " + appointmentReason.ReasonName +
                                            " was rejected as a duplicate." + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + appointmentReason.ReasonName +
                                            " caused a database error." + "<br />";
                                    }
                                    //Here is the trick to using SaveChanges in a loop.  You must remove the 
                                    //offending object from the cue or it will keep raising the same error.
                                    _context.Remove(appointmentReason);
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    if (ex.GetBaseException().Message.Contains("correct format"))
                                    {
                                        feedBack += "Error: Record " + appointmentReason.ReasonName
                                            + " was rejected becuase it was not in the correct format." + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + appointmentReason.ReasonName
                                            + " caused and error." + "<br />";
                                    }
                                }
                            }
                            feedBack += "Finished Importing " + (successCount + errorCount).ToString() +
                                " Records with " + successCount.ToString() + " inserted and " +
                                errorCount.ToString() + " rejected";
                        }
                        else
                        {
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> " +
                                "Remember, you must have the heading 'Appointment Reason' in the " +
                                "first cell of the first row.";
                        }
                    }
                    else
                    {
                        feedBack = "Error: That file is not an Excel spreadsheet.";
                    }
                }
                else
                {
                    feedBack = "Error:  file appears to be empty";
                }
            }
            else
            {
                feedBack = "Error: No file uploaded";
            }

            TempData["Feedback"] = feedBack + "<br /><br />";

            //Note that we are assuming that you are using the Preferred Approach to Lookup Values
            //And the custom LookupsController
            return Redirect(ViewData["returnURL"].ToString());
        }


        private bool AppointmentReasonExists(int id)
        {
          return _context.AppointmentReasons.Any(e => e.ID == id);
        }
    }
}
