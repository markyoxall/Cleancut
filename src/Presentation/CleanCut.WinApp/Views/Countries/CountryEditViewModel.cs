using System;
using System.Collections.Generic;
using System.Text;

namespace CleanCut.WinApp.Views.Countries;

public class CountryEditViewModel
{
    public Guid? Id { get; set; }
    public string CountryName { get; set; } = string.Empty;

}
