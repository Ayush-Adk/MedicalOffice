﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.CustomControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MedicalOffice.ViewModels;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class SpecialtyController : LookupsController
    {
        private readonly MedicalOfficeContext _context;

        public SpecialtyController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Specialty
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Specialty/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specialty/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,SpecialtyName")] Specialty specialty)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(specialty);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ExceptionMessageVM msg = new();
                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            //Decide if we need to send the Validaiton Errors directly to the client
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                //Was an AJAX request so build a message with all validation errors
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                //Note: returning a BadRequest results in HTTP Status code 400
                return BadRequest(errorMessage);
            }


            return View(specialty);
        }

        // GET: Specialty/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Specialties == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }
            return View(specialty);
        }

        // POST: Specialty/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var specialtyToUpdate = await _context.Specialties
                    .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (specialtyToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Specialty>(specialtyToUpdate, "",
                d => d.SpecialtyName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecialtyExists(specialtyToUpdate.ID))
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
            return View(specialtyToUpdate);
        }

        // GET: Specialty/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Specialties == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(m => m.ID == id);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        // POST: Specialty/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Specialties == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.Specialties'  is null.");
            }
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (specialty != null)
                {
                    _context.Specialties.Remove(specialty);
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
            return View(specialty);
        }

        private bool SpecialtyExists(int id)
        {
          return _context.Specialties.Any(e => e.ID == id);
        }
    }
}
