using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileHash.Data;
using FileHash.Models;

namespace misc.Controllers
{
    public class HomeController : Controller
    {
        private readonly FileHashContext _context;

        public HomeController(FileHashContext context)
        {
            _context = context;
        }

        // GET: Hashes
        public async Task<IActionResult> Index(string hashtype, string searchString)
        {
            // Use LINQ to get list of hashtypes
            IQueryable<string> hashtypeQuery = from h in _context.FileMeta
                        orderby h.HashType
                        select h.HashType;

            var hashes = from f in _context.FileMeta
                        // where f.IsActive == true
                        select f;

            if (!String.IsNullOrEmpty(searchString)) {
                hashes = hashes.Where(s => s.Filename.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(hashtype))
            {
                hashes = hashes.Where(h => h.HashType == hashtype);
            }

            var hashTypeViewModel = new HashTypeViewModel {
                HashTypes = new SelectList(await hashtypeQuery.Distinct().ToListAsync()),
                HashList = await hashes.ToListAsync()
            };

            return View(hashTypeViewModel);
        }

        // GET: Hashes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileInfo = await _context.FileMeta
                .FirstOrDefaultAsync(m => m.FileId == id);
            if (fileInfo == null)
            {
                return NotFound();
            }

            return View(fileInfo);
        }

        // GET: Hashes/Create
        public IActionResult Create()
        {
            ViewData["FileId"] = new SelectList(_context.FileMeta, "FileId", "FileId");

            List<string> hashTypeList = new List<string>();
            hashTypeList.Add("md5");
            hashTypeList.Add("sha1");

            var hashViewModel = new HashViewModel {
                HashTypes = new SelectList(hashTypeList)
            };
            return View(hashViewModel);
        }

        // POST: Hashes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FileId,Filename,HashType,Size,LocalFile,Last,IsActive")] HashViewModel hashVM)
        {
            string hashString = "";
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            FileMeta fileInfo = new FileMeta();
            if (hashVM.LocalFile != null) {
                DateTime lastDate = baseDate.AddMilliseconds(hashVM.Last);
                fileInfo.Filename = WebUtility.HtmlEncode(Path.GetFileName(hashVM.LocalFile.FileName));
                fileInfo.Size = hashVM.Size.ToString() + " bytes";
                fileInfo.Last = lastDate;

                Task t = Task.Run(() => {
                    using (var ms = new MemoryStream()) {
                        hashVM.LocalFile.CopyTo(ms);
                        if (hashVM.HashType == "md5") {
                            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(ms.ToArray());
                            foreach (byte x in hash) {
                                hashString += String.Format("{0:x2}", x);
                            }
                            fileInfo.HashType = "md5";
                        } else if (hashVM.HashType == "sha1") {
                            byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(ms.ToArray());
                            foreach (byte x in hash) {
                                hashString += String.Format("{0:x2}", x);
                            }
                            fileInfo.HashType = "sha1";
                        }
                        else {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                });
                List<Task> tList = new List<Task>();
                tList.Add(t);

                while (tList.Count > 0) {
                    Task finished = await Task.WhenAny(tList);
                    tList.Remove(finished);
                }
                
                fileInfo.Hash = hashString;
            }
            if (ModelState.IsValid)
            {
                _context.Add(fileInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FileId"] = new SelectList(_context.FileMeta, "FileId", "FileId", fileInfo.FileId);
            return View(fileInfo);
        }

        // GET: Hashes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileInfo = await _context.FileMeta.FindAsync(id);
            if (fileInfo == null)
            {
                return NotFound();
            }
            ViewData["FileId"] = new SelectList(_context.FileMeta, "FileId", "FileId", fileInfo.FileId);

            List<string> hashTypeList = new List<string>();
            hashTypeList.Add("md5");
            hashTypeList.Add("sha1");

            var hashViewModel = new HashViewModel {
                FileId = fileInfo.FileId,
                Filename = fileInfo.Filename,
                Hash = fileInfo.Hash,
                HashTypes = new SelectList(hashTypeList),
                Size = fileInfo.Size
            };
            return View(hashViewModel);
            /*
            return View(fileInfo);
            */
        }

        // POST: Hashes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FileId,Filename,HashType,Size,LocalFile,Last,IsActive")] HashViewModel hashVM)
        {
            if (id != hashVM.FileId) {
                return NotFound();
            }

            string hashString = "";
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            FileMeta fileInfo = _context.FileMeta.FindAsync(id).Result;
            if (hashVM.LocalFile != null) {
                DateTime lastDate = baseDate.AddMilliseconds(hashVM.Last);
                if (fileInfo.Filename != WebUtility.HtmlEncode(Path.GetFileName(hashVM.LocalFile.FileName))) {
                    ViewData["FileId"] = new SelectList(_context.FileMeta, "FileId", "FileId", id);
                    ViewData["Message"] = @"Filename mismatch!";

                    List<string> hashTypeList = new List<string>();
                    hashTypeList.Add("md5");
                    hashTypeList.Add("sha1");
                    hashVM.HashTypes = new SelectList(hashTypeList);

                    return View(hashVM);
                }
                fileInfo.Size = hashVM.Size.ToString() + " bytes";
                fileInfo.Last = lastDate;

                Task t = Task.Run(() => {
                    using (var ms = new MemoryStream()) {
                        hashVM.LocalFile.CopyTo(ms);
                        if (hashVM.HashType == "md5") {
                            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(ms.ToArray());
                            foreach (byte x in hash) {
                                hashString += String.Format("{0:x2}", x);
                            }
                            fileInfo.HashType = "md5";
                        } else if (hashVM.HashType == "sha1") {
                            byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(ms.ToArray());
                            foreach (byte x in hash) {
                                hashString += String.Format("{0:x2}", x);
                            }
                            fileInfo.HashType = "sha1";
                        }
                        else {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                });
                List<Task> tList = new List<Task>();
                tList.Add(t);

                while (tList.Count > 0) {
                    Task finished = await Task.WhenAny(tList);
                    tList.Remove(finished);
                }
                
                fileInfo.Hash = hashString;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fileInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileInfoExists(fileInfo.FileId))
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
            ViewData["FileId"] = new SelectList(_context.FileMeta, "FileId", "FileId", fileInfo.FileId);
            return View(fileInfo);
        }

        // GET: Hashes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileInfo = await _context.FileMeta
                .FirstOrDefaultAsync(m => m.FileId == id);
            if (fileInfo == null)
            {
                return NotFound();
            }

            return View(fileInfo);
        }

        // POST: Hashes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fileInfo = await _context.FileMeta.FindAsync(id);
            _context.FileMeta.Remove(fileInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FileInfoExists(int id)
        {
            return _context.FileMeta.Any(e => e.FileId == id);
        }
    }
}
