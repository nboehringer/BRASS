﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BRASS.DataAccessLayer;
using BRASS.Models;
using BRASS.Models.PageModels;

namespace BRASS.Controllers
{
    public class RoutesController : Controller
    {
        private readonly SchoolContext _context;

        public RoutesController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Routes
        public async Task<IActionResult> Index()
        {
            using (var context = _context)
            {
                var RouteNumbers = context.Routes.Select(x => new { Text = "Route: " + x.RouteId.ToString(), Value = x.RouteId });

                var model = new RoutesPage();
                List<SelectListItem> routes = new List<SelectListItem>();
                foreach (var route in RouteNumbers)
                {
                    routes.Add(new SelectListItem { Text = route.Value.ToString(), Value = route.Text });
                }
                model.RouteList = new SelectList(routes, "Text", "Value");


                var studentQuery = context.Students.AsNoTracking().ToList();
                model.StudentList = studentQuery;

                var RouteStopsQuery = context.RouteStops.AsNoTracking().ToList();
                model.RouteStopsList = RouteStopsQuery;

                var RoutesQuery = context.Routes.AsNoTracking().ToList();
                model.RoutesList = RoutesQuery;

                var BusesQuery = context.Buses.AsNoTracking().ToList();
                model.BusList = BusesQuery;

                var unassignedStudentsQuery = context.Students.AsNoTracking()
                    .Where(x => x.StopId == 0)
                    .ToList();
                var UnassignedStudents = unassignedStudentsQuery.Count;
                model.unAssignedStudents = UnassignedStudents;

                var unassignedBusesQuery = context.Routes.AsNoTracking()
                    .Where(x => x.RouteId == 0)
                    .ToList();
                var UnassignedBuses = unassignedBusesQuery.Count;
                model.unAssignedBuses = UnassignedBuses;

                return View(model);
            }
        }

        // GET: Routes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes
                .FirstOrDefaultAsync(m => m.RouteId == id);
            if (routes == null)
            {
                return NotFound();
            }

            return View(routes);
        }

        // GET: Routes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteId,RouteGroup,BusId")] Routes routes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(routes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(routes);
        }

        // GET: Routes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes.FindAsync(id);
            if (routes == null)
            {
                return NotFound();
            }
            return View(routes);
        }

        // POST: Routes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteId,RouteGroup,BusId")] Routes routes)
        {
            if (id != routes.RouteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoutesExists(routes.RouteId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(routes);
        }

        // GET: Routes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routes = await _context.Routes
                .FirstOrDefaultAsync(m => m.RouteId == id);
            if (routes == null)
            {
                return NotFound();
            }

            return View(routes);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routes = await _context.Routes.FindAsync(id);
            _context.Routes.Remove(routes);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoutesExists(int id)
        {
            return _context.Routes.Any(e => e.RouteId == id);
        }

        public ActionResult GetStudentsOnRoute(int id, List<Routes> routeList)
        {
            List<RoutesPageTable> routesPageTable = new List<RoutesPageTable>();
            if (id == 0)
            {
                using (var context = _context)
                {
                    var routeStopIds = context.RouteStops.AsNoTracking()
                        .Select(x => x.StopId)
                        .ToList();

                    var studentList = context.Students.AsNoTracking()
                        .Where(x => routeStopIds.Contains(x.StopId))
                        .ToList();

                    foreach(Students student in studentList)
                    {
                        var studentStop = context.RouteStops.AsNoTracking().Where(x => x.StopId == student.StopId).Select(x => x.StopNumber).FirstOrDefault();
                        var studentRoute = context.RouteStops.AsNoTracking().Where(x => x.StopId == student.StopId).Select(x => x.RouteId).FirstOrDefault();
                        var studentBus = context.Buses.AsNoTracking().Where(x => x.RouteId == studentRoute).Select(x => x.BusNumb).FirstOrDefault();

                        RoutesPageTable row = new RoutesPageTable();
                        row.FirstName = student.FirstName;
                        row.LastName = student.LastName;
                        row.StreetAddress = student.StreetAddress;
                        row.StopNumber = studentStop;
                        row.BusNumber = studentBus;

                        routesPageTable.Add(row);
                    }

                    return Json(studentList);
                }
            }
            else
            {
                using (var context = _context)
                {
                    var model = new RoutesPage();

                    var routeStopIds = context.RouteStops.AsNoTracking()
                        .Where(x => x.RouteId == id)
                        .Select(x => x.StopId)
                        .ToList();

                    var studentList = context.Students.AsNoTracking()
                        .Where(x => routeStopIds.Contains(x.StopId))
                        .ToList();

                    foreach (Students student in studentList)
                    {
                        var studentStop = context.RouteStops.AsNoTracking().Where(x => x.StopId == student.StopId).Select(x => x.StopNumber).FirstOrDefault();
                        var studentRoute = context.RouteStops.AsNoTracking().Where(x => x.StopId == student.StopId).Select(x => x.RouteId).FirstOrDefault();
                        var studentBus = context.Buses.AsNoTracking().Where(x => x.RouteId == studentRoute).Select(x => x.BusNumb).FirstOrDefault();

                        RoutesPageTable row = new RoutesPageTable();
                        row.FirstName = student.FirstName;
                        row.LastName = student.LastName;
                        row.StreetAddress = student.StreetAddress;
                        row.StopNumber = studentStop;
                        row.BusNumber = studentBus;

                        routesPageTable.Add(row);
                    }

                    return Json(routesPageTable);
                }
            }
        }
    }
}
