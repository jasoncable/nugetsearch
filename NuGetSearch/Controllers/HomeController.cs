using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using NuGetSearch.Models;
using NuGetSearch.Repositories;

namespace NuGetSearch.Controllers
{
    public class HomeController : Controller
    {
        private ISearchRepo _repo;
        private NuGetSearchAppConfig _config;

        public HomeController(IOptions<NuGetSearchAppConfig> config, ISearchRepo repo)
        {
            _repo = repo;
            _config = config.Value;
            _repo.Initialize(_config);
        }

        public IActionResult Index()
        {
            return View(new NuGetSearchMainSearchResult());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(NuGetSearchMainSearchResult model)
        {
            return View(_repo.DoSearch(model.SearchInput, model.Page));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
