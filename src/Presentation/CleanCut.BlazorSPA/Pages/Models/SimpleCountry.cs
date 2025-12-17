using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanCut.BlazorSPA.Pages.Models
{
    public class SimpleCountry
    {
            public Guid Id { get; set; }
            public string CountryName { get; set; } = string.Empty;
    }
}
