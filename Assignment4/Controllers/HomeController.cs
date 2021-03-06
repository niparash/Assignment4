﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Assignment4.Models.EF_model;
using Assignment4.Models;
// ADD THESE DIRECTIVES
using Assignment4.DataAccess;
using Newtonsoft.Json;
using System.Net.Http;

namespace API_Simple.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;
        string BASE_URL = "https://api.iextrading.com/1.0/";
        HttpClient httpClient;

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
            
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new
                System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /*
            Calls the IEX reference API to get the list of symbols.
            Returns a list of the companies whose information is available. 
        */
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_Symbols = BASE_URL + "ref-data/symbols";
            string companyList = "";
            List<Company> companies = null;

            // Connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_Symbols);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_Symbols).GetAwaiter().GetResult();

            // Read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // Parse the Json strings as C# objects
            if (!companyList.Equals(""))
            {
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                companies = companies.GetRange(0, 50);
            }

            return companies;
        }

        public List<Financials> GetFinancials(string symbol)
        {
            string IEXTrading_API_Financials = BASE_URL + "/stock/{symbol}/financials";
            string totalRevenue = "";
            List<Financials> financials = null;

            // Connect to the IEXTrading API and retrieve information
            httpClient.BaseAddress = new Uri(IEXTrading_API_Financials);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_Financials).GetAwaiter().GetResult();

            // Read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                totalRevenue = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // Parse the Json strings as C# objects
            if (!totalRevenue.Equals(""))
            {
                financials = JsonConvert.DeserializeObject<List<Financials>>(totalRevenue);
                financials = financials.GetRange(0, 50);
            }

            return financials;
        }


        public IActionResult Index()
        {
            // Get the data from the List using GetSymbols method
            List<Company> companies = GetSymbols();
            TempData["Companies"] = JsonConvert.SerializeObject(companies);
            // Send the data to the Index view
            return View(companies);
        }

        public IActionResult Financials()
        {
            // Get the data from the List using GetSymbols method
            List<Financials> financials = GetFinancials("AAPL");
            TempData["Financials"] = JsonConvert.SerializeObject(financials);
            // Send the data to the Index view
            return View(financials);
        }

        public IActionResult PopulateSymbols()
        {
            // Retrieve the companies that were saved in the symbols method
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());

            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }

            dbContext.SaveChanges();
            //ViewBag.dbSuccessComp = 1;
            return View("Index", companies);
        }

        public IActionResult PopulateFinancials()
        {
            // Retrieve the companies that were saved in the symbols method
            List<Financials> financials = JsonConvert.DeserializeObject<List<Financials>>(TempData["Financials"].ToString());

            foreach (Financials financial in financials)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Financials.Where(c => c.totalRevenue.Equals(financial.totalRevenue)).Count() == 0)
                {
                    dbContext.Financials.Add(financial);
                }
            }

            dbContext.SaveChanges();
            //ViewBag.dbSuccessComp = 1;
            return View("Financials", financials);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
