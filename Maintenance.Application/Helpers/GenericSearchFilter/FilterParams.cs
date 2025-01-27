﻿using Maintenance.Application.Helpers.GenericSearchFilter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Application.Helpers.GenericSearchFilter
{
  /// <summary>  
  /// Filter parameters Model Class  
  /// </summary>  
  public class FilterParams
  {
    public string ColumnName { get; set; } = string.Empty;
    public string FilterValue { get; set; } = string.Empty;
    public FilterOptions FilterOption { get; set; } = FilterOptions.Contains;
  }
}
